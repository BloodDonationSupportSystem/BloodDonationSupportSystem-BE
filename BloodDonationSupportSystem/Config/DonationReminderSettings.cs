using System;

namespace BloodDonationSupportSystem.Config
{
    public class DonationReminderSettings
    {
        /// <summary>
        /// S? ng�y m?c ??nh tr??c khi ?? ?i?u ki?n ?? g?i nh?c nh?
        /// </summary>
        public int DefaultDaysBeforeEligible { get; set; } = 7;
        
        /// <summary>
        /// Th?i gian l�n l?ch ch?y h�ng ng�y (gi?:ph�t:gi�y)
        /// </summary>
        public string ScheduledRunTime { get; set; } = "08:00:00";
        
        /// <summary>
        /// Kho?ng c�ch t?i thi?u gi?a c�c l?n g?i nh?c nh? (ng�y)
        /// </summary>
        public int MinReminderIntervalDays { get; set; } = 7;
        
        /// <summary>
        /// S? ng�y gi?a c�c l?n hi?n máu
        /// </summary>
        public int DonationIntervalDays { get; set; } = 90;
        
        /// <summary>
        /// S? ng�y gi?a c�c l?n hi?n m�u kh?n c?p
        /// </summary>
        public int EmergencyDonationIntervalDays { get; set; } = 30;
        
        /// <summary>
        /// C� b?t g?i nh?c nh? qua email kh�ng
        /// </summary>
        public bool EnableEmailReminders { get; set; } = true;
        
        /// <summary>
        /// C� b?t g?i nh?c nh? trong ?ng d?ng kh�ng
        /// </summary>
        public bool EnableInAppReminders { get; set; } = true;
    }
}