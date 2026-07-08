using api.Views;

namespace api.Services.Interfaces
{
    public interface IAdmin
    {
        Task<AdminKpisDto> GetKpisAsync();
    }
}
