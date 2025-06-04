using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        
        // These methods are kept for backward compatibility
        // but will be simplified since we're not doing email verification
        Task SendVerificationEmailAsync(string email, string userId, string verificationToken);
        Task SendPasswordResetEmailAsync(string email, string resetToken);
    }
}