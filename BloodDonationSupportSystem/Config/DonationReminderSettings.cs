using System;

namespace BloodDonationSupportSystem.Config
{
    public class DonationReminderSettings
    {
        /// <summary>
        /// S? ngày m?c ??nh tr??c khi ?? ?i?u ki?n ?? g?i nh?c nh?
        /// </summary>
        public int DefaultDaysBeforeEligible { get; set; } = 7;
        
        /// <summary>
        /// Th?i gian lên l?ch ch?y hàng ngày (gi?:phút:giây)
        /// </summary>
        public string ScheduledRunTime { get; set; } = "08:00:00";
        
        /// <summary>
        /// Kho?ng cách t?i thi?u gi?a các l?n g?i nh?c nh? (ngày)
        /// </summary>
        public int MinReminderIntervalDays { get; set; } = 7;
        
        /// <summary>
        /// S? ngày gi?a các l?n hi?n máu
        /// </summary>
        public int DonationIntervalDays { get; set; } = 90;
        
        /// <summary>
        /// S? ngày gi?a các l?n hi?n máu kh?n c?p
        /// </summary>
        public int EmergencyDonationIntervalDays { get; set; } = 30;
        
        /// <summary>
        /// Có b?t g?i nh?c nh? qua email không
        /// </summary>
        public bool EnableEmailReminders { get; set; } = true;
        
        /// <summary>
        /// Có b?t g?i nh?c nh? trong ?ng d?ng không
        /// </summary>
        public bool EnableInAppReminders { get; set; } = true;
    }
}