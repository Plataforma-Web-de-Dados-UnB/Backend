using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

            List<Guid>? batchIdsPorArquivo = null;
            Guid? batchIdBuscado = null;
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca!.ToLower();
                _ = Guid.TryParse(termo, out var parsedBatchId);
                batchIdBuscado = parsedBatchId;

                batchIdsPorArquivo = await _context.BronzeUploadsAuditoria
                    .Where(u => u.NomeArquivo.ToLower().Contains(termo))
                    .Select(u => u.BatchId)
                    .Distinct()
                    .ToListAsync()
                    .ConfigureAwait(false);

                query = query.Where(e =>
                    e.Pipeline.Nome.ToLower().Contains(termo) ||
                    e.TabelaSilver.ToLower().Contains(termo) ||
                    e.TabelaGold.ToLower().Contains(termo) ||
                    (batchIdBuscado.HasValue && e.BatchId == batchIdBuscado.Value) ||
                    batchIdsPorArquivo.Contains(e.BatchId));
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
                    TabelasGoldExtras = e.TabelasGoldExtras != null
                        ? JsonSerializer.Deserialize<List<string>>(e.TabelasGoldExtras)
                            !.Where(t => t != e.TabelaGold).ToList()
                        : null,
                    Status = e.Status,
                    Mensagem = e.Mensagem,
                    IniciadoEm = e.IniciadoEm,
                    FinalizadoEm = e.FinalizadoEm,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync()
                .ConfigureAwait(false);

            var batchIds = items.Select(i => i.BatchId).Distinct().ToList();
            var nomesArquivos = await _context.BronzeUploadsAuditoria
                .Where(u => batchIds.Contains(u.BatchId))
                .ToDictionaryAsync(u => u.BatchId, u => u.NomeArquivo)
                .ConfigureAwait(false);

            foreach (var item in items)
            {
                item.NomeArquivo = nomesArquivos.GetValueOrDefault(item.BatchId);
            }

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

            var nomeArquivo = await _context.BronzeUploadsAuditoria
                .Where(u => u.BatchId == execucao.BatchId)
                .Select(u => u.NomeArquivo)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return Resultado<PipelineExecucaoGetDto>.Ok(MapearGetDto(execucao, nomeArquivo));
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

            if (!pipeline.Ativo)
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha("Pipeline desativada. Ative-a antes de executar.");

            byte[] conteudo;
            using (var ms = new MemoryStream())
            {
                await arquivo.CopyToAsync(ms).ConfigureAwait(false);
                conteudo = ms.ToArray();
            }

            var fileHash = Convert.ToHexString(SHA256.HashData(conteudo)).ToLowerInvariant();

            var batchIdsComHash = await _context.BronzeUploadsAuditoria
                .Where(u => u.FileHash == fileHash)
                .Select(u => u.BatchId)
                .ToListAsync()
                .ConfigureAwait(false);

            var existeSucesso = await _context.PipelineExecucoes
                .AnyAsync(e => batchIdsComHash.Contains(e.BatchId)
                            && e.PipelineId == dto.PipelineId
                            && e.Status == StatusPipelineExecucao.Sucesso)
                .ConfigureAwait(false);

            if (existeSucesso)
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha(
                    "Este arquivo já foi processado com sucesso por esta pipeline. Envie um arquivo diferente ou use outra pipeline.");

            if (!Regex.IsMatch(dto.TabelaSilver, @"^[a-z_][a-z0-9_]{0,62}$"))
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha(
                    "Nome da tabela prata inválido. Use apenas letras minúsculas, números e _. Deve começar com letra ou _.");

            if (!Regex.IsMatch(dto.TabelaGold, @"^[a-z_][a-z0-9_]{0,62}$"))
                return Resultado<PipelineExecucaoExecutarResultDto>.Falha(
                    "Nome da tabela ouro inválido. Use apenas letras minúsculas, números e _. Deve começar com letra ou _.");

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
                    Delimiter = DetectarDelimitador(conteudo)
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

        private static string DetectarDelimitador(byte[] conteudo)
        {
            var primeiraLinha = new StreamReader(new MemoryStream(conteudo)).ReadLine() ?? "";
            return primeiraLinha.Count(c => c == ';') >= primeiraLinha.Count(c => c == ',') ? ";" : ",";
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
            var extras = DeserializarGoldExtras(execucao.TabelasGoldExtras)
                ?.Where(t => !string.Equals(t, execucao.TabelaGold, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? [];

            var extraGoldDeletes = string.Concat(extras.Select(t => $"""

                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'gold' AND table_name = '{t}'
                    ) THEN
                        EXECUTE format('DELETE FROM gold.%I WHERE batch_id::text = %L', '{t}', '{batchIdStr}');
                    END IF;
                """));

            var sql = $"""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'silver' AND table_name = '{execucao.TabelaSilver}'
                    ) THEN
                        EXECUTE format('DELETE FROM silver.%I WHERE batch_id::text = %L', '{execucao.TabelaSilver}', '{batchIdStr}');
                    END IF;
                    IF EXISTS (
                        SELECT 1 FROM information_schema.tables
                        WHERE table_schema = 'gold' AND table_name = '{execucao.TabelaGold}'
                    ) THEN
                        EXECUTE format('DELETE FROM gold.%I WHERE batch_id::text = %L', '{execucao.TabelaGold}', '{batchIdStr}');
                    END IF;{extraGoldDeletes}
                END $$;
                DELETE FROM bronze.arquivos_brutos WHERE batch_id = '{batchIdStr}';
                """;
            await _context.Database.ExecuteSqlRawAsync(sql).ConfigureAwait(false);

            var auditoria = await _context.BronzeUploadsAuditoria
                .FirstOrDefaultAsync(u => u.BatchId == execucao.BatchId)
                .ConfigureAwait(false);
            if (auditoria != null)
                _context.BronzeUploadsAuditoria.Remove(auditoria);

            execucao.Status = StatusPipelineExecucao.Rollback;
            execucao.Mensagem = "Rollback executado com sucesso.";
            execucao.FinalizadoEm = DateTime.UtcNow;
            execucao.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Resultado<string>.Ok("Rollback executado com sucesso.");
        }

        private static List<string>? DeserializarGoldExtras(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<List<string>>(json); }
            catch { return null; }
        }

        private static PipelineExecucaoGetDto MapearGetDto(PipelineExecucao execucao, string? nomeArquivo = null) => new()
        {
            Id = execucao.Id,
            BatchId = execucao.BatchId,
            PipelineId = execucao.PipelineId,
            PipelineNome = execucao.Pipeline?.Nome ?? string.Empty,
            NomeArquivo = nomeArquivo,
            TabelaSilver = execucao.TabelaSilver,
            TabelaGold = execucao.TabelaGold,
            TabelasGoldExtras = DeserializarGoldExtras(execucao.TabelasGoldExtras)
                ?.Where(t => t != execucao.TabelaGold).ToList(),
            Status = execucao.Status,
            Mensagem = execucao.Mensagem,
            IniciadoEm = execucao.IniciadoEm,
            FinalizadoEm = execucao.FinalizadoEm,
            CreatedAt = execucao.CreatedAt,
            UpdatedAt = execucao.UpdatedAt
        };
    }
}
