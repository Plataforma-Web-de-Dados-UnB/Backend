using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Services
{
    public class PipelineService(AppDbContext context) : IPipeline
    {
        private readonly AppDbContext _context = context;

        public async Task<ResultadoPaginado<PipelineListDto>> GetPipelinesAsync(string? busca, int page, int limit)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var query = _context.Pipelines.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(p => p.Nome.Contains(busca) || (p.Descricao != null && p.Descricao.Contains(busca)));

            var total = await query.CountAsync().ConfigureAwait(false);

            var items = await query
                .OrderBy(p => p.Nome)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(p => new PipelineListDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Descricao = p.Descricao,
                    Ativo = p.Ativo,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return ResultadoPaginado<PipelineListDto>.Ok(page, limit, total, items);
        }

        public async Task<Resultado<PipelineGetDto>> GetPipelineByIdAsync(int id)
        {
            var pipeline = await _context.Pipelines
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);

            if (pipeline == null)
                return Resultado<PipelineGetDto>.Falha("Pipeline não encontrada.");

            return Resultado<PipelineGetDto>.Ok(MapearGetDto(pipeline));
        }

        public async Task<Resultado<PipelineGetDto>> CreatePipelineAsync(PipelineCreateDto dto)
        {
            var pipeline = new Pipeline
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                ScriptPython = dto.ScriptPython,
                Ativo = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Pipelines.Add(pipeline);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<PipelineGetDto>.Ok(MapearGetDto(pipeline));
        }

        public async Task<Resultado<PipelineGetDto>> UpdatePipelineAsync(int id, PipelineUpdateDto dto)
        {
            var pipeline = await _context.Pipelines
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);

            if (pipeline == null)
                return Resultado<PipelineGetDto>.Falha("Pipeline não encontrada.");

            pipeline.Nome = dto.Nome;
            pipeline.Descricao = dto.Descricao;
            pipeline.ScriptPython = dto.ScriptPython;
            pipeline.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<PipelineGetDto>.Ok(MapearGetDto(pipeline));
        }

        public async Task<Resultado<string>> DeletePipelineAsync(int id)
        {
            var pipeline = await _context.Pipelines
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);

            if (pipeline == null)
                return Resultado<string>.Falha("Pipeline não encontrada.");

            pipeline.Ativo = false;
            pipeline.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<string>.Ok("Pipeline inativada com sucesso.");
        }

        private static PipelineGetDto MapearGetDto(Pipeline pipeline) => new()
        {
            Id = pipeline.Id,
            Nome = pipeline.Nome,
            Descricao = pipeline.Descricao,
            ScriptPython = pipeline.ScriptPython,
            Ativo = pipeline.Ativo,
            CreatedAt = pipeline.CreatedAt,
            UpdatedAt = pipeline.UpdatedAt
        };
    }
}
