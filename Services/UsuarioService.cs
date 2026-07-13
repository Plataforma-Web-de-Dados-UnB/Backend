using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using api.Data;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Services
{
    public class UsuarioService(UserManager<Usuario> userManager, IConfiguration configuration, AppDbContext context, IEmailService emailService) : IUsuario
    {
        private readonly UserManager<Usuario> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly AppDbContext _context = context;
        private readonly IEmailService _emailService = emailService;

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

        private Resultado<string> GerarAccessToken(Usuario usuario)
        {
            string? chaveJwt = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(chaveJwt))
                return Resultado<string>.Falha("A chave de autenticação está ausente ou vazia.");

            int expiracaoMinutos = int.TryParse(_configuration["Jwt:AccessTokenExpirationMinutes"], out int min) ? min : 15;

            var alegacoes = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id),
                new(ClaimTypes.Name, usuario.Email!),
                new("Nome", usuario.Nome),
                new("UltimoNome", usuario.UltimoNome),
                new("Email", usuario.Email!),
                new(ClaimTypes.Role, usuario.Cargo.ToString())
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveJwt));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: alegacoes,
                expires: DateTime.UtcNow.AddMinutes(expiracaoMinutos),
                signingCredentials: credenciais
            );

            return Resultado<string>.Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

        private async Task<string> GerarRefreshTokenAsync(string usuarioId, string familyId)
        {
            int expiracaoDias = int.TryParse(_configuration["Jwt:RefreshTokenExpirationDays"], out int dias) ? dias : 7;

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            string token = Convert.ToBase64String(tokenBytes);

            var refreshToken = new RefreshToken
            {
                Token = token,
                FamilyId = familyId,
                UsuarioId = usuarioId,
                ExpiresAt = DateTime.UtcNow.AddDays(expiracaoDias),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return token;
        }

        public async Task<Resultado<UsuarioLoginResponseDto>> LoginAsync(UsuarioLoginDto user)
        {
            var usuario = await _userManager.FindByEmailAsync(user.Email).ConfigureAwait(false);

            if (usuario == null) return Resultado<UsuarioLoginResponseDto>.Falha("As credenciais informadas estão incorretas.");

            bool resultado = await _userManager.CheckPasswordAsync(usuario, user.Senha).ConfigureAwait(false);

            if (!resultado) return Resultado<UsuarioLoginResponseDto>.Falha("As credenciais informadas estão incorretas.");

            if (usuario.Status == StatusUsuario.Pendente)
                return Resultado<UsuarioLoginResponseDto>.Falha("Seu cadastro está pendente de aprovação pelo administrador.");

            if (usuario.Status == StatusUsuario.Recusado)
                return Resultado<UsuarioLoginResponseDto>.Falha("Seu acesso foi recusado. Entre em contato com o administrador.");

            var accessTokenResult = GerarAccessToken(usuario);
            if (!accessTokenResult.Success)
                return Resultado<UsuarioLoginResponseDto>.Falha(accessTokenResult.Error!);

            string familyId = Guid.NewGuid().ToString();
            string refreshToken = await GerarRefreshTokenAsync(usuario.Id, familyId).ConfigureAwait(false);

            return Resultado<UsuarioLoginResponseDto>.Ok(
                new UsuarioLoginResponseDto
                {
                    AccessToken = accessTokenResult.Data!,
                    RefreshToken = refreshToken,
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    UltimoNome = usuario.UltimoNome,
                    Email = usuario.Email!,
                    Cargo = usuario.Cargo
                }
            );
        }

        public async Task<Resultado<AuthRefreshResponseDto>> RefreshAsync(string refreshToken)
        {
            var tokenEntry = await _context.RefreshTokens
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Token == refreshToken)
                .ConfigureAwait(false);

            if (tokenEntry == null)
                return Resultado<AuthRefreshResponseDto>.Falha("Refresh token inválido.");

            if (tokenEntry.IsRevoked)
            {
                await RevogarFamiliaAsync(tokenEntry.FamilyId).ConfigureAwait(false);
                return Resultado<AuthRefreshResponseDto>.Falha("Refresh token reutilizado. Sessão encerrada por segurança.");
            }

            if (tokenEntry.IsExpired)
                return Resultado<AuthRefreshResponseDto>.Falha("Refresh token expirado. Faça login novamente.");

            var usuario = tokenEntry.Usuario;

            if (usuario.Status != StatusUsuario.Ativo)
                return Resultado<AuthRefreshResponseDto>.Falha("Acesso não autorizado.");

            tokenEntry.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            var accessTokenResult = GerarAccessToken(usuario);
            if (!accessTokenResult.Success)
                return Resultado<AuthRefreshResponseDto>.Falha(accessTokenResult.Error!);

            string novoRefreshToken = await GerarRefreshTokenAsync(usuario.Id, tokenEntry.FamilyId).ConfigureAwait(false);

            return Resultado<AuthRefreshResponseDto>.Ok(new AuthRefreshResponseDto
            {
                AccessToken = accessTokenResult.Data!,
                RefreshToken = novoRefreshToken
            });
        }

        public async Task<Resultado<string>> LogoutAsync(string refreshToken)
        {
            var tokenEntry = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken)
                .ConfigureAwait(false);

            if (tokenEntry == null || tokenEntry.IsRevoked)
                return Resultado<string>.Ok("Sessão encerrada.");

            tokenEntry.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<string>.Ok("Sessão encerrada com sucesso.");
        }

        private async Task RevogarFamiliaAsync(string familyId)
        {
            var tokensAtivos = await _context.RefreshTokens
                .Where(r => r.FamilyId == familyId && r.RevokedAt == null)
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (var t in tokensAtivos)
                t.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);
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
                var searchPattern = $"%{StringHelper.RemoverAcentos(busca.Trim())}%";
                query = query.Where(u =>
                    EF.Functions.ILike(EF.Functions.Unaccent(u.Nome), searchPattern) ||
                    EF.Functions.ILike(EF.Functions.Unaccent(u.UltimoNome), searchPattern) ||
                    EF.Functions.ILike(EF.Functions.Unaccent(u.Email!), searchPattern));
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
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
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

            var nomeCompleto = $"{usuario.Nome} {usuario.UltimoNome}";
            if (status == StatusUsuario.Ativo)
                _ = _emailService.SendCadastroAprovadoAsync(usuario.Email!, nomeCompleto);
            else if (status == StatusUsuario.Recusado)
                _ = _emailService.SendCadastroRecusadoAsync(usuario.Email!, nomeCompleto);

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

        public async Task<Resultado<string>> DeleteSelfAccountAsync(string userId, string senha)
        {
            var usuario = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (usuario == null) return Resultado<string>.Falha("Usuário não encontrado.");

            bool confirmacao = await _userManager.CheckPasswordAsync(usuario, senha).ConfigureAwait(false);
            if (!confirmacao) return Resultado<string>.Falha("A senha informada está incorreta.");

            if (usuario.Cargo == CargoUsuario.SuperAdministrador)
            {
                return Resultado<string>.Falha("Não é possível excluir a conta de um Super Administrador.");
            }

            var resultado = await _userManager.DeleteAsync(usuario).ConfigureAwait(false);
            if (!resultado.Succeeded)
            {
                return Resultado<string>.Falha(resultado.Errors.FirstOrDefault()?.Description ?? "Erro ao excluir conta.");
            }

            return Resultado<string>.Ok("Conta excluída com sucesso.");
        }

        public async Task<Resultado<string>> DeleteUsuarioAsync(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
            if (usuario == null) return Resultado<string>.Falha("Usuário não encontrado.");

            if (usuario.Cargo == CargoUsuario.SuperAdministrador)
            {
                return Resultado<string>.Falha("Não é possível excluir a conta de um Super Administrador.");
            }

            var resultado = await _userManager.DeleteAsync(usuario).ConfigureAwait(false);
            if (!resultado.Succeeded)
            {
                return Resultado<string>.Falha(resultado.Errors.FirstOrDefault()?.Description ?? "Erro ao excluir usuário.");
            }

            return Resultado<string>.Ok("Usuário excluído com sucesso.");
        }

        public async Task<Resultado<string>> SolicitarRecuperacaoSenhaAsync(string email)
        {
            var usuario = await _userManager.FindByEmailAsync(email).ConfigureAwait(false);

            if (usuario != null && usuario.Status == StatusUsuario.Ativo)
            {
                var tokenEntry = new PasswordResetToken
                {
                    Token = Guid.NewGuid().ToString("N"),
                    UsuarioId = usuario.Id,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    CreatedAt = DateTime.UtcNow
                };

                _context.PasswordResetTokens.Add(tokenEntry);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                var frontendBaseUrl = _configuration["Email:FrontendBaseUrl"] ?? "http://localhost:5173";
                var resetLink = $"{frontendBaseUrl}/redefinir-senha?token={tokenEntry.Token}";
                var nomeCompleto = $"{usuario.Nome} {usuario.UltimoNome}";

                _ = _emailService.SendRecuperacaoSenhaAsync(usuario.Email!, nomeCompleto, resetLink);
            }

            return Resultado<string>.Ok("Se este e-mail estiver cadastrado, você receberá um link para redefinição de senha.");
        }

        public async Task<Resultado<string>> RedefinirSenhaAsync(string token, string novaSenha)
        {
            var tokenEntry = await _context.PasswordResetTokens
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.Token == token)
                .ConfigureAwait(false);

            if (tokenEntry == null || tokenEntry.IsExpired || tokenEntry.IsUsed)
                return Resultado<string>.Falha("Token inválido ou expirado. Solicite um novo link de redefinição.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(tokenEntry.Usuario).ConfigureAwait(false);
            var resultado = await _userManager.ResetPasswordAsync(tokenEntry.Usuario, resetToken, novaSenha).ConfigureAwait(false);

            if (!resultado.Succeeded)
                return Resultado<string>.Falha(resultado.Errors.FirstOrDefault()?.Description ?? "Erro ao redefinir senha.");

            tokenEntry.UsedAt = DateTime.UtcNow;
            tokenEntry.Usuario.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<string>.Ok("Senha redefinida com sucesso. Você já pode fazer login com a nova senha.");
        }
    }
}
