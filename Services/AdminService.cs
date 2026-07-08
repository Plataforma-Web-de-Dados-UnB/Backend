using System.Data;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using api.Services.Interfaces;
using api.Views;

namespace api.Services
{
    public class AdminService(
        AppDbContext context,
        IConfiguration configuration,
        IRedisPublisher redisPublisher) : IAdmin
    {
        private readonly AppDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;
        private readonly IRedisPublisher _redisPublisher = redisPublisher;

        public async Task<AdminKpisDto> GetKpisAsync()
        {
            var totalPipelines = await _context.Pipelines.CountAsync().ConfigureAwait(false);
            var totalCategorias = await _context.Categorias.CountAsync().ConfigureAwait(false);
            var totalPaineis = await _context.Paineis.CountAsync().ConfigureAwait(false);

            long volumeBronzeBytes = 0;
            long volumeSilverBytes = 0;
            long volumeGoldBytes = 0;

            long linhasBronze = 0;
            long linhasSilver = 0;
            long linhasGold = 0;

            bool databaseOnline = false;
            bool supersetOnline = false;

            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync().ConfigureAwait(false);
                }
                databaseOnline = true;

                using (var cmd = conn.CreateCommand())
                {
                    // Bronze size
                    cmd.CommandText = @"
                        SELECT COALESCE(SUM(pg_total_relation_size(quote_ident(schemaname) || '.' || quote_ident(tablename))), 0)
                        FROM pg_tables
                        WHERE schemaname = 'bronze';";
                    volumeBronzeBytes = Convert.ToInt64(await cmd.ExecuteScalarAsync().ConfigureAwait(false));

                    // Bronze rows - only arquivos_brutos
                    linhasBronze = await GetExactRowCountAsync(conn, "bronze").ConfigureAwait(false);

                    // Silver size
                    cmd.CommandText = @"
                        SELECT COALESCE(SUM(pg_total_relation_size(quote_ident(schemaname) || '.' || quote_ident(tablename))), 0)
                        FROM pg_tables
                        WHERE schemaname = 'silver';";
                    volumeSilverBytes = Convert.ToInt64(await cmd.ExecuteScalarAsync().ConfigureAwait(false));

                    // Silver rows
                    linhasSilver = await GetExactRowCountAsync(conn, "silver").ConfigureAwait(false);

                    // Gold size
                    cmd.CommandText = @"
                        SELECT COALESCE(SUM(pg_total_relation_size(quote_ident(schemaname) || '.' || quote_ident(tablename))), 0)
                        FROM pg_tables
                        WHERE schemaname = 'gold';";
                    volumeGoldBytes = Convert.ToInt64(await cmd.ExecuteScalarAsync().ConfigureAwait(false));

                    // Gold rows
                    linhasGold = await GetExactRowCountAsync(conn, "gold").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular volumes e linhas: {ex.Message}");
            }

            int totalSugestoesPendentes = 0;
            int totalUsuariosPendentes = 0;

            try
            {
                totalSugestoesPendentes = await _context.Sugestoes.CountAsync(s => s.Status == StatusSugestao.Pendente).ConfigureAwait(false);
                totalUsuariosPendentes = await _context.Usuarios.CountAsync(u => u.Status == StatusUsuario.Pendente).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao contar pendencias: {ex.Message}");
            }

            bool redisOnline = false;
            try
            {
                redisOnline = await _redisPublisher.PingAsync().ConfigureAwait(false);
            }
            catch
            {
                redisOnline = false;
            }

            try
            {
                var supersetUrl = _configuration["Superset:BaseUrl"];
                if (!string.IsNullOrEmpty(supersetUrl))
                {
                    supersetOnline = await TestUrlAsync(supersetUrl).ConfigureAwait(false);
                    if (!supersetOnline && supersetUrl.Contains("localhost"))
                    {
                        var dockerFallback = supersetUrl.Replace("localhost", "host.docker.internal");
                        supersetOnline = await TestUrlAsync(dockerFallback).ConfigureAwait(false);
                    }
                    if (!supersetOnline && supersetUrl.Contains("localhost"))
                    {
                        var dockerFallback2 = supersetUrl.Replace("localhost", "superset");
                        supersetOnline = await TestUrlAsync(dockerFallback2).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                supersetOnline = false;
            }

            return new AdminKpisDto
            {
                TotalPipelines = totalPipelines,
                TotalCategorias = totalCategorias,
                TotalPaineis = totalPaineis,
                VolumeBronze = volumeBronzeBytes,
                VolumeSilver = volumeSilverBytes,
                VolumeGold = volumeGoldBytes,
                LinhasBronze = linhasBronze,
                LinhasSilver = linhasSilver,
                LinhasGold = linhasGold,
                DatabaseOnline = databaseOnline,
                SupersetOnline = supersetOnline,
                RedisOnline = redisOnline,
                TotalSugestoesPendentes = totalSugestoesPendentes,
                TotalUsuariosPendentes = totalUsuariosPendentes
            };
        }

        private static async Task<bool> TestUrlAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(1000);
                    var response = await client.GetAsync(url).ConfigureAwait(false);
                    return response.IsSuccessStatusCode || (int)response.StatusCode < 500;
                }
            }
            catch
            {
                return false;
            }
        }

        private static async Task<long> GetExactRowCountAsync(System.Data.Common.DbConnection conn, string schemaName)
        {
            try
            {
                if (schemaName.Equals("bronze", StringComparison.OrdinalIgnoreCase))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM bronze.arquivos_brutos;";
                        var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                        return Convert.ToInt64(result);
                    }
                }

                var tableNames = new List<string>();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT tablename FROM pg_tables WHERE schemaname = @schema;";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "@schema";
                    param.Value = schemaName;
                    cmd.Parameters.Add(param);

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            tableNames.Add(reader.GetString(0));
                        }
                    }
                }

                if (tableNames.Count == 0) return 0;

                var sqlParts = tableNames.Select(t => $"(SELECT COUNT(*) FROM {schemaName}.\"{t}\")");
                var sql = $"SELECT COALESCE({string.Join(" + ", sqlParts)}, 0);";

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    return Convert.ToInt64(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular contagem exata para {schemaName}: {ex.Message}");
                return 0;
            }
        }
    }
}
