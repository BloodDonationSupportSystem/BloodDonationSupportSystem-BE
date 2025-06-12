using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class DonorReminderSettings
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid DonorProfileId { get; set; }
        
        [ForeignKey("DonorProfileId")]
        public virtual DonorProfile DonorProfile { get; set; }
        
        /// <summary>
        /// Có b?t thông báo nh?c nh? không
        /// </summary>
        public bool EnableReminders { get; set; } = true;
        
        /// <summary>
        /// S? ngày tr??c khi ?? ?i?u ki?n hi?n máu ?? g?i nh?c nh?
        /// </summary>
        public int DaysBeforeEligible { get; set; } = 7;
        
        /// <summary>
        /// Có nh?n thông báo qua email không
        /// </summary>
        public bool EmailNotifications { get; set; } = true;
        
        /// <summary>
        /// Có nh?n thông báo trong ?ng d?ng không
        /// </summary>
        public bool InAppNotifications { get; set; } = true;
        
        /// <summary>
        /// Ngày gi? t?o b?n ghi
        /// </summary>
        public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
        
        /// <summary>
        /// Ngày gi? c?p nh?t g?n nh?t
        /// </summary>
        public DateTimeOffset? LastUpdatedTime { get; set; }
        
        /// <summary>
        /// Th?i gian g?i thông báo g?n nh?t
        /// </summary>
        public DateTimeOffset? LastReminderSentTime { get; set; }
    }
}