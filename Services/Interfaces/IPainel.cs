using api.Helpers;
using api.Views;

namespace api.Services.Interfaces
{
    public interface IPainel
    {
        Task<ResultadoPaginado<PainelListDto>> GetPaineisAsync(bool? active, int? categoriaId, string? busca, int page, int limit);
        Task<Resultado<PainelGetDto>> GetPainelByIdAsync(int id, bool apenasAtivo = false);
        Task<Resultado<PainelGetDto>> CreatePainelAsync(PainelCreateDto dto);
        Task<Resultado<PainelGetDto>> UpdatePainelAsync(int id, PainelUpdateDto dto);
        Task<Resultado<string>> DeletePainelAsync(int id);
        Task<Resultado<string>> ToggleActiveAsync(int id);
        Task<List<PainelBuscaDto>> BuscarAsync(string q, int limit);
    }
}
