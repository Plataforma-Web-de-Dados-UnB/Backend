namespace api.Services.Interfaces
{
    public interface IRedisPublisher
    {
        Task PublicarAsync(string fila, object payload);
    }
}
