using System;

namespace Shared.Constants
{
    /// <summary>
    /// Constants for Blood Request Status management
    /// </summary>
    public static class BloodRequestStatus
    {
        public const string Pending = "Pending";                    // V?a t?o, ch? x? lý
        public const string Processing = "Processing";              // ?ang x? lý (thi?u máu, t?o appointment)
        public const string AwaitingDonation = "AwaitingDonation";  // Ch? donor hi?n máu (appointment approved)
        public const string DonorConfirmed = "DonorConfirmed";      // Donor ?ã xác nh?n, ch? ??n ngày hi?n
        public const string DonationInProgress = "DonationInProgress"; // Donation ?ang di?n ra
        public const string Fulfilled = "Fulfilled";               // ?ã có máu
        public const string PickedUp = "PickedUp";                 // ?ã l?y máu (internal status)
        public const string Cancelled = "Cancelled";               // ?ã h?y
        public const string Expired = "Expired";                   // H?t h?n
        
        /// <summary>
        /// Get all valid status values
        /// </summary>
        public static readonly string[] AllStatuses = 
        {
            Pending, Processing, AwaitingDonation, DonorConfirmed, 
            DonationInProgress, Fulfilled, PickedUp, Cancelled, Expired
        };
        
        /// <summary>
        /// Active statuses (ch?a hoàn thành ho?c h?y)
        /// </summary>
        public static readonly string[] ActiveStatuses = 
        {
            Pending, Processing, AwaitingDonation, DonorConfirmed, DonationInProgress
        };
        
        /// <summary>
        /// Completed statuses (?ã hoàn thành)
        /// </summary>
        public static readonly string[] CompletedStatuses = 
        {
            Fulfilled, PickedUp
        };
        
        /// <summary>
        /// Terminal statuses (không th? thay ??i)
        /// </summary>
        public static readonly string[] TerminalStatuses = 
        {
            PickedUp, Cancelled, Expired
        };
    }
    
    /// <summary>
    /// Constants for Donation Appointment Status
    /// </summary>
    public static class AppointmentStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
        public const string CheckedIn = "CheckedIn";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Incomplete = "Incomplete";
    }
    
    /// <summary>
    /// Constants for Donation Event Status
    /// </summary>
    public static class DonationEventStatus
    {
        public const string Created = "Created";
        public const string DonorAssigned = "DonorAssigned";
        public const string Scheduled = "Scheduled";
        public const string CheckedIn = "CheckedIn";
        public const string WalkIn = "WalkIn";
        public const string HealthCheckPassed = "HealthCheckPassed";
        public const string HealthCheckFailed = "HealthCheckFailed";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Incomplete = "Incomplete";
    }
}