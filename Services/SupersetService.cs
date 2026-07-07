using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using api.Helpers;
using api.Services.Interfaces;
using api.Views;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

public class SupersetService : ISupersetService
{
    private readonly ILogger<SupersetService> _logger;
    private readonly string _guestTokenJwtSecret;

    public SupersetService(IConfiguration configuration, ILogger<SupersetService> logger)
    {
        _logger = logger;
        _guestTokenJwtSecret = configuration["Superset:GuestTokenJwtSecret"] ?? "TROQUE_ESTA_CHAVE_GUEST_TOKEN_EM_PRODUCAO";
    }

    public Task<Resultado<string>> GetGuestTokenAsync(string dashboardId, string username, string firstName, string lastName)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_guestTokenJwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("user", JsonSerializer.Serialize(new SupersetGuestTokenUser
                {
                    Username = username,
                    FirstName = firstName,
                    LastName = lastName
                }), JsonClaimValueTypes.Json),
                new Claim("resources", JsonSerializer.Serialize(new List<SupersetGuestTokenResource>
                {
                    new() { Type = "dashboard", Id = dashboardId }
                }), JsonClaimValueTypes.Json),
                new Claim("rls_rules", JsonSerializer.Serialize(new List<object>()), JsonClaimValueTypes.Json),
                new Claim("type", "guest")
            };

            var token = new JwtSecurityToken(
                issuer: "superset",
                audience: "superset",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Task.FromResult(Resultado<string>.Ok(tokenString));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao gerar guest token do Superset.");
            return Task.FromResult(Resultado<string>.Falha("Erro inesperado ao gerar token de acesso ao dashboard."));
        }
    }
}
