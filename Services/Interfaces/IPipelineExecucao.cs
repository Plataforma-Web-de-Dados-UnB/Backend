using api.Helpers;
using api.Models;
using api.Views;

namespace api.Services.Interfaces
{
    public interface IPipelineExecucao
    {
        Task<ResultadoPaginado<PipelineExecucaoListDto>> GetExecucoesAsync(int? pipelineId, StatusPipelineExecucao? status, string? busca, int page, int limit);
        Task<Resultado<PipelineExecucaoGetDto>> GetExecucaoByIdAsync(int id);
        Task<Resultado<PipelineExecucaoExecutarResultDto>> ExecutarAsync(PipelineExecucaoCreateDto dto, string uploadedBy);
        Task<Resultado<string>> RollbackAsync(int id);
    }
}
