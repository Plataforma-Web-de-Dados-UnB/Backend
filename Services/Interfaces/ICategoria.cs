using api.Helpers;
using api.Views;

namespace api.Services.Interfaces
{
    public interface ICategoria
    {
        Task<ResultadoPaginado<CategoriaListDto>> GetCategoriasAsync(bool? active, string? busca, int page, int limit, string baseUrl);
        Task<Resultado<CategoriaGetDto>> GetCategoriaByIdAsync(int id, string baseUrl, bool apenasAtiva = false);
        Task<Resultado<CategoriaGetDto>> CreateCategoriaAsync(CategoriaCreateDto dto, string baseUrl);
        Task<Resultado<CategoriaGetDto>> UpdateCategoriaAsync(int id, CategoriaUpdateDto dto, string baseUrl);
        Task<Resultado<string>> DeleteCategoriaAsync(int id, bool hardDelete = false);
        Task<Resultado<string>> ToggleActiveAsync(int id);
    }
}
