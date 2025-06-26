using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDonationReminderService
    {
        /// <summary>
        /// Ki?m tra và g?i nh?c nh? cho t?t c? ng??i hi?n máu ?? ?i?u ki?n
        /// </summary>
        /// <param name="daysBeforeEligible">S? ngày tr??c khi ?? ?i?u ki?n ?? g?i nh?c nh?</param>
        /// <returns>S? l??ng nh?c nh? ?ã g?i</returns>
        Task<ApiResponse<int>> CheckAndSendRemindersAsync(int daysBeforeEligible = 7);

        /// <summary>
        /// G?i nh?c nh? c? th? cho m?t ng??i hi?n máu
        /// </summary>
        /// <param name="donorId">ID c?a ng??i hi?n máu</param>
        /// <param name="daysBeforeEligible">S? ngày tr??c khi ?? ?i?u ki?n ?? g?i nh?c nh?</param>
        /// <returns>Thông tin v? nh?c nh? ?ã g?i</returns>
        Task<ApiResponse<NotificationDto>> SendReminderAsync(Guid donorId, int daysBeforeEligible = 7);
        
        /// <summary>
        /// T?o cài ??t nh?c nh? m?i cho ng??i hi?n máu
        /// </summary>
        /// <param name="reminderSettingsDto">Thông tin cài ??t nh?c nh? m?i</param>
        /// <returns>Thông tin cài ??t ?ã t?o</returns>
        Task<ApiResponse<DonorReminderSettingsDto>> CreateReminderSettingsAsync(CreateDonorReminderSettingsDto reminderSettingsDto);
        
        /// <summary>
        /// L?y thông tin cài ??t nh?c nh? c?a ng??i hi?n máu
        /// </summary>
        /// <param name="donorId">ID c?a ng??i hi?n máu</param>
        /// <returns>Thông tin cài ??t nh?c nh?</returns>
        Task<ApiResponse<DonorReminderSettingsDto>> GetReminderSettingsAsync(Guid donorId);
        
        /// <summary>
        /// C?p nh?t cài ??t nh?c nh? c?a ng??i hi?n máu
        /// </summary>
        /// <param name="reminderSettingsDto">Thông tin cài ??t nh?c nh? c?n c?p nh?t</param>
        /// <returns>Thông tin cài ??t ?ã c?p nh?t</returns>
        Task<ApiResponse<DonorReminderSettingsDto>> UpdateReminderSettingsAsync(UpdateDonorReminderSettingsDto reminderSettingsDto);
    }
}