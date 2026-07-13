using api.Models;

namespace api.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendCadastroAprovadoAsync(string email, string nome);
        Task SendCadastroRecusadoAsync(string email, string nome);
        Task SendSugestaoAtualizadaAsync(string email, string nomeContato, string tituloSugestao, StatusSugestao status);
        Task SendRecuperacaoSenhaAsync(string email, string nome, string resetLink);
    }
}
