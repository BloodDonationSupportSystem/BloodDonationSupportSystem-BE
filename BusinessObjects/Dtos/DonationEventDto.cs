using System;
using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace BusinessObjects.Dtos
{
    /// <summary>
    /// DTO for displaying donation event details
    /// </summary>
    public class DonationEventDto
    {
        public Guid Id { get; set; }
        
        // Donor Information
        public Guid? DonorId { get; set; }
        public string? DonorName { get; set; }
        public string? DonorEmail { get; set; }
        public string? DonorPhone { get; set; }
        
        // Blood Information
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; } = string.Empty;
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; } = string.Empty;
        
        // Location and Staff
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;
        public Guid? StaffId { get; set; }
        public string? StaffName { get; set; }
        
        // Request Information
        public string RequestType { get; set; } = string.Empty; // "Appointment", "BloodRequest", "DirectDonation"
        public Guid? RequestId { get; set; }
        
        // Appointment Details
        public DateTimeOffset? AppointmentDate { get; set; }
        public string? AppointmentLocation { get; set; }
        public bool AppointmentConfirmed { get; set; }
        
        // Status and Progress
        public string Status { get; set; } = string.Empty;
        public string? StatusDescription { get; set; }
        public string? Notes { get; set; }
        
        // Health Check Information
        public string? BloodPressure { get; set; }
        public double? Temperature { get; set; }
        public double? HemoglobinLevel { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string? MedicalNotes { get; set; }
        public string? RejectionReason { get; set; }
        
        // Donation Details
        public DateTimeOffset? DonationDate { get; set; }
        public DateTimeOffset? DonationStartTime { get; set; }
        public double? QuantityDonated { get; set; }
        public double? QuantityUnits { get; set; }
        public bool IsUsable { get; set; }
        
        // Complications
        public string? ComplicationType { get; set; }
        public string? ComplicationDetails { get; set; }
        public string? ActionTaken { get; set; }
        
        // Inventory Connection
        public Guid? InventoryId { get; set; }
        
        // Timestamps
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? CheckInTime { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public bool IsActive { get; set; }
    }
    
    /// <summary>
    /// DTO for creating donation events
    /// </summary>
    public class CreateDonationEventDto
    {
        [Required(ErrorMessage = "Request ID is required")]
        public Guid RequestId { get; set; }
        
        [Required(ErrorMessage = "Request type is required")]
        [RegularExpression("^(BloodRequest)$", ErrorMessage = "Request type must be BloodRequest")]
        public string RequestType { get; set; } = "BloodRequest";
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
        
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
        
        public bool CheckInventoryFirst { get; set; } = true;
    }
    
    /// <summary>
    /// DTO for creating walk-in donation events
    /// </summary>
    public class CreateWalkInDonationEventDto
    {
        [Required(ErrorMessage = "Donor information is required")]
        public WalkInDonorInfoDto DonorInfo { get; set; } = new();
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        public Guid? StaffId { get; set; }
        
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
    
    public class WalkInDonorInfoDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTimeOffset DateOfBirth { get; set; }
        
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }
        
        public DateTimeOffset? LastDonationDate { get; set; }
    }
    
    /// <summary>
    /// DTO for updating donation events
    /// </summary>
    public class UpdateDonationEventDto
    {
        public string? Status { get; set; }
        public Guid? DonorId { get; set; }
        public DateTimeOffset? AppointmentDate { get; set; }
        public string? AppointmentLocation { get; set; }
        
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// DTO for checking in appointments
    /// </summary>
    public class CheckInAppointmentDto
    {
        [Required(ErrorMessage = "Appointment ID is required")]
        public Guid AppointmentId { get; set; }
        
        [Required(ErrorMessage = "Check-in time is required")]
        public DateTimeOffset CheckInTime { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// DTO for donor health check
    /// </summary>
    public class DonorHealthCheckDto
    {
        [Required(ErrorMessage = "Donation event ID is required")]
        public Guid DonationEventId { get; set; }
        
        [Required(ErrorMessage = "Blood pressure is required")]
        public string BloodPressure { get; set; } = string.Empty;
        
        [Range(35.0, 42.0, ErrorMessage = "Temperature must be between 35°C and 42°C")]
        public double Temperature { get; set; }
        
        [Range(8.0, 20.0, ErrorMessage = "Hemoglobin level must be between 8.0 and 20.0 g/dL")]
        public double HemoglobinLevel { get; set; }
        
        [Range(40.0, 200.0, ErrorMessage = "Weight must be between 40kg and 200kg")]
        public double Weight { get; set; }
        
        [Range(100.0, 250.0, ErrorMessage = "Height must be between 100cm and 250cm")]
        public double Height { get; set; }
        
        [StringLength(1000, ErrorMessage = "Medical notes cannot exceed 1000 characters")]
        public string? MedicalNotes { get; set; }
        
        [Required(ErrorMessage = "Eligibility status is required")]
        public bool IsEligible { get; set; }
        
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string? RejectionReason { get; set; }
        
        public Guid? VerifiedBloodGroupId { get; set; }
    }
    
    /// <summary>
    /// DTO for starting donation process
    /// </summary>
    public class StartDonationDto
    {
        [Required(ErrorMessage = "Donation event ID is required")]
        public Guid DonationEventId { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// DTO for recording donation complications
    /// </summary>
    public class DonationComplicationDto
    {
        [Required(ErrorMessage = "Donation event ID is required")]
        public Guid DonationEventId { get; set; }
        
        [Required(ErrorMessage = "Complication type is required")]
        [StringLength(100, ErrorMessage = "Complication type cannot exceed 100 characters")]
        public string ComplicationType { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Action taken is required")]
        [StringLength(1000, ErrorMessage = "Action taken cannot exceed 1000 characters")]
        public string ActionTaken { get; set; } = string.Empty;
        
        [Range(0.0, 1000.0, ErrorMessage = "Collected amount must be between 0 and 1000 ml")]
        public double? CollectedAmount { get; set; }
        
        public bool IsUsable { get; set; } = false;
    }
    
    /// <summary>
    /// DTO for completing donation
    /// </summary>
    public class CompleteDonationDto
    {
        [Required(ErrorMessage = "Donation event ID is required")]
        public Guid DonationEventId { get; set; }
        
        [Required(ErrorMessage = "Donation date is required")]
        public DateTimeOffset DonationDate { get; set; }
        
        [Required(ErrorMessage = "Quantity donated is required")]
        [Range(100.0, 500.0, ErrorMessage = "Quantity donated must be between 100ml and 500ml")]
        public double QuantityDonated { get; set; }
        
        [Required(ErrorMessage = "Quantity in units is required")]
        [Range(0.1, 5.0, ErrorMessage = "Quantity units must be between 0.1 and 5.0")]
        public double QuantityUnits { get; set; }
        
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// Parameters for searching donation events
    /// </summary>
    public class DonationEventParameters : PaginationParameters
    {
        public string? Status { get; set; }
        public string? RequestType { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? LocationId { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public Guid? StaffId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsUsable { get; set; }
    }
}