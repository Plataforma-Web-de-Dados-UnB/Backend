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
            {
                return BadRequest(ModelState);
            }

            var usuario = await usuarioService.LoginAsync(usuarioLoginDto).ConfigureAwait(false);

            if (!usuario.Success) return BadRequest(new { message = usuario.Error });

            if (usuario.Data == null) return Unauthorized(new { message = "Erro ao realizar login." });

            return Ok(usuario.Data);
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
