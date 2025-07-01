using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(to);

                using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Recipient}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(List<string> toAddresses, string subject, string body)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var address in toAddresses)
                {
                    message.To.Add(address);
                }

                using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {RecipientCount} recipients", toAddresses.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to multiple recipients");
                return false;
            }
        }

        public async Task<bool> SendAppointmentEmailAsync(
            string donorEmail,
            string donorName,
            DateTimeOffset appointmentDate,
            string appointmentTime,
            string location,
            string status,
            string notes = null)
        {
            try
            {
                var subject = $"Cập nhật lịch hiến máu - {status}";

                // Tạo body của email bằng StringBuilder
                var bodyBuilder = new StringBuilder();
                bodyBuilder.AppendLine("<html>");
                bodyBuilder.AppendLine("<head>");
                bodyBuilder.AppendLine("<style>");
                bodyBuilder.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
                bodyBuilder.AppendLine(".container { width: 100%; max-width: 600px; margin: 0 auto; padding: 20px; }");
                bodyBuilder.AppendLine(".header { background-color: #e74c3c; color: white; padding: 10px; text-align: center; }");
                bodyBuilder.AppendLine(".content { padding: 20px; }");
                bodyBuilder.AppendLine(".footer { background-color: #f9f9f9; padding: 10px; text-align: center; font-size: 12px; }");
                bodyBuilder.AppendLine(".appointment-details { background-color: #f9f9f9; padding: 15px; margin: 15px 0; border-left: 4px solid #e74c3c; }");
                bodyBuilder.AppendLine(".highlight { color: #e74c3c; font-weight: bold; }");
                bodyBuilder.AppendLine("</style>");
                bodyBuilder.AppendLine("</head>");
                bodyBuilder.AppendLine("<body>");
                bodyBuilder.AppendLine("<div class='container'>");
                bodyBuilder.AppendLine("<div class='header'>");
                bodyBuilder.AppendLine("<h2>Hệ thống Hỗ trợ Hiến máu</h2>");
                bodyBuilder.AppendLine("</div>");
                bodyBuilder.AppendLine("<div class='content'>");
                bodyBuilder.AppendLine($"<p>Xin chào <b>{donorName}</b>,</p>");
                bodyBuilder.AppendLine($"<p>Lịch hẹn hiến máu của bạn đã được <span class='highlight'>{GetStatusVietnamese(status)}</span>.</p>");

                bodyBuilder.AppendLine("<div class='appointment-details'>");
                bodyBuilder.AppendLine("<h3>Chi tiết lịch hẹn:</h3>");
                bodyBuilder.AppendLine($"<p><b>Ngày:</b> {appointmentDate.ToString("dd/MM/yyyy")}</p>");
                bodyBuilder.AppendLine($"<p><b>Thời gian:</b> {GetTimeSlotVietnamese(appointmentTime)}</p>");
                bodyBuilder.AppendLine($"<p><b>Địa điểm:</b> {location}</p>");

                if (!string.IsNullOrEmpty(notes))
                {
                    bodyBuilder.AppendLine($"<p><b>Ghi chú:</b> {notes}</p>");
                }

                bodyBuilder.AppendLine("</div>");

                bodyBuilder.AppendLine($"<p>{GetStatusMessage(status)}</p>");
                bodyBuilder.AppendLine("<p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi qua email hoặc số điện thoại của hệ thống.</p>");
                bodyBuilder.AppendLine("<p>Trân trọng,<br/>Hệ thống Hỗ trợ Hiến máu</p>");
                bodyBuilder.AppendLine("</div>");
                bodyBuilder.AppendLine("<div class='footer'>");
                bodyBuilder.AppendLine("<p>© 2023 Hệ thống Hỗ trợ Hiến máu. Tất cả các quyền được bảo lưu.</p>");
                bodyBuilder.AppendLine("<p>Email này được gửi tự động, vui lòng không trả lời.</p>");
                bodyBuilder.AppendLine("</div>");
                bodyBuilder.AppendLine("</div>");
                bodyBuilder.AppendLine("</body>");
                bodyBuilder.AppendLine("</html>");

                string body = bodyBuilder.ToString();
                return await SendEmailAsync(donorEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send appointment email to {DonorEmail}", donorEmail);
                return false;
            }
        }

        private string GetStatusVietnamese(string status)
        {
            return status switch
            {
                "Pending" => "đang chờ xử lý",
                "Approved" => "được chấp nhận",
                "Rejected" => "bị từ chối",
                "Cancelled" => "đã hủy",
                "Completed" => "hoàn thành",
                "CheckedIn" => "đã check-in",
                "Accepted" => "được chấp nhận",
                _ => status
            };
        }

        private string GetTimeSlotVietnamese(string timeSlot)
        {
            return timeSlot switch
            {
                "Morning" => "Buổi sáng (8:00 - 12:00)",
                "Afternoon" => "Buổi chiều (13:00 - 17:00)",
                "Evening" => "Buổi tối (18:00 - 21:00)",
                _ => timeSlot
            };
        }

        private string GetStatusMessage(string status)
        {
            return status switch
            {
                "Pending" => "Yêu cầu của bạn đang được xem xét. Chúng tôi sẽ thông báo cho bạn khi có kết quả.",
                "Approved" => "Vui lòng đến đúng giờ và mang theo giấy tờ tùy thân. Hãy đảm bảo bạn ăn uống đầy đủ trước khi đến hiến máu.",
                "Rejected" => "Rất tiếc, yêu cầu của bạn đã bị từ chối. Bạn có thể đăng ký lại vào thời gian khác hoặc liên hệ với chúng tôi để biết thêm chi tiết.",
                "Cancelled" => "Yêu cầu của bạn đã được hủy. Bạn có thể đăng ký lại vào thời gian khác.",
                "Completed" => "Cảm ơn bạn đã tham gia hiến máu. Hành động của bạn có thể cứu sống nhiều người!",
                "CheckedIn" => "Bạn đã check-in thành công. Vui lòng đợi đến lượt của mình.",
                "Accepted" => "Cảm ơn bạn đã chấp nhận lịch hẹn. Vui lòng đến đúng giờ và mang theo giấy tờ tùy thân.",
                _ => "Cảm ơn bạn đã quan tâm đến việc hiến máu. Sự đóng góp của bạn rất có ý nghĩa."
            };
        }
    }
}