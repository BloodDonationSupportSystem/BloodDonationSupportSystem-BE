using System;

namespace BusinessObjects.Models
{
    /// <summary>
    /// Model for donation appointment requests - can be initiated by donors or staff
    /// </summary>
    public class DonationAppointmentRequest : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        // Request Details
        public Guid DonorId { get; set; }
        public virtual DonorProfile Donor { get; set; } = null!;
        
        public DateTimeOffset PreferredDate { get; set; }
        public string PreferredTimeSlot { get; set; } = string.Empty; // Morning, Afternoon, Evening
        
        public Guid LocationId { get; set; }
        public virtual Location Location { get; set; } = null!;
        
        public Guid? BloodGroupId { get; set; }
        public virtual BloodGroup? BloodGroup { get; set; }
        
        public Guid? ComponentTypeId { get; set; }
        public virtual ComponentType? ComponentType { get; set; }
        
        // Request Origin and Status
        public string RequestType { get; set; } = string.Empty; // "DonorInitiated", "StaffInitiated"
        public Guid? InitiatedByUserId { get; set; } // User who created the request
        public virtual User? InitiatedByUser { get; set; }
        
        public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected", "Confirmed", "Completed", "Cancelled"
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
        
        // Staff Response
        public Guid? ReviewedByUserId { get; set; }
        public virtual User? ReviewedByUser { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        
        // Final Appointment Details (if approved)
        public DateTimeOffset? ConfirmedDate { get; set; }
        public string? ConfirmedTimeSlot { get; set; }
        public Guid? ConfirmedLocationId { get; set; }
        public virtual Location? ConfirmedLocation { get; set; }
        
        // Donor Response to Staff Assignment
        public bool? DonorAccepted { get; set; } // null = no response, true = accepted, false = rejected
        public DateTimeOffset? DonorResponseAt { get; set; }
        public string? DonorResponseNotes { get; set; }
        
        // Priority and Urgency
        public bool IsUrgent { get; set; } = false;
        public int Priority { get; set; } = 1; // 1 = Normal, 2 = High, 3 = Critical
        
        // Automatic Expiry
        public DateTimeOffset? ExpiresAt { get; set; }
        
        // Timing Details
        public DateTimeOffset? CheckInTime { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        public DateTimeOffset? CancelledTime { get; set; }
    }
}