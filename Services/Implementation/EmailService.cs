using Microsoft.Extensions.Options;
using Services.Interface;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(new MailAddress(to));

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
            }
            catch (Exception)
            {
                // Log the exception
                // For now, we'll just re-throw it
                throw;
            }
        }

        // Simplified version that doesn't actually send emails
        // since we're auto-verifying all registrations
        public Task SendVerificationEmailAsync(string email, string userId, string verificationToken)
        {
            // Simply return a completed task since we don't need to send verification emails
            return Task.CompletedTask;
        }

        // Simplified version that doesn't actually send emails
        public Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            // Simply return a completed task since we're not using email for password resets
            return Task.CompletedTask;
        }
    }

    public class EmailSettings
    {
        public string FromEmail { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApplicationUrl { get; set; }
    }
}