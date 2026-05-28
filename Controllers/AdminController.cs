using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "SuperAdministrador")]
    public class AdminController(IUsuario usuarioService) : ControllerBase
    {
        [HttpGet("usuarios")]
        public async Task<ActionResult<ResultadoPaginado<UsuarioListDto>>> GetUsuarios(
            [FromQuery] StatusUsuario? status,
            [FromQuery] CargoUsuario? cargo,
            [FromQuery] string? busca,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await usuarioService.GetUsuariosAsync(status, cargo, busca, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [HttpGet("usuarios/{id}")]
        public async Task<ActionResult<UsuarioGetDto>> GetUsuarioById(string id)
        {
            var usuario = await usuarioService.GetUsuarioByIdAsync(id).ConfigureAwait(false);

            if (!usuario.Success)
            {
                return NotFound(new { message = usuario.Error });
            }

            return Ok(usuario.Data);
        }

        [HttpPut("usuarios/{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UsuarioUpdateStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await usuarioService.UpdateStatusAsync(id, statusDto.Status).ConfigureAwait(false);

            if (!resultado.Success)
            {
                return BadRequest(new { message = resultado.Error });
            }

            return Ok(new { message = resultado.Data });
        }
    }
}
