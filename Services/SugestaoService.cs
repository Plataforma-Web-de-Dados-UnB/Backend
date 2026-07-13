using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Services
{
    public class SugestaoService(AppDbContext context, IEmailService emailService) : ISugestao
    {
        private readonly AppDbContext _context = context;
        private readonly IEmailService _emailService = emailService;

        private static SugestaoGetDto ToGetDto(Sugestao s) => new()
        {
            Id = s.Id,
            Tipo = s.Tipo,
            Titulo = s.Titulo,
            Descricao = s.Descricao,
            NomeContato = s.NomeContato,
            EmailContato = s.EmailContato,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };

        private static SugestaoListDto ToListDto(Sugestao s) => new()
        {
            Id = s.Id,
            Tipo = s.Tipo,
            Titulo = s.Titulo,
            Descricao = s.Descricao,
            NomeContato = s.NomeContato,
            EmailContato = s.EmailContato,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };

        public async Task<Resultado<string>> CreateSugestaoAsync(SugestaoCreateDto dto)
        {
            var sugestao = new Sugestao
            {
                Tipo = dto.Tipo,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                NomeContato = dto.NomeContato,
                EmailContato = dto.EmailContato,
                Status = StatusSugestao.Pendente,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Sugestoes.Add(sugestao);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<string>.Ok("Sugestão enviada com sucesso. Obrigado pelo seu contato!");
        }

        public async Task<ResultadoPaginado<SugestaoListDto>> GetSugestoesAsync(StatusSugestao? status, TipoSugestao? tipo, string? busca, int page, int limit)
        {
            var query = _context.Sugestoes.AsQueryable();

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (tipo.HasValue)
                query = query.Where(s => s.Tipo == tipo.Value);

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var searchPattern = $"%{StringHelper.RemoverAcentos(busca.Trim())}%";
                query = query.Where(s =>
                    EF.Functions.ILike(EF.Functions.Unaccent(s.Titulo), searchPattern) ||
                    EF.Functions.ILike(EF.Functions.Unaccent(s.Descricao), searchPattern) ||
                    (s.NomeContato != null && EF.Functions.ILike(EF.Functions.Unaccent(s.NomeContato), searchPattern)) ||
                    (s.EmailContato != null && EF.Functions.ILike(EF.Functions.Unaccent(s.EmailContato), searchPattern)));
            }

            query = query.OrderByDescending(s => s.CreatedAt);

            int totalItens = await query.CountAsync().ConfigureAwait(false);

            var itens = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync()
                .ConfigureAwait(false);

            return ResultadoPaginado<SugestaoListDto>.Ok(page, limit, totalItens, itens.Select(ToListDto).ToList());
        }

        public async Task<Resultado<SugestaoGetDto>> GetSugestaoByIdAsync(int id)
        {
            var sugestao = await _context.Sugestoes.FindAsync(id).ConfigureAwait(false);

            if (sugestao == null)
                return Resultado<SugestaoGetDto>.Falha("Sugestão não encontrada.");

            return Resultado<SugestaoGetDto>.Ok(ToGetDto(sugestao));
        }

        public async Task<Resultado<string>> UpdateStatusAsync(int id, SugestaoUpdateStatusDto dto)
        {
            var sugestao = await _context.Sugestoes.FindAsync(id).ConfigureAwait(false);

            if (sugestao == null)
                return Resultado<string>.Falha("Sugestão não encontrada.");

            sugestao.Status = dto.Status;
            sugestao.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(sugestao.EmailContato) && dto.Status != StatusSugestao.Pendente)
            {
                var nome = sugestao.NomeContato ?? "Prezado(a)";
                _ = _emailService.SendSugestaoAtualizadaAsync(sugestao.EmailContato, nome, sugestao.Titulo, dto.Status);
            }

            return Resultado<string>.Ok("Status da sugestão atualizado com sucesso.");
        }

        public async Task<Resultado<string>> DeleteSugestaoAsync(int id)
        {
            var sugestao = await _context.Sugestoes.FindAsync(id).ConfigureAwait(false);

            if (sugestao == null)
                return Resultado<string>.Falha("Sugestão não encontrada.");

            _context.Sugestoes.Remove(sugestao);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<string>.Ok("Sugestão excluída com sucesso.");
        }
    }
}
