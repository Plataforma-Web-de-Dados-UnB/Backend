using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Services
{
    public class CategoriaService(AppDbContext context, IWebHostEnvironment env) : ICategoria
    {
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;

        private static CategoriaGetDto ToGetDto(Categoria c, int quantidadePaineis, string baseUrl) => new()
        {
            Id = c.Id,
            Nome = c.Nome,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemPath != null ? $"{baseUrl}/{c.ImagemPath}" : null,
            SortOrdem = c.SortOrdem,
            Active = c.Active,
            DeactivatedAt = c.DeactivatedAt,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            QuantidadePaineis = quantidadePaineis
        };

        private static CategoriaListDto ToListDto(Categoria c, int quantidadePaineis, string baseUrl) => new()
        {
            Id = c.Id,
            Nome = c.Nome,
            Descricao = c.Descricao,
            ImagemUrl = c.ImagemPath != null ? $"{baseUrl}/{c.ImagemPath}" : null,
            SortOrdem = c.SortOrdem,
            Active = c.Active,
            CreatedAt = c.CreatedAt,
            QuantidadePaineis = quantidadePaineis
        };

        public async Task<ResultadoPaginado<CategoriaListDto>> GetCategoriasAsync(bool? active, string? busca, int page, int limit)
        {
            var query = _context.Categorias.AsQueryable();

            if (active.HasValue)
                query = query.Where(c => c.Active == active.Value);

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(c => c.Nome.Contains(busca) || (c.Descricao != null && c.Descricao.Contains(busca)));

            query = query.OrderBy(c => c.SortOrdem).ThenBy(c => c.Nome);

            int totalItens = await query.CountAsync().ConfigureAwait(false);

            var itens = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Include(c => c.Paineis)
                .ToListAsync()
                .ConfigureAwait(false);

            var dtos = itens.Select(c => ToListDto(c, c.Paineis.Count(p => p.Active), string.Empty)).ToList();

            return ResultadoPaginado<CategoriaListDto>.Ok(page, limit, totalItens, dtos);
        }

        public async Task<Resultado<CategoriaGetDto>> GetCategoriaByIdAsync(int id, bool apenasAtiva = false)
        {
            var query = _context.Categorias.Include(c => c.Paineis).AsQueryable();

            if (apenasAtiva)
                query = query.Where(c => c.Active);

            var categoria = await query.FirstOrDefaultAsync(c => c.Id == id).ConfigureAwait(false);

            if (categoria == null)
                return Resultado<CategoriaGetDto>.Falha("Categoria não encontrada.");

            int qtd = categoria.Paineis.Count(p => p.Active);
            return Resultado<CategoriaGetDto>.Ok(ToGetDto(categoria, qtd, string.Empty));
        }

        public async Task<Resultado<CategoriaGetDto>> CreateCategoriaAsync(CategoriaCreateDto dto, string baseUrl)
        {
            string? imagemPath = null;

            if (dto.Imagem != null)
            {
                imagemPath = await SalvarImagemAsync(dto.Imagem).ConfigureAwait(false);
            }

            var categoria = new Categoria
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                ImagemPath = imagemPath,
                SortOrdem = dto.SortOrdem,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<CategoriaGetDto>.Ok(ToGetDto(categoria, 0, baseUrl));
        }

        public async Task<Resultado<CategoriaGetDto>> UpdateCategoriaAsync(int id, CategoriaUpdateDto dto, string baseUrl)
        {
            var categoria = await _context.Categorias.Include(c => c.Paineis)
                .FirstOrDefaultAsync(c => c.Id == id).ConfigureAwait(false);

            if (categoria == null)
                return Resultado<CategoriaGetDto>.Falha("Categoria não encontrada.");

            if (dto.Imagem != null)
            {
                DeletarImagem(categoria.ImagemPath);
                categoria.ImagemPath = await SalvarImagemAsync(dto.Imagem).ConfigureAwait(false);
            }

            categoria.Nome = dto.Nome;
            categoria.Descricao = dto.Descricao;
            categoria.SortOrdem = dto.SortOrdem;
            categoria.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            int qtd = categoria.Paineis.Count(p => p.Active);
            return Resultado<CategoriaGetDto>.Ok(ToGetDto(categoria, qtd, baseUrl));
        }

        public async Task<Resultado<string>> DeleteCategoriaAsync(int id, bool hardDelete = false)
        {
            var categoria = await _context.Categorias.Include(c => c.Paineis)
                .FirstOrDefaultAsync(c => c.Id == id).ConfigureAwait(false);

            if (categoria == null)
                return Resultado<string>.Falha("Categoria não encontrada.");

            if (hardDelete)
            {
                if (categoria.Paineis.Count != 0)
                    return Resultado<string>.Falha("Não é possível excluir permanentemente uma categoria que possui painéis vinculados.");

                DeletarImagem(categoria.ImagemPath);
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync().ConfigureAwait(false);

                return Resultado<string>.Ok("Categoria excluída permanentemente com sucesso.");
            }
            else
            {
                categoria.Active = false;
                categoria.DeactivatedAt = DateTime.UtcNow;
                categoria.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync().ConfigureAwait(false);

                return Resultado<string>.Ok("Categoria desativada com sucesso.");
            }
        }

        public async Task<Resultado<string>> ToggleActiveAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id).ConfigureAwait(false);

            if (categoria == null)
                return Resultado<string>.Falha("Categoria não encontrada.");

            categoria.Active = !categoria.Active;
            categoria.DeactivatedAt = categoria.Active ? null : DateTime.UtcNow;
            categoria.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            string msg = categoria.Active ? "Categoria ativada com sucesso." : "Categoria desativada com sucesso.";
            return Resultado<string>.Ok(msg);
        }

        private async Task<string> SalvarImagemAsync(IFormFile imagem)
        {
            string uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "categorias");
            Directory.CreateDirectory(uploadsDir);

            string extensao = Path.GetExtension(imagem.FileName);
            string nomeArquivo = $"{Guid.NewGuid()}{extensao}";
            string caminhoCompleto = Path.Combine(uploadsDir, nomeArquivo);

            using var stream = new FileStream(caminhoCompleto, FileMode.Create);
            await imagem.CopyToAsync(stream).ConfigureAwait(false);

            return $"uploads/categorias/{nomeArquivo}";
        }

        private void DeletarImagem(string? imagemPath)
        {
            if (string.IsNullOrWhiteSpace(imagemPath)) return;

            string caminhoCompleto = Path.Combine(_env.WebRootPath, imagemPath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(caminhoCompleto))
                File.Delete(caminhoCompleto);
        }
    }
}
