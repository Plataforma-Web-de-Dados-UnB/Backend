using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class SupersetController(ISupersetService supersetService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("guest-token")]
    public async Task<ActionResult<SupersetGuestTokenResponseDto>> GetGuestToken([FromBody] SupersetGuestTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DashboardId))
            return BadRequest(new { message = "O ID do dashboard é obrigatório." });

        var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
        var user = isAuthenticated ? User! : null;
        var username = isAuthenticated
            ? (user!.FindFirstValue(ClaimTypes.Name) ?? user!.FindFirstValue(ClaimTypes.Email) ?? "usuario")
            : "visitante";
        var firstName = isAuthenticated
            ? (user!.FindFirstValue(ClaimTypes.GivenName) ?? username)
            : "Visitante";
        var lastName = isAuthenticated
            ? (user!.FindFirstValue(ClaimTypes.Surname) ?? "")
            : "UnB Portal";

        var resultado = await supersetService.GetGuestTokenAsync(dto.DashboardId, username, firstName, lastName).ConfigureAwait(false);

        if (!resultado.Success)
            return BadRequest(new { message = resultado.Error });

        return Ok(new SupersetGuestTokenResponseDto { Token = resultado.Data! });
    }

    [Authorize]
    [HttpGet("sso-url")]
    public async Task<ActionResult<SupersetSsoUrlResponseDto>> GetSsoUrl()
    {
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized(new { message = "Usuário não autenticado." });

        var nome =
            User.FindFirstValue("Nome")
            ?? User.FindFirstValue(ClaimTypes.GivenName)
            ?? "";
        var ultimoNome =
            User.FindFirstValue("UltimoNome")
            ?? User.FindFirstValue(ClaimTypes.Surname)
            ?? "";
        var cargo = User.FindFirstValue(ClaimTypes.Role) ?? "";

        var resultado = await supersetService.GetSsoUrlAsync(email, nome, ultimoNome, cargo).ConfigureAwait(false);

        if (!resultado.Success)
            return BadRequest(new { message = resultado.Error });

        return Ok(new SupersetSsoUrlResponseDto { Url = resultado.Data! });
    }
}
