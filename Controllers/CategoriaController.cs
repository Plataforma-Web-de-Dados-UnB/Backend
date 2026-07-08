using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Helpers;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoriaController(ICategoria categoriaService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private string GetBaseUrl()
        {
            var request = httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;
            return $"{request.Scheme}://{request.Host}";
        }

        [HttpGet]
        public async Task<ActionResult<ResultadoPaginado<CategoriaListDto>>> GetCategorias(
            [FromQuery] string? busca,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await categoriaService.GetCategoriasAsync(true, busca, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaGetDto>> GetCategoria(int id)
        {
            bool isUserAdmin = User.IsInRole("SuperAdministrador") || User.IsInRole("Administrador");
            var resultado = await categoriaService.GetCategoriaByIdAsync(id, apenasAtiva: !isUserAdmin).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpGet("admin")]
        public async Task<ActionResult<ResultadoPaginado<CategoriaListDto>>> GetCategoriasAdmin(
            [FromQuery] bool? active,
            [FromQuery] string? busca,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var resultado = await categoriaService.GetCategoriasAsync(active, busca, page, limit).ConfigureAwait(false);
            return Ok(resultado);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpGet("admin/{id}")]
        public async Task<ActionResult<CategoriaGetDto>> GetCategoriaAdmin(int id)
        {
            var resultado = await categoriaService.GetCategoriaByIdAsync(id, apenasAtiva: false).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CategoriaGetDto>> CreateCategoria([FromForm] CategoriaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await categoriaService.CreateCategoriaAsync(dto, GetBaseUrl()).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return CreatedAtAction(nameof(GetCategoria), new { id = resultado.Data!.Id }, resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CategoriaGetDto>> UpdateCategoria(int id, [FromForm] CategoriaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await categoriaService.UpdateCategoriaAsync(id, dto, GetBaseUrl()).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(resultado.Data);
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id, [FromQuery] bool hardDelete = true)
        {
            var resultado = await categoriaService.DeleteCategoriaAsync(id, hardDelete).ConfigureAwait(false);

            if (!resultado.Success)
                return BadRequest(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }

        [Authorize(Roles = "SuperAdministrador,Administrador")]
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var resultado = await categoriaService.ToggleActiveAsync(id).ConfigureAwait(false);

            if (!resultado.Success)
                return NotFound(new { message = resultado.Error });

            return Ok(new { message = resultado.Data });
        }
    }
}
