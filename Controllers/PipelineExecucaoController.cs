using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers
{
    [ApiController]
    [Route("pipeline/execucoes")]
    [Authorize(Roles = "SuperAdministrador,Administrador")]
    public class PipelineExecucaoController(IPipelineExecucao pipelineExecucaoService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResultadoPaginado<PipelineExecucaoListDto>>> GetExecucoes(
            [FromQuery] int? pipelineId,
            [FromQuery] StatusPipelineExecucao? status,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await pipelineExecucaoService.GetExecucoesAsync(pipelineId, status, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PipelineExecucaoGetDto>> GetExecucao(int id)
        {
            var resultado = await pipelineExecucaoService.GetExecucaoByIdAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [HttpPost("executar")]
        public async Task<ActionResult<PipelineExecucaoGetDto>> Executar([FromBody] PipelineExecucaoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await pipelineExecucaoService.ExecutarAsync(dto).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [HttpPost("{id}/rollback")]
        public async Task<IActionResult> Rollback(int id)
        {
            var resultado = await pipelineExecucaoService.RollbackAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<UploadPreviewDto>> Upload([FromForm] UploadRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var uploadedBy = User.FindFirstValue(ClaimTypes.Name) ?? "desconhecido";
            var resultado = await pipelineExecucaoService.ProcessarUploadAsync(dto.Arquivo, uploadedBy).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(resultado.Data);
        }
    }
}
