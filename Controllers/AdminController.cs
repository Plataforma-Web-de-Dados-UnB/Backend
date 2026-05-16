using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<List<UsuarioListDto>>> GetUsuarios(
            [FromQuery] StatusUsuario? status,
            [FromQuery] string? busca)
        {
            var usuarios = await usuarioService.GetUsuariosAsync(status, busca).ConfigureAwait(false);
            return Ok(usuarios);
        }

        [HttpGet("usuarios/{id}")]
        public async Task<ActionResult<UsuarioGetDto>> GetUsuarioById(string id)
        {
            var usuario = await usuarioService.GetUsuarioByIdAsync(id).ConfigureAwait(false);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            return Ok(usuario);
        }

        [HttpPut("usuarios/{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UsuarioUpdateStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? erro = await usuarioService.UpdateStatusAsync(id, statusDto.Status).ConfigureAwait(false);

            if (erro != null)
            {
                return BadRequest(new { message = erro });
            }

            string mensagem = statusDto.Status switch
            {
                StatusUsuario.Ativo => "Usuário aprovado com sucesso.",
                StatusUsuario.Recusado => "Usuário recusado com sucesso.",
                StatusUsuario.Pendente => "Status do usuário alterado para pendente.",
                _ => "Status atualizado com sucesso."
            };

            return Ok(new { message = mensagem });
        }
    }
}
