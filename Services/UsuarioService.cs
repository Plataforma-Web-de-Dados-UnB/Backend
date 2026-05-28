using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Services
{
    public class UsuarioService(UserManager<Usuario> userManager, IConfiguration configuration) : IUsuario
    {
        private readonly UserManager<Usuario> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;

        public async Task<Resultado<string>> RegisterAsync(UsuarioRegisterDto user)
        {
            var usuario = new Usuario
            {
                UserName = user.Email,
                Email = user.Email,
                Nome = user.Nome,
                UltimoNome = user.UltimoNome,
                Cargo = CargoUsuario.Administrador,
                Status = StatusUsuario.Pendente,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(usuario, user.Senha).ConfigureAwait(false);

            if (!resultado.Succeeded)
            {
                return Resultado<string>.Falha(resultado.Errors.FirstOrDefault()?.Description ?? "Erro desconhecido ao registrar usuário.");
            }

            return Resultado<string>.Ok("Cadastro realizado com sucesso. Aguarde a aprovação do administrador.");
        }

        public async Task<Resultado<UsuarioLoginResponseDto>> LoginAsync(UsuarioLoginDto user)
        {
            var usuario = await _userManager.FindByEmailAsync(user.Email).ConfigureAwait(false);

            if (usuario == null) return Resultado<UsuarioLoginResponseDto>.Falha("Email não encontrado.");

            bool resultado = await _userManager.CheckPasswordAsync(usuario, user.Senha).ConfigureAwait(false);

            if (!resultado) return Resultado<UsuarioLoginResponseDto>.Falha("Senha incorreta.");

            if (usuario.Status == StatusUsuario.Pendente)
            {
                return Resultado<UsuarioLoginResponseDto>.Falha("Seu cadastro está pendente de aprovação pelo administrador.");
            }

            if (usuario.Status == StatusUsuario.Recusado)
            {
                return Resultado<UsuarioLoginResponseDto>.Falha("Seu acesso foi recusado. Entre em contato com o administrador.");
            }

            var alegacoes = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Name, usuario.Email!),
                new Claim("Nome", usuario.Nome),
                new Claim("UltimoNome", usuario.UltimoNome),
                new Claim("Email", usuario.Email!),
                new Claim(ClaimTypes.Role, usuario.Cargo.ToString())
            };

            string? chaveJwt = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(chaveJwt)) return Resultado<UsuarioLoginResponseDto>.Falha("A chave de autenticação está ausente ou vazia.");

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt));

            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: alegacoes,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credenciais
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Resultado<UsuarioLoginResponseDto>.Ok(
                new UsuarioLoginResponseDto
                {
                    Token = tokenString,
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    UltimoNome = usuario.UltimoNome,
                    Email = usuario.Email!,
                    Cargo = usuario.Cargo
                }
            );
        }

        public async Task<Resultado<UsuarioGetDto>> GetUsuarioByIdAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id).ConfigureAwait(false);

            if (usuario == null) return Resultado<UsuarioGetDto>.Falha("Usuário não encontrado.");

            return Resultado<UsuarioGetDto>.Ok(new UsuarioGetDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                UltimoNome = usuario.UltimoNome,
                Email = usuario.Email!,
                Cargo = usuario.Cargo,
                Status = usuario.Status,
                CreatedAt = usuario.CreatedAt,
                UpdatedAt = usuario.UpdatedAt
            });
        }

        public async Task<Resultado<UsuarioGetDto>> GetPerfilAsync(string userId)
        {
            return await GetUsuarioByIdAsync(userId).ConfigureAwait(false);
        }

        public async Task<ResultadoPaginado<UsuarioListDto>> GetUsuariosAsync(StatusUsuario? status, CargoUsuario? cargo, string? busca, int page, int limit)
        {
            var query = _userManager.Users.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            if (cargo.HasValue)
            {
                query = query.Where(u => u.Cargo == cargo.Value);
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                query = query.Where(u =>
                    u.Nome.Contains(busca) ||
                    u.UltimoNome.Contains(busca) ||
                    u.Email!.Contains(busca));
            }

            query = query.OrderByDescending(u => u.CreatedAt);

            int totalItens = await query.CountAsync().ConfigureAwait(false);

            var itens = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync()
                .ConfigureAwait(false);

            return ResultadoPaginado<UsuarioListDto>.Ok(page, limit, totalItens,
                itens.Select(u => new UsuarioListDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    UltimoNome = u.UltimoNome,
                    Email = u.Email!,
                    Cargo = u.Cargo,
                    Status = u.Status,
                    CreatedAt = u.CreatedAt
                }).ToList());
        }

        public async Task<Resultado<string>> UpdateStatusAsync(string id, StatusUsuario status)
        {
            var usuario = await _userManager.FindByIdAsync(id).ConfigureAwait(false);

            if (usuario == null) return Resultado<string>.Falha("Usuário não encontrado.");

            if (usuario.Cargo == CargoUsuario.SuperAdministrador)
            {
                return Resultado<string>.Falha("Não é possível alterar o status de um Super Administrador.");
            }

            usuario.Status = status;
            usuario.UpdatedAt = DateTime.UtcNow;

            var resultado = await _userManager.UpdateAsync(usuario).ConfigureAwait(false);

            if (!resultado.Succeeded)
            {
                return Resultado<string>.Falha(resultado.Errors.FirstOrDefault()?.Description ?? "Erro ao atualizar status do usuário.");
            }

            return Resultado<string>.Ok("Status atualizado com sucesso.");
        }

        public async Task<Resultado<string>> ChangePasswordAsync(string userId, UsuarioChangePasswordDto passwordDto)
        {
            var usuario = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (usuario == null) return Resultado<string>.Falha("Usuário não encontrado.");

            var resultado = await _userManager.ChangePasswordAsync(usuario, passwordDto.SenhaAntiga, passwordDto.SenhaNova).ConfigureAwait(false);

            if (!resultado.Succeeded)
            {
                return Resultado<string>.Falha(resultado.Errors.FirstOrDefault()?.Description ?? "Erro ao alterar senha.");
            }

            usuario.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(usuario).ConfigureAwait(false);

            return Resultado<string>.Ok("Senha alterada com sucesso.");
        }
    }
}
