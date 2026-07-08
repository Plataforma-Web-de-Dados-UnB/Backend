using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Services
{
    public class PainelService(AppDbContext context) : IPainel
    {
        private readonly AppDbContext _context = context;

        private static PainelGetDto ToGetDto(Painel p) => new()
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            GraphEmbedLink = p.GraphEmbedLink,
            EmbedDashboardUuid = p.EmbedDashboardUuid,
            SortOrdem = p.SortOrdem,
            Active = p.Active,
            DeactivatedAt = p.DeactivatedAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CategoriaId = p.CategoriaId,
            CategoriaNome = p.Categoria?.Nome ?? string.Empty
        };

        private static PainelListDto ToListDto(Painel p) => new()
        {
            Id = p.Id,
            Nome = p.Nome,
            Descricao = p.Descricao,
            GraphEmbedLink = p.GraphEmbedLink,
            EmbedDashboardUuid = p.EmbedDashboardUuid,
            SortOrdem = p.SortOrdem,
            Active = p.Active,
            CreatedAt = p.CreatedAt,
            CategoriaId = p.CategoriaId,
            CategoriaNome = p.Categoria?.Nome ?? string.Empty
        };

        public async Task<ResultadoPaginado<PainelListDto>> GetPaineisAsync(bool? active, int? categoriaId, string? busca, int page, int limit)
        {
            var query = _context.Paineis.Include(p => p.Categoria).AsQueryable();

            if (active.HasValue)
                query = query.Where(p => p.Active == active.Value);

            if (categoriaId.HasValue)
                query = query.Where(p => p.CategoriaId == categoriaId.Value);

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(p => p.Nome.Contains(busca) || (p.Descricao != null && p.Descricao.Contains(busca)));

            query = query.OrderBy(p => p.SortOrdem).ThenBy(p => p.Nome);

            int totalItens = await query.CountAsync().ConfigureAwait(false);

            var itens = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync()
                .ConfigureAwait(false);

            return ResultadoPaginado<PainelListDto>.Ok(page, limit, totalItens, itens.Select(ToListDto).ToList());
        }

        public async Task<Resultado<PainelGetDto>> GetPainelByIdAsync(int id, bool apenasAtivo = false)
        {
            var query = _context.Paineis.Include(p => p.Categoria).AsQueryable();

            if (apenasAtivo)
                query = query.Where(p => p.Active);

            var painel = await query.FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);

            if (painel == null)
                return Resultado<PainelGetDto>.Falha("Painel não encontrado.");

            return Resultado<PainelGetDto>.Ok(ToGetDto(painel));
        }

        public async Task<Resultado<PainelGetDto>> CreatePainelAsync(PainelCreateDto dto)
        {
            bool categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId).ConfigureAwait(false);
            if (!categoriaExiste)
                return Resultado<PainelGetDto>.Falha("Categoria não encontrada.");

            var painel = new Painel
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                GraphEmbedLink = dto.GraphEmbedLink,
                EmbedDashboardUuid = dto.EmbedDashboardUuid,
                SortOrdem = dto.SortOrdem,
                CategoriaId = dto.CategoriaId,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Paineis.Add(painel);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            await _context.Entry(painel).Reference(p => p.Categoria).LoadAsync().ConfigureAwait(false);

            return Resultado<PainelGetDto>.Ok(ToGetDto(painel));
        }

        public async Task<Resultado<PainelGetDto>> UpdatePainelAsync(int id, PainelUpdateDto dto)
        {
            var painel = await _context.Paineis.Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);

            if (painel == null)
                return Resultado<PainelGetDto>.Falha("Painel não encontrado.");

            if (dto.CategoriaId != painel.CategoriaId)
            {
                bool categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId).ConfigureAwait(false);
                if (!categoriaExiste)
                    return Resultado<PainelGetDto>.Falha("Categoria não encontrada.");
            }

            painel.Nome = dto.Nome;
            painel.Descricao = dto.Descricao;
            painel.GraphEmbedLink = dto.GraphEmbedLink;
            painel.EmbedDashboardUuid = dto.EmbedDashboardUuid;
            painel.SortOrdem = dto.SortOrdem;
            painel.CategoriaId = dto.CategoriaId;
            painel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            await _context.Entry(painel).Reference(p => p.Categoria).LoadAsync().ConfigureAwait(false);

            return Resultado<PainelGetDto>.Ok(ToGetDto(painel));
        }

        public async Task<Resultado<string>> DeletePainelAsync(int id, bool hardDelete = false)
        {
            var painel = await _context.Paineis.FindAsync(id).ConfigureAwait(false);

            if (painel == null)
                return Resultado<string>.Falha("Painel não encontrado.");

            if (hardDelete)
            {
                _context.Paineis.Remove(painel);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                return Resultado<string>.Ok("Painel excluído permanentemente com sucesso.");
            }
            else
            {
                painel.Active = false;
                painel.DeactivatedAt = DateTime.UtcNow;
                painel.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync().ConfigureAwait(false);

                return Resultado<string>.Ok("Painel desativado com sucesso.");
            }
        }

        public async Task<List<PainelBuscaDto>> BuscarAsync(string q, int limit)
        {
            return await _context.Paineis
                .Include(p => p.Categoria)
                .Where(p => p.Active &&
                    (p.Nome.Contains(q) || (p.Descricao != null && p.Descricao.Contains(q))))
                .OrderBy(p => p.SortOrdem)
                .Take(limit)
                .Select(p => new PainelBuscaDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Descricao = p.Descricao,
                    CategoriaId = p.CategoriaId,
                    CategoriaNome = p.Categoria.Nome
                })
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Resultado<string>> ToggleActiveAsync(int id)
        {
            var painel = await _context.Paineis.FindAsync(id).ConfigureAwait(false);

            if (painel == null)
                return Resultado<string>.Falha("Painel não encontrado.");

            painel.Active = !painel.Active;
            painel.DeactivatedAt = painel.Active ? null : DateTime.UtcNow;
            painel.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            string msg = painel.Active ? "Painel ativado com sucesso." : "Painel desativado com sucesso.";
            return Resultado<string>.Ok(msg);
        }
    }
}
