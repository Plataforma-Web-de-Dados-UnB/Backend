using api.Helpers;
using api.Views;

namespace api.Services.Interfaces
{
    public interface IPipeline
    {
        Task<ResultadoPaginado<PipelineListDto>> GetPipelinesAsync(string? busca, int page, int limit);
        Task<Resultado<PipelineGetDto>> GetPipelineByIdAsync(int id);
        Task<Resultado<PipelineGetDto>> CreatePipelineAsync(PipelineCreateDto dto);
        Task<Resultado<PipelineGetDto>> UpdatePipelineAsync(int id, PipelineUpdateDto dto);
        Task<Resultado<string>> DeletePipelineAsync(int id);
    }
}
