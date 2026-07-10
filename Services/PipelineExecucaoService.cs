using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using api.Data;
using api.Helpers;
using api.Models;
using api.Services.Interfaces;
using api.Views;
using System.Text.Json;

namespace api.Services
{
    public class PipelineExecucaoService(
        AppDbContext context,
        IRedisPublisher redisPublisher,
        IConfiguration configuration) : IPipelineExecucao
    {
        private readonly AppDbContext _context = context;
        private readonly IRedisPublisher _redisPublisher = redisPublisher;
        private readonly string _hmacKey = configuration["Mascaramento:HmacKey"] ?? "chave-padrao-trocar-em-producao";

        public async Task<ResultadoPaginado<PipelineExecucaoListDto>> GetExecucoesAsync(int? pipelineId, StatusPipelineExecucao? status, string? busca, int page, int limit)
        {
            if (page < 1) page = 1;
            if (limit < 1) limit = 1;

            var query = _context.PipelineExecucoes
                .Include(e => e.Pipeline)
                .AsQueryable();

            if (pipelineId.HasValue)
                query = query.Where(e => e.PipelineId == pipelineId.Value);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.ToLower();
                query = query.Where(e =>
                    e.Pipeline.Nome.ToLower().Contains(termo) ||
                    e.TabelaSilver.ToLower().Contains(termo) ||
                    e.TabelaGold.ToLower().Contains(termo));
            }

            var total = await query.CountAsync().ConfigureAwait(false);

            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(e => new PipelineExecucaoListDto
                {
                    Id = e.Id,
                    BatchId = e.BatchId,
                    PipelineId = e.PipelineId,
                    PipelineNome = e.Pipeline.Nome,
                    TabelaSilver = e.TabelaSilver,
                    TabelaGold = e.TabelaGold,
                    Status = e.Status,
                    Mensagem = e.Mensagem,
                    IniciadoEm = e.IniciadoEm,
                    FinalizadoEm = e.FinalizadoEm,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return ResultadoPaginado<PipelineExecucaoListDto>.Ok(page, limit, total, items);
        }

        public async Task<Resultado<PipelineExecucaoGetDto>> GetExecucaoByIdAsync(int id)
        {
            var execucao = await _context.PipelineExecucoes
                .Include(e => e.Pipeline)
                .FirstOrDefaultAsync(e => e.Id == id)
                .ConfigureAwait(false);

            if (execucao == null)
                return Resultado<PipelineExecucaoGetDto>.Falha("Execução não encontrada.");

            return Resultado<PipelineExecucaoGetDto>.Ok(MapearGetDto(execucao));
        }

        public async Task<Resultado<PipelineExecucaoExecutarResultDto>> ExecutarAsync(PipelineExecucaoCreateDto dto, string uploadedBy)
        {
            var arquivo = dto.Arquivo;

            if (arquivo == null || arquivo.Length == 0)
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha("Arquivo não enviado.");

            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            if (extensao != ".csv" && extensao != ".xlsx")
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha("Apenas arquivos CSV ou XLSX são permitidos.");

            var pipeline = await _context.Pipelines
                .FirstOrDefaultAsync(p => p.Id == dto.PipelineId)
                .ConfigureAwait(false);

            if (pipeline == null)
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha("Pipeline não encontrada.");

            byte[] conteudo;
            using (var ms = new MemoryStream())
            {
                await arquivo.CopyToAsync(ms).ConfigureAwait(false);
                conteudo = ms.ToArray();
            }

            var fileHash = Convert.ToHexString(SHA256.HashData(conteudo)).ToLowerInvariant();

            var existente = await _context.BronzeUploadsAuditoria
                .FirstOrDefaultAsync(u => u.FileHash == fileHash)
                .ConfigureAwait(false);

            if (existente != null)
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha(
                    $"Este arquivo já foi processado anteriormente (batch {existente.BatchId}, em {existente.CreatedAt:dd/MM/yyyy HH:mm}).");

            List<ColunaSensivelDto>? colunasSensiveis = null;
            if (!string.IsNullOrWhiteSpace(dto.ColunasSensiveisJson))
            {
                try
                {
                    colunasSensiveis = JsonSerializer.Deserialize<List<ColunaSensivelDto>>(
                        dto.ColunasSensiveisJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    return Resultado<PipelineExecucaoExecutarResultDto>.Falha("colunasSensiveis contém JSON inválido.");
                }
            }

            var batchId = Guid.NewGuid();
            var registros = new List<BronzeArquivoBruto>();
            var colunas = new List<string>();
            var primeirasLinhas = new List<Dictionary<string, string>>();

            using var stream = new MemoryStream(conteudo);

            if (extensao == ".csv")
            {
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";"
                });

                var records = csv.GetRecords<dynamic>().ToList();
                var allRecords = records.Select(r => (IDictionary<string, object>)r).ToList();

                if (!allRecords.Any())
                    return Resultado<PipelineExecucaoExecutarResultDto>.Falha("Arquivo CSV está vazio.");

                colunas = allRecords.First().Keys
                    .Where(k => !EhSuprimida(k, colunasSensiveis))
                    .ToList();

                for (int i = 0; i < allRecords.Count; i++)
                {
                    var dados = allRecords[i].ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? string.Empty);
                    AplicarMascaramento(dados, colunasSensiveis);
                    var json = JsonSerializer.Serialize(dados);

                    registros.Add(new BronzeArquivoBruto
                    {
                        BatchId = batchId,
                        NumeroLinha = i + 1,
                        Dados = json
                    });

                    if (i < 5)
                        primeirasLinhas.Add(new Dictionary<string, string>(dados));
                }
            }
            else
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.First();
                var rows = worksheet.RowsUsed().ToList();

                if (rows.Count < 2)
                    return Resultado<PipelineExecucaoExecutarResultDto>.Falha("Arquivo XLSX está vazio.");

                var headerCells = rows[0].Cells();
                var todasColunas = headerCells.Select(c => c.GetValue<string>()).ToList();
                colunas = todasColunas.Where(c => !EhSuprimida(c, colunasSensiveis)).ToList();

                for (int i = 1; i < rows.Count; i++)
                {
                    var cells = rows[i].Cells(1, todasColunas.Count).ToList();
                    var dados = new Dictionary<string, string>();

                    for (int j = 0; j < todasColunas.Count; j++)
                    {
                        var valor = j < cells.Count ? cells[j].GetValue<string>() : string.Empty;
                        dados[todasColunas[j]] = valor ?? string.Empty;
                    }

                    AplicarMascaramento(dados, colunasSensiveis);
                    var json = JsonSerializer.Serialize(dados);

                    registros.Add(new BronzeArquivoBruto
                    {
                        BatchId = batchId,
                        NumeroLinha = i,
                        Dados = json
                    });

                    if (i <= 5)
                        primeirasLinhas.Add(new Dictionary<string, string>(dados));
                }
            }

            _context.BronzeArquivosBrutos.AddRange(registros);
            _context.BronzeUploadsAuditoria.Add(new BronzeUploadAuditoria
            {
                FileHash = fileHash,
                BatchId = batchId,
                NomeArquivo = arquivo.FileName,
                TotalLinhas = registros.Count,
                UploadedBy = uploadedBy
            });

            var execucao = new PipelineExecucao
            {
                BatchId = batchId,
                PipelineId = dto.PipelineId,
                TabelaSilver = dto.TabelaSilver,
                TabelaGold = dto.TabelaGold,
                Status = StatusPipelineExecucao.Pendente,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PipelineExecucoes.Add(execucao);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            var payload = new
            {
                execucao_id = execucao.Id,
                batch_id = batchId,
                pipeline_id = dto.PipelineId,
                tabela_silver = dto.TabelaSilver,
                tabela_gold = dto.TabelaGold
            };

            await _redisPublisher.PublicarAsync("pipeline_tasks", payload).ConfigureAwait(false);

            return Resultado<PipelineExecucaoExecutarResultDto>.Ok(new PipelineExecucaoExecutarResultDto
            {
                Id = execucao.Id,
                BatchId = batchId,
                PipelineId = execucao.PipelineId,
                PipelineNome = pipeline.Nome,
                TabelaSilver = execucao.TabelaSilver,
                TabelaGold = execucao.TabelaGold,
                Status = execucao.Status,
                Mensagem = execucao.Mensagem,
                IniciadoEm = execucao.IniciadoEm,
                FinalizadoEm = execucao.FinalizadoEm,
                CreatedAt = execucao.CreatedAt,
                UpdatedAt = execucao.UpdatedAt,
                Colunas = colunas,
                TotalLinhas = registros.Count,
                PrimeirasLinhas = primeirasLinhas
            });
        }

        private static bool EhSuprimida(string coluna, List<ColunaSensivelDto>? colunasSensiveis)
        {
            if (colunasSensiveis == null) return false;
            return colunasSensiveis.Any(c =>
                string.Equals(c.Coluna, coluna, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Estrategia, "suprimir", StringComparison.OrdinalIgnoreCase));
        }

        private void AplicarMascaramento(Dictionary<string, string> dados, List<ColunaSensivelDto>? colunasSensiveis)
        {
            if (colunasSensiveis == null) return;

            foreach (var regra in colunasSensiveis)
            {
                var chave = dados.Keys.FirstOrDefault(k =>
                    string.Equals(k, regra.Coluna, StringComparison.OrdinalIgnoreCase));

                if (chave == null) continue;

                if (string.Equals(regra.Estrategia, "suprimir", StringComparison.OrdinalIgnoreCase))
                {
                    dados.Remove(chave);
                }
                else if (string.Equals(regra.Estrategia, "hmac", StringComparison.OrdinalIgnoreCase))
                {
                    var keyBytes = Encoding.UTF8.GetBytes(_hmacKey);
                    var valueBytes = Encoding.UTF8.GetBytes(dados[chave]);
                    var hash = HMACSHA256.HashData(keyBytes, valueBytes);
                    dados[chave] = Convert.ToHexString(hash).ToLowerInvariant();
                }
            }
        }

        public async Task<Resultado<string>> RollbackAsync(int id)
        {
            var execucao = await _context.PipelineExecucoes
                .FirstOrDefaultAsync(e => e.Id == id)
                .ConfigureAwait(false);

            if (execucao == null)
                return Resultado<string>.Falha("Execução não encontrada.");

            if (execucao.Status == StatusPipelineExecucao.Rollback)
                return Resultado<string>.Ok("Rollback já foi realizado.");

            if (execucao.Status == StatusPipelineExecucao.Pendente || execucao.Status == StatusPipelineExecucao.Processando)
                return Resultado<string>.Falha("Rollback não pode ser executado enquanto a execução está pendente ou em processamento.");

            var batchIdStr = execucao.BatchId.ToString();
            await _context.Database.ExecuteSqlRawAsync(
                $"DELETE FROM silver.{execucao.TabelaSilver} WHERE batch_id::text = '{batchIdStr}'; DELETE FROM gold.{execucao.TabelaGold} WHERE batch_id::text = '{batchIdStr}'; DELETE FROM bronze.arquivos_brutos WHERE batch_id = '{batchIdStr}'")
                .ConfigureAwait(false);

            execucao.Status = StatusPipelineExecucao.Rollback;
            execucao.Mensagem = "Rollback executado com sucesso.";
            execucao.FinalizadoEm = DateTime.UtcNow;
            execucao.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<string>.Ok("Rollback executado com sucesso.");
        }

        private static PipelineExecucaoGetDto MapearGetDto(PipelineExecucao execucao) => new()
        {
            Id = execucao.Id,
            BatchId = execucao.BatchId,
            PipelineId = execucao.PipelineId,
            PipelineNome = execucao.Pipeline?.Nome ?? string.Empty,
            TabelaSilver = execucao.TabelaSilver,
            TabelaGold = execucao.TabelaGold,
            Status = execucao.Status,
            Mensagem = execucao.Mensagem,
            IniciadoEm = execucao.IniciadoEm,
            FinalizadoEm = execucao.FinalizadoEm,
            CreatedAt = execucao.CreatedAt,
            UpdatedAt = execucao.UpdatedAt
        };
    }
}
