using System.Text;
using System.Text.Json;
using api.Services.Interfaces;
using StackExchange.Redis;

namespace api.Services
{
    public class RedisPublisher(IConfiguration configuration) : IRedisPublisher, IDisposable
    {
        private readonly string _connectionString = configuration.GetConnectionString("RedisConnection")
            ?? throw new InvalidOperationException("Connection string 'RedisConnection' nao configurada.");

        private readonly JsonSerializerOptions _snakeCaseOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private ConnectionMultiplexer? _connection;
        private readonly Lock _lock = new();

        private IDatabase GetDatabase()
        {
            if (_connection is { IsConnected: true })
                return _connection.GetDatabase();

            lock (_lock)
            {
                if (_connection is { IsConnected: true })
                    return _connection.GetDatabase();

                _connection?.Dispose();
                _connection = ConnectionMultiplexer.Connect(_connectionString);
                return _connection.GetDatabase();
            }
        }

        public async Task PublicarAsync(string fila, object payload)
        {
            var db = GetDatabase();
            var envelope = MontarEnvelopeKombu(fila, payload);
            await db.ListLeftPushAsync(fila, envelope).ConfigureAwait(false);
        }

        public async Task<bool> PingAsync()
        {
            try
            {
                var db = GetDatabase();
                await db.PingAsync().ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string MontarEnvelopeKombu(string fila, object payload)
        {
            var taskId = Guid.NewGuid().ToString();
            var payloadJson = JsonSerializer.Serialize(payload, _snakeCaseOptions);
            var bodyJson = $"[[], {payloadJson}, {{\"callbacks\": null, \"errbacks\": null, \"chain\": null, \"chord\": null}}]";
            var bodyBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(bodyJson));

            var envelope = new Dictionary<string, object?>
            {
                ["body"] = bodyBase64,
                ["content-type"] = "application/json",
                ["content-encoding"] = "utf-8",
                ["headers"] = new Dictionary<string, object?>
                {
                    ["lang"] = "py",
                    ["task"] = "tasks.processar_pipeline",
                    ["id"] = taskId,
                    ["shadow"] = null,
                    ["eta"] = null,
                    ["expires"] = null,
                    ["group"] = null,
                    ["group_index"] = null,
                    ["retries"] = 0,
                    ["timelimit"] = new int?[] { null, null },
                    ["root_id"] = taskId,
                    ["parent_id"] = null,
                    ["argsrepr"] = "()",
                    ["kwargsrepr"] = payloadJson,
                    ["origin"] = "backend@unb-portal",
                    ["ignore_result"] = false,
                    ["replaced_task_nesting"] = 0,
                    ["stamped_headers"] = null,
                    ["stamps"] = new Dictionary<string, object>()
                },
                ["properties"] = new Dictionary<string, object?>
                {
                    ["correlation_id"] = taskId,
                    ["reply_to"] = Guid.NewGuid().ToString(),
                    ["delivery_mode"] = 2,
                    ["delivery_info"] = new Dictionary<string, string>
                    {
                        ["exchange"] = "",
                        ["routing_key"] = fila
                    },
                    ["priority"] = 0,
                    ["body_encoding"] = "base64",
                    ["delivery_tag"] = Guid.NewGuid().ToString()
                }
            };

            return JsonSerializer.Serialize(envelope);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
