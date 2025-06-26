using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    // Location DTOs
    public class LocationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }

        // Capacity information
        public List<LocationCapacityDto> Capacities { get; set; } = new();
        public List<LocationStaffAssignmentDto> StaffAssignments { get; set; } = new();
        public List<LocationOperatingHoursDto> OperatingHours { get; set; } = new();
    }

    public class CreateLocationDto
    {
        [Required(ErrorMessage = "Location name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude is required")]
        public string Latitude { get; set; } = string.Empty;

        [Required(ErrorMessage = "Longitude is required")]
        public string Longitude { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLocationDto
    {
        [Required(ErrorMessage = "Location name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Latitude is required")]
        public string Latitude { get; set; } = string.Empty;

        [Required(ErrorMessage = "Longitude is required")]
        public string Longitude { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // LocationCapacity DTOs
    public class LocationCapacityDto
    {
        public Guid Id { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

    public class CreateLocationCapacityDto
    {
        [Required(ErrorMessage = "Location ID is required")]
        public Guid LocationId { get; set; }

        [Required(ErrorMessage = "Time slot is required")]
        [RegularExpression("^(Morning|Afternoon|Evening)$", ErrorMessage = "Time slot must be Morning, Afternoon, or Evening")]
        public string TimeSlot { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Total capacity must be between 1 and 100")]
        public int TotalCapacity { get; set; } = 10;

        public DayOfWeek? DayOfWeek { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLocationCapacityDto
    {
        [Required(ErrorMessage = "Time slot is required")]
        [RegularExpression("^(Morning|Afternoon|Evening)$", ErrorMessage = "Time slot must be Morning, Afternoon, or Evening")]
        public string TimeSlot { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Total capacity must be between 1 and 100")]
        public int TotalCapacity { get; set; } = 10;

        public DayOfWeek? DayOfWeek { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // LocationStaffAssignment DTOs
    public class LocationStaffAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool CanManageCapacity { get; set; }
        public bool CanApproveAppointments { get; set; }
        public bool CanViewReports { get; set; }
        public DateTimeOffset AssignedDate { get; set; }
        public DateTimeOffset? UnassignedDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

    public class CreateLocationStaffAssignmentDto
    {
        [Required(ErrorMessage = "Location ID is required")]
        public Guid LocationId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(LocationManager|Staff|Supervisor)$", ErrorMessage = "Role must be LocationManager, Staff, or Supervisor")]
        public string Role { get; set; } = string.Empty;

        public bool CanManageCapacity { get; set; } = false;
        public bool CanApproveAppointments { get; set; } = false;
        public bool CanViewReports { get; set; } = false;
        public string? Notes { get; set; }
    }

    public class UpdateLocationStaffAssignmentDto
    {
        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(LocationManager|Staff|Supervisor)$", ErrorMessage = "Role must be LocationManager, Staff, or Supervisor")]
        public string Role { get; set; } = string.Empty;

        public bool CanManageCapacity { get; set; } = false;
        public bool CanApproveAppointments { get; set; } = false;
        public bool CanViewReports { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }

    // LocationOperatingHours DTOs
    public class LocationOperatingHoursDto
    {
        public Guid Id { get; set; }
        public Guid LocationId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekName { get; set; } = string.Empty;
        public TimeSpan MorningStartTime { get; set; }
        public TimeSpan MorningEndTime { get; set; }
        public TimeSpan AfternoonStartTime { get; set; }
        public TimeSpan AfternoonEndTime { get; set; }
        public TimeSpan? EveningStartTime { get; set; }
        public TimeSpan? EveningEndTime { get; set; }
        public bool IsClosed { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateLocationOperatingHoursDto
    {
        [Required(ErrorMessage = "Location ID is required")]
        public Guid LocationId { get; set; }

        [Required(ErrorMessage = "Day of week is required")]
        [Range(0, 6, ErrorMessage = "Day of week must be between 0 (Sunday) and 6 (Saturday)")]
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan MorningStartTime { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan MorningEndTime { get; set; } = new TimeSpan(12, 0, 0);
        public TimeSpan AfternoonStartTime { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan AfternoonEndTime { get; set; } = new TimeSpan(17, 0, 0);
        public TimeSpan? EveningStartTime { get; set; } = new TimeSpan(18, 0, 0);
        public TimeSpan? EveningEndTime { get; set; } = new TimeSpan(21, 0, 0);
        public bool IsClosed { get; set; } = false;
        public string? Notes { get; set; }
    }

    public class UpdateLocationOperatingHoursDto
    {
        public TimeSpan MorningStartTime { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan MorningEndTime { get; set; } = new TimeSpan(12, 0, 0);
        public TimeSpan AfternoonStartTime { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan AfternoonEndTime { get; set; } = new TimeSpan(17, 0, 0);
        public TimeSpan? EveningStartTime { get; set; } = new TimeSpan(18, 0, 0);
        public TimeSpan? EveningEndTime { get; set; } = new TimeSpan(21, 0, 0);
        public bool IsClosed { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }

    // Enhanced AvailableTimeSlotsDto
    public class EnhancedAvailableTimeSlotsDto
    {
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; }
        public bool IsLocationOpen { get; set; }
        public string? ClosedReason { get; set; }
        public List<EnhancedTimeSlotDto> AvailableSlots { get; set; } = new();
    }

    public class EnhancedTimeSlotDto
    {
        public string TimeSlot { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int AvailableCapacity { get; set; }
        public int TotalCapacity { get; set; }
        public int BookedCount { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsOpen { get; set; } // Có m? c?a trong khung gi? này không
        public string? UnavailableReason { get; set; }
    }
}