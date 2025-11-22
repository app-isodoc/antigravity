using System.Net;
using System.Net.Mail;
using Isodoc.Web.Models;
using Microsoft.Extensions.Options;

namespace Isodoc.Web.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName, string clientName, string password, string loginUrl)
    {
        var subject = "Bem-vindo ao Isodoc!";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #667eea;'>Bem-vindo ao Isodoc!</h1>
                </div>
                <p>Olá <strong>{toName}</strong>,</p>
                <p>Sua conta no <strong>Isodoc - Sistema de Gestão da Qualidade ISO 9001</strong> foi criada com sucesso para a empresa <strong>{clientName}</strong>!</p>
                <p>Você foi designado como <strong>Usuário Master</strong> e tem acesso total ao sistema.</p>
                
                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                    <h3 style='margin-top: 0; color: #4a5568;'>🔒 Suas Credenciais de Acesso</h3>
                    <p style='margin-bottom: 5px;'><strong>Email:</strong> {toEmail}</p>
                    <p style='margin-bottom: 0;'><strong>Senha Temporária:</strong> <span style='background-color: #e2e8f0; padding: 2px 5px; border-radius: 3px; font-family: monospace;'>{password}</span></p>
                </div>

                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{loginUrl}' style='background-color: #667eea; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Acessar o Sistema</a>
                </div>

                <div style='background-color: #fff3cd; color: #856404; padding: 15px; border-radius: 5px; font-size: 0.9em;'>
                    <strong>⚠️ Importante:</strong> Por segurança, recomendamos que você altere sua senha no primeiro acesso ao sistema.
                </div>
                
                <p style='margin-top: 30px; font-size: 0.9em; color: #6c757d;'>
                    Se você tiver alguma dúvida ou precisar de ajuda, nossa equipe está à disposição.
                </p>
            </div>";

        return await SendEmailAsync(toEmail, subject, body, toName);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetUrl)
    {
        var subject = "Redefinição de Senha - Isodoc";
        var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                <h2 style='color: #667eea;'>Redefinição de Senha</h2>
                <p>Olá <strong>{toName}</strong>,</p>
                <p>Recebemos uma solicitação para redefinir a senha da sua conta no Isodoc.</p>
                <p>Clique no botão abaixo para criar uma nova senha:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetUrl}' style='background-color: #667eea; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Redefinir Minha Senha</a>
                </div>
                <p>Se você não solicitou esta alteração, pode ignorar este email com segurança.</p>
            </div>";

        return await SendEmailAsync(toEmail, subject, body, toName);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? toName = null)
    {
        try
        {
            _logger.LogInformation($"Tentando enviar email para {toEmail} via SMTP...");

            using var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            message.To.Add(new MailAddress(toEmail, toName));
            message.Subject = subject;
            message.Body = htmlContent;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
            client.Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
            client.EnableSsl = true;
            
            await client.SendMailAsync(message);
            
            _logger.LogInformation($"✅ Email enviado com sucesso para {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Erro ao enviar email para {toEmail}: {ex.Message}");
            // Fallback para log no console em caso de erro, para não travar o fluxo
            _logger.LogInformation("================ EMAIL (FALLBACK LOG) ================");
            _logger.LogInformation($"Para: {toName} <{toEmail}>");
            _logger.LogInformation($"Assunto: {subject}");
            _logger.LogInformation($"Conteúdo: {htmlContent}");
            _logger.LogInformation("======================================================");
            return false;
        }
    }
}
