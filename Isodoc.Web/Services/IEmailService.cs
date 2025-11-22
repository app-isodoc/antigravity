using System.Threading.Tasks;

namespace Isodoc.Web.Services;

public interface IEmailService
{
    Task<bool> SendWelcomeEmailAsync(string toEmail, string toName, string clientName, string password, string loginUrl);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetUrl);
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? toName = null);
}
