using System;
using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace BusinessObjects.Dtos
{
    /// <summary>
    /// DTO for displaying donation appointment request details
    /// </summary>
    public class DonationAppointmentRequestDto
    {
        public Guid Id { get; set; }
        
        // Donor Information
        public Guid DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string DonorEmail { get; set; } = string.Empty;
        public string DonorPhone { get; set; } = string.Empty;
        
        // Requested Details
        public DateTimeOffset PreferredDate { get; set; }
        public string PreferredTimeSlot { get; set; } = string.Empty;
        
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;
        
        public Guid? BloodGroupId { get; set; }
        public string? BloodGroupName { get; set; }
        
        public Guid? ComponentTypeId { get; set; }
        public string? ComponentTypeName { get; set; }
        
        // Request Origin and Status
        public string RequestType { get; set; } = string.Empty;
        public Guid? InitiatedByUserId { get; set; }
        public string? InitiatedByUserName { get; set; }
        
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }
        
        // Staff Response
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByUserName { get; set; }
        public DateTimeOffset? ReviewedAt { get; set; }
        
        // Final Appointment Details
        public DateTimeOffset? ConfirmedDate { get; set; }
        public string? ConfirmedTimeSlot { get; set; }
        public Guid? ConfirmedLocationId { get; set; }
        public string? ConfirmedLocationName { get; set; }
        
        // Donor Response
        public bool? DonorAccepted { get; set; }
        public DateTimeOffset? DonorResponseAt { get; set; }
        public string? DonorResponseNotes { get; set; }
        
        // Workflow Connection
        public Guid? WorkflowId { get; set; }
        
        // Priority and Urgency
        public bool IsUrgent { get; set; }
        public int Priority { get; set; }
        
        // Dates
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        
        // Additional Times
        public DateTimeOffset? CheckInTime { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        public DateTimeOffset? CancelledTime { get; set; }
    }
    
    /// <summary>
    /// DTO for donors to create appointment requests
    /// </summary>
    public class CreateDonorAppointmentRequestDto
    {
        [Required(ErrorMessage = "Preferred date is required")]
        public DateTimeOffset PreferredDate { get; set; }
        
        [Required(ErrorMessage = "Preferred time slot is required")]
        [RegularExpression("^(Morning|Afternoon|Evening)$", ErrorMessage = "Time slot must be Morning, Afternoon, or Evening")]
        public string PreferredTimeSlot { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
        
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
        
        public bool IsUrgent { get; set; } = false;
    }
    
    /// <summary>
    /// DTO for staff to create appointment requests (assignments)
    /// </summary>
    public class CreateStaffAppointmentRequestDto
    {
        [Required(ErrorMessage = "Donor ID is required")]
        public Guid DonorId { get; set; }
        
        [Required(ErrorMessage = "Preferred date is required")]
        public DateTimeOffset PreferredDate { get; set; }
        
        [Required(ErrorMessage = "Preferred time slot is required")]
        [RegularExpression("^(Morning|Afternoon|Evening)$", ErrorMessage = "Time slot must be Morning, Afternoon, or Evening")]
        public string PreferredTimeSlot { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
        
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
        
        public bool IsUrgent { get; set; } = false;
        
        [Range(1, 3, ErrorMessage = "Priority must be between 1 (Normal) and 3 (Critical)")]
        public int Priority { get; set; } = 1;
        
        // Optional: Auto-expire after X hours if no response
        public int? AutoExpireHours { get; set; } = 72; // Default 3 days
    }
    
    /// <summary>
    /// DTO for staff to respond to donor requests
    /// </summary>
    public class StaffAppointmentResponseDto
    {
        [Required(ErrorMessage = "Request ID is required")]
        public Guid RequestId { get; set; }
        
        [Required(ErrorMessage = "Response action is required")]
        [RegularExpression("^(Approve|Reject|Modify)$", ErrorMessage = "Action must be Approve, Reject, or Modify")]
        public string Action { get; set; } = string.Empty; // "Approve", "Reject", "Modify"
        
        // For Approve or Modify actions
        public DateTimeOffset? ConfirmedDate { get; set; }
        public string? ConfirmedTimeSlot { get; set; }
        public Guid? ConfirmedLocationId { get; set; }
        
        // For Reject action
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string? RejectionReason { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// DTO for donors to respond to staff assignments
    /// </summary>
    public class DonorAppointmentResponseDto
    {
        [Required(ErrorMessage = "Request ID is required")]
        public Guid RequestId { get; set; }
        
        [Required(ErrorMessage = "Response is required")]
        public bool Accepted { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// DTO for updating appointment requests
    /// </summary>
    public class UpdateAppointmentRequestDto
    {
        public DateTimeOffset? PreferredDate { get; set; }
        public string? PreferredTimeSlot { get; set; }
        public Guid? LocationId { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public string? Notes { get; set; }
        public bool? IsUrgent { get; set; }
        public int? Priority { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public Guid? UpdatedByUserId { get; set; }
    }

    /// <summary>
    /// Parameters for searching appointment requests
    /// </summary>
    public class AppointmentRequestParameters : PaginationParameters
    {
        public string? Status { get; set; }
        public string? RequestType { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? LocationId { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public Guid? InitiatedByUserId { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool? IsUrgent { get; set; }
        public int? Priority { get; set; }
        public bool? RequiresDonorResponse { get; set; }
        public bool? IsExpired { get; set; }
    }
    
    /// <summary>
    /// DTO for available time slots
    /// </summary>
    public class AvailableTimeSlotsDto
    {
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    }
    
    public class TimeSlotDto
    {
        public string TimeSlot { get; set; } = string.Empty; // Morning, Afternoon, Evening
        public int AvailableCapacity { get; set; }
        public int TotalCapacity { get; set; }
        public bool IsAvailable { get; set; }
    }
}