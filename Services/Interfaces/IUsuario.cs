using api.Helpers;
using api.Views;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IUsuario
    {
        Task<string?> RegisterAsync(UsuarioRegisterDto user);
        Task<Resultado<UsuarioLoginResponseDto>> LoginAsync(UsuarioLoginDto user);
        Task<UsuarioGetDto?> GetUsuarioByIdAsync(string id);
        Task<UsuarioGetDto?> GetPerfilAsync(string userId);
        Task<List<UsuarioListDto>> GetUsuariosAsync(StatusUsuario? status, string? busca);
        Task<string?> UpdateStatusAsync(string id, StatusUsuario status);
        Task<string?> ChangePasswordAsync(string userId, UsuarioChangePasswordDto passwordDto);
    }
}
