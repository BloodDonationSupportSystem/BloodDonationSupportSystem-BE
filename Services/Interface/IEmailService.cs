using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IEmailService
    {
        /// <summary>
        /// G?i email ??n m?t ??a ch?
        /// </summary>
        /// <param name="to">??a ch? email ng??i nh?n</param>
        /// <param name="subject">Tiêu ?? email</param>
        /// <param name="body">N?i dung email (HTML)</param>
        /// <returns>True n?u g?i thành công</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body);

        /// <summary>
        /// G?i email ??n nhi?u ??a ch?
        /// </summary>
        /// <param name="toAddresses">Danh sách ??a ch? email ng??i nh?n</param>
        /// <param name="subject">Tiêu ?? email</param>
        /// <param name="body">N?i dung email (HTML)</param>
        /// <returns>True n?u g?i thành công</returns>
        Task<bool> SendEmailAsync(List<string> toAddresses, string subject, string body);

        /// <summary>
        /// G?i email thông báo cho donor v? l?ch h?n hi?n máu
        /// </summary>
        /// <param name="donorEmail">Email c?a donor</param>
        /// <param name="donorName">Tên c?a donor</param>
        /// <param name="appointmentDate">Ngày h?n</param>
        /// <param name="appointmentTime">Kho?ng th?i gian</param>
        /// <param name="location">??a ?i?m hi?n máu</param>
        /// <param name="status">Tr?ng thái cu?c h?n</param>
        /// <param name="notes">Ghi chú b? sung (n?u có)</param>
        /// <returns>True n?u g?i thành công</returns>
        Task<bool> SendAppointmentEmailAsync(
            string donorEmail,
            string donorName,
            DateTimeOffset appointmentDate,
            string appointmentTime,
            string location,
            string status,
            string notes = null);

        /// <summary>
        /// G?i email th? nghi?m ?? ki?m tra c?u hình SMTP
        /// </summary>
        /// <param name="to">??a ch? email ng??i nh?n</param>
        /// <returns>Tuple g?m tr?ng thái thành công và thông ?i?p ch?n ?oán</returns>
        Task<(bool success, string message)> SendTestEmailAsync(string to);
    }
}