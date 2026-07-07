using api.Helpers;
using api.Views;

namespace api.Services.Interfaces;

public interface ISupersetService
{
    Task<Resultado<string>> GetGuestTokenAsync(string dashboardId, string username, string firstName, string lastName);
}
