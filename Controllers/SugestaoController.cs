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
    public class SugestaoController(ISugestao sugestaoService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateSugestao([FromBody] SugestaoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await sugestaoService.CreateSugestaoAsync(dto).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginado<SugestaoListDto>>> GetSugestoes(
            [FromQuery] StatusSugestao? status,
            [FromQuery] TipoSugestao? tipo,
            [FromQuery] string? busca,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await sugestaoService.GetSugestoesAsync(status, tipo, busca, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SugestaoGetDto>> GetSugestao(int id)
        {
            var resultado = await sugestaoService.GetSugestaoByIdAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] SugestaoUpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await sugestaoService.UpdateStatusAsync(id, dto).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSugestao(int id)
        {
            var resultado = await sugestaoService.DeleteSugestaoAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }
    }
}
