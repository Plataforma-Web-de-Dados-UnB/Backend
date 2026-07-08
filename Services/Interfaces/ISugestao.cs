using api.Helpers;
using api.Models;
using api.Views;

namespace api.Services.Interfaces
{
    public interface ISugestao
    {
        Task<Resultado<string>> CreateSugestaoAsync(SugestaoCreateDto dto);
        Task<ResultadoPaginado<SugestaoListDto>> GetSugestoesAsync(StatusSugestao? status, TipoSugestao? tipo, string? busca, int page, int limit);
        Task<Resultado<SugestaoGetDto>> GetSugestaoByIdAsync(int id);
        Task<Resultado<string>> UpdateStatusAsync(int id, SugestaoUpdateStatusDto dto);
        Task<Resultado<string>> DeleteSugestaoAsync(int id);
    }
}
