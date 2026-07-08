using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PainelController(IPainel painelService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginado<PainelListDto>>> GetPaineis(
            [FromQuery] int? categoriaId,
            [FromQuery] string? busca,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await painelService.GetPaineisAsync(true, categoriaId, busca, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PainelGetDto>> GetPainel(int id)
        {
            bool isUserAdmin = User.IsInRole("SuperAdministrador") || User.IsInRole("Administrador");
            var resultado = await painelService.GetPainelByIdAsync(id, apenasAtivo: !isUserAdmin).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [HttpGet("busca")]
        public async Task<ActionResult<List<PainelBuscaDto>>> Buscar(
            [FromQuery] string q,
            [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "O parâmetro 'q' é obrigatório." });

            if (limit < 1 || limit > 50) limit = 10;

            var resultado = await painelService.BuscarAsync(q, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpGet("admin")]
        public async Task<ActionResult<ResultadoPaginado<PainelListDto>>> GetPaineisAdmin(
            [FromQuery] bool? active,
            [FromQuery] int? categoriaId,
            [FromQuery] string? busca,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await painelService.GetPaineisAsync(active, categoriaId, busca, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpGet("admin/{id}")]
        public async Task<ActionResult<PainelGetDto>> GetPainelAdmin(int id)
        {
            var resultado = await painelService.GetPainelByIdAsync(id, apenasAtivo: false).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPost]
        public async Task<ActionResult<PainelGetDto>> CreatePainel([FromBody] PainelCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await painelService.CreatePainelAsync(dto).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return CreatedAtAction(nameof(GetPainel), new { id = resultado.Data!.Id }, resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PainelGetDto>> UpdatePainel(int id, [FromBody] PainelUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await painelService.UpdatePainelAsync(id, dto).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePainel(int id, [FromQuery] bool hardDelete = true)
        {
            var resultado = await painelService.DeletePainelAsync(id, hardDelete).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var resultado = await painelService.ToggleActiveAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }
    }
}
