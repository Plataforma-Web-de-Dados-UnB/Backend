using api.Helpers;
using api.Views;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IUsuario
    {
        Task<Resultado<string>> RegisterAsync(UsuarioRegisterDto user);
        Task<Resultado<UsuarioLoginResponseDto>> LoginAsync(UsuarioLoginDto user);
        Task<Resultado<UsuarioGetDto>> GetUsuarioByIdAsync(string id);
        Task<Resultado<UsuarioGetDto>> GetPerfilAsync(string userId);
        Task<ResultadoPaginado<UsuarioListDto>> GetUsuariosAsync(StatusUsuario? status, CargoUsuario? cargo, string? busca, int page, int limit);
        Task<Resultado<string>> UpdateStatusAsync(string id, StatusUsuario status);
        Task<Resultado<string>> ChangePasswordAsync(string userId, UsuarioChangePasswordDto passwordDto);
    }
}
