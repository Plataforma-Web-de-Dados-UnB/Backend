using api.Helpers;
using api.Models;
using api.Views;

namespace api.Services.Interfaces
{
    public interface IPipelineExecucao
    {
        Task<ResultadoPaginado<PipelineExecucaoListDto>> GetExecucoesAsync(int? pipelineId, StatusPipelineExecucao? status, int page, int limit);
        Task<Resultado<PipelineExecucaoGetDto>> GetExecucaoByIdAsync(int id);
        Task<Resultado<UploadPreviewDto>> ProcessarUploadAsync(IFormFile arquivo, string uploadedBy);
        Task<Resultado<PipelineExecucaoGetDto>> ExecutarAsync(PipelineExecucaoCreateDto dto);
        Task<Resultado<string>> RollbackAsync(int id);
    }
}
