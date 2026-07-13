using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using api.Models;
using api.Services.Interfaces;

namespace api.Services
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<EmailService> _logger = logger;

        private string GetTemplatesPath() =>
            Path.Combine(AppContext.BaseDirectory, "Templates");

        private async Task<string> LoadTemplateAsync(string templateName)
        {
            var path = Path.Combine(GetTemplatesPath(), templateName);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Template de email não encontrado: {path}");
            return await File.ReadAllTextAsync(path).ConfigureAwait(false);
        }

        private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.TryParse(_configuration["Email:SmtpPort"], out var p) ? p : 587;
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Email não enviado: credenciais SMTP não configuradas.");
                return;
            }

            var fromAddress = _configuration["Email:FromAddress"] ?? username;
            var fromName = _configuration["Email:FromName"] ?? "Portal de Dados UnB";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls).ConfigureAwait(false);
                await client.AuthenticateAsync(username, password).ConfigureAwait(false);
                await client.SendAsync(message).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
                _logger.LogInformation("Email enviado para {Email} — assunto: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar email para {Email}", toEmail);
            }
        }

        public async Task SendCadastroAprovadoAsync(string email, string nome)
        {
            var portalUrl = _configuration["Email:FrontendBaseUrl"] ?? "http://localhost:5173";
            var template = await LoadTemplateAsync("CadastroAprovado.html").ConfigureAwait(false);
            var html = template
                .Replace("{{NOME}}", nome)
                .Replace("{{EMAIL}}", email)
                .Replace("{{PORTAL_URL}}", portalUrl);
            await SendAsync(email, nome, "Cadastro Aprovado - Portal de Dados Institucionais", html).ConfigureAwait(false);
        }

        public async Task SendCadastroRecusadoAsync(string email, string nome)
        {
            var template = await LoadTemplateAsync("CadastroRecusado.html").ConfigureAwait(false);
            var html = template
                .Replace("{{NOME}}", nome)
                .Replace("{{EMAIL}}", email);
            await SendAsync(email, nome, "Atualização de Cadastro - Portal de Dados Institucionais", html).ConfigureAwait(false);
        }

        public async Task SendSugestaoAtualizadaAsync(string email, string nomeContato, string tituloSugestao, StatusSugestao status)
        {
            var template = await LoadTemplateAsync("SugestaoAtualizada.html").ConfigureAwait(false);
            var statusTexto = status switch
            {
                StatusSugestao.Analisado => "Analisada",
                StatusSugestao.Descartado => "Descartada",
                _ => status.ToString()
            };
            var statusCor = status switch
            {
                StatusSugestao.Analisado => "#009c3b",
                StatusSugestao.Descartado => "#dc2626",
                _ => "#374151"
            };
            var statusBadge = $"<p style=\"margin:0;font-size:14px;font-weight:700;color:{statusCor};\">{statusTexto}</p>";
            var html = template
                .Replace("{{NOME}}", nomeContato)
                .Replace("{{TITULO_SUGESTAO}}", tituloSugestao)
                .Replace("{{STATUS_BADGE}}", statusBadge);
            await SendAsync(email, nomeContato, $"Sua sugestão foi {statusTexto.ToLower()} - Portal de Dados Institucionais", html).ConfigureAwait(false);
        }

        public async Task SendRecuperacaoSenhaAsync(string email, string nome, string resetLink)
        {
            var template = await LoadTemplateAsync("RecuperacaoSenha.html").ConfigureAwait(false);
            var html = template
                .Replace("{{NOME}}", nome)
                .Replace("{{RESET_LINK}}", resetLink);
            await SendAsync(email, nome, "Redefinição de Senha - Portal de Dados Institucionais", html).ConfigureAwait(false);
        }
    }
}
