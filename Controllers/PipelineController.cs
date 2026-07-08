using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "SuperAdministrador,Administrador")]
    public class PipelineController(IPipeline pipelineService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginado<PipelineListDto>>> GetPipelines(
            [FromQuery] string? busca,
            [FromQuery] bool? ativo,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await pipelineService.GetPipelinesAsync(busca, ativo, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PipelineGetDto>> GetPipeline(int id)
        {
            var resultado = await pipelineService.GetPipelineByIdAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [HttpPost]
        public async Task<ActionResult<PipelineGetDto>> CreatePipeline([FromBody] PipelineCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await pipelineService.CreatePipelineAsync(dto).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return CreatedAtAction(nameof(GetPipeline), new { id = resultado.Data!.Id }, resultado.Data);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PipelineGetDto>> UpdatePipeline(int id, [FromBody] PipelineUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await pipelineService.UpdatePipelineAsync(id, dto).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePipeline(int id, [FromQuery] bool hardDelete = false)
        {
            var resultado = await pipelineService.DeletePipelineAsync(id, hardDelete).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var resultado = await pipelineService.ToggleActiveAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }
    }
}
