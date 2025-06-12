using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    /// <summary>
    /// DTO cho cài đặt nhắc nhở của người hiến máu
    /// </summary>
    public class DonorReminderSettingsDto
    {
        public Guid Id { get; set; }
        public Guid DonorProfileId { get; set; }
        public string DonorName { get; set; }
        
        /// <summary>
        /// Có bật thông báo nhắc nhở không
        /// </summary>
        public bool EnableReminders { get; set; }
        
        /// <summary>
        /// Số ngày trước khi đủ điều kiện hiến máu để gửi nhắc nhở
        /// </summary>
        public int DaysBeforeEligible { get; set; }
        
        /// <summary>
        /// Có nhận thông báo qua email không
        /// </summary>
        public bool EmailNotifications { get; set; }
        
        /// <summary>
        /// Có nhận thông báo trong ứng dụng không
        /// </summary>
        public bool InAppNotifications { get; set; }
        
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }
    
    /// <summary>
    /// DTO cho việc tạo mới cài đặt nhắc nhở
    /// </summary>
    public class CreateDonorReminderSettingsDto
    {
        [Required(ErrorMessage = "Donor profile ID is required")]
        public Guid DonorProfileId { get; set; }
        
        public bool EnableReminders { get; set; } = true;
        
        [Range(1, 30, ErrorMessage = "Days before eligible must be between 1 and 30")]
        public int DaysBeforeEligible { get; set; } = 7;
        
        public bool EmailNotifications { get; set; } = true;
        
        public bool InAppNotifications { get; set; } = true;
    }
    
    /// <summary>
    /// DTO cho việc cập nhật cài đặt nhắc nhở
    /// </summary>
    public class UpdateDonorReminderSettingsDto
    {
        [Required(ErrorMessage = "Donor profile ID is required")]
        public Guid DonorProfileId { get; set; }
        
        public bool EnableReminders { get; set; }
        
        [Range(1, 30, ErrorMessage = "Days before eligible must be between 1 and 30")]
        public int DaysBeforeEligible { get; set; } = 7;
        
        public bool EmailNotifications { get; set; } = true;
        
        public bool InAppNotifications { get; set; } = true;
    }
}