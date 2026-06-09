using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsuarioController(IUsuario usuarioService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUsuario([FromBody] UsuarioRegisterDto usuarioRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await usuarioService.RegisterAsync(usuarioRegisterDto).ConfigureAwait(false);

            if (!resultado.Success)
            {
                return BadRequest(new { message = resultado.Error });
            }

            return Ok(new { message = resultado.Data });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UsuarioLoginResponseDto>> LoginUsuario([FromBody] UsuarioLoginDto usuarioLoginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await usuarioService.LoginAsync(usuarioLoginDto).ConfigureAwait(false);

            if (!resultado.Success) return BadRequest(new { message = resultado.Error });
            if (resultado.Data == null) return Unauthorized(new { message = "Erro ao realizar login." });

            DefinirRefreshTokenCookie(resultado.Data.RefreshToken);

            return Ok(new
            {
                resultado.Data.AccessToken,
                resultado.Data.Id,
                resultado.Data.Nome,
                resultado.Data.UltimoNome,
                resultado.Data.Email,
                resultado.Data.Cargo
            });
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthRefreshResponseDto>> Refresh()
        {
            string? refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(new { message = "Refresh token ausente." });

            var resultado = await usuarioService.RefreshAsync(refreshToken).ConfigureAwait(false);

            if (!resultado.Success)
            {
                RemoverRefreshTokenCookie();
                return Unauthorized(new { message = resultado.Error });
            }

            DefinirRefreshTokenCookie(resultado.Data!.RefreshToken);

            return Ok(new { resultado.Data.AccessToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrWhiteSpace(refreshToken))
                await usuarioService.LogoutAsync(refreshToken).ConfigureAwait(false);

            RemoverRefreshTokenCookie();

            return Ok(new { message = "Sessão encerrada com sucesso." });
        }

        private void DefinirRefreshTokenCookie(string token)
        {
            var opcoes = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, opcoes);
        }

        private void RemoverRefreshTokenCookie()
        {
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }

        [Authorize]
        [HttpGet("perfil")]
        public async Task<ActionResult<UsuarioGetDto>> GetPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var perfil = await usuarioService.GetPerfilAsync(userId).ConfigureAwait(false);

            if (!perfil.Success)
            {
                return NotFound(new { message = perfil.Error });
            }

            return Ok(perfil.Data);
        }

        [Authorize]
        [HttpPut("senha")]
        public async Task<IActionResult> ChangePassword([FromBody] UsuarioChangePasswordDto passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuário não autenticado." });
            }

            var resultado = await usuarioService.ChangePasswordAsync(userId, passwordDto).ConfigureAwait(false);

            if (!resultado.Success)
            {
                return BadRequest(new { message = resultado.Error });
            }

            return Ok(new { message = resultado.Data });
        }
    }
}
