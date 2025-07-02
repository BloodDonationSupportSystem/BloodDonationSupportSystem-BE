using System;
using Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class UpdateDonorBloodGroupDto
    {
        [Required(ErrorMessage = "Donor profile ID is required")]
        public Guid DonorProfileId { get; set; }

        [Required(ErrorMessage = "Blood group ID is required")]
        public Guid BloodGroupId { get; set; }

        public string Notes { get; set; }
    }
    public class DonorProfileDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public DateTimeOffset? LastDonationDate { get; set; } // Changed to nullable
        public string HealthStatus { get; set; }
        public DateTimeOffset LastHealthCheckDate { get; set; }
        public int TotalDonations { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? NextAvailableDonationDate { get; set; }
        public bool IsAvailableForEmergency { get; set; }
        public string PreferredDonationTime { get; set; }
        public double? DistanceKm { get; set; } // Added for distance-based search results
        public bool? IsEligible { get; set; } // Indicates if donor is eligible to donate
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class WalkInDonorProfileDto
    {
        // Thông tin cá nhân b?t bu?c
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTimeOffset DateOfBirth { get; set; }

        [Required]
        public Guid BloodGroupId { get; set; }

        // Thông tin nh?n d?ng
        [Required]
        public string IdentityNumber { get; set; }

        // Thông tin liên h? và ??a ch? (có th? không b?t bu?c)
        public string Email { get; set; }

        public string Address { get; set; }

        // Thông tin s?c kh?e
        public bool HasDonatedBefore { get; set; }

        public DateTimeOffset? LastDonationDate { get; set; }

        // Thông tin b? sung
        public string Notes { get; set; }
    }

    public class CreateDonorProfileDto
    {
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTimeOffset DateOfBirth { get; set; }

        public bool Gender { get; set; }

        // Changed to nullable and removed Required attribute
        public DateTimeOffset? LastDonationDate { get; set; }

        [Required(ErrorMessage = "Health status is required")]
        [StringLength(100, ErrorMessage = "Health status cannot be longer than 100 characters")]
        public string HealthStatus { get; set; }

        [Required(ErrorMessage = "Last health check date is required")]
        public DateTimeOffset LastHealthCheckDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total donations must be a positive number")]
        public int TotalDonations { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Blood group ID is required")]
        public Guid BloodGroupId { get; set; }

        public DateTimeOffset? NextAvailableDonationDate { get; set; }
        
        [Display(Name = "Available for emergency donation")]
        public bool IsAvailableForEmergency { get; set; } = false;
        
        [StringLength(50, ErrorMessage = "Preferred donation time cannot be longer than 50 characters")]
        [Display(Name = "Preferred donation time (e.g. Morning, Afternoon, Evening)")]
        public string PreferredDonationTime { get; set; }
    }

    public class UpdateDonorProfileDto
    {
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTimeOffset DateOfBirth { get; set; }

        public bool Gender { get; set; }

        // Changed to nullable and removed Required attribute
        public DateTimeOffset? LastDonationDate { get; set; }

        [Required(ErrorMessage = "Health status is required")]
        [StringLength(100, ErrorMessage = "Health status cannot be longer than 100 characters")]
        public string HealthStatus { get; set; }

        [Required(ErrorMessage = "Last health check date is required")]
        public DateTimeOffset LastHealthCheckDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total donations must be a positive number")]
        public int TotalDonations { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        [Required(ErrorMessage = "Blood group ID is required")]
        public Guid BloodGroupId { get; set; }

        public DateTimeOffset? NextAvailableDonationDate { get; set; }
        
        [Display(Name = "Available for emergency donation")]
        public bool IsAvailableForEmergency { get; set; }
        
        [StringLength(50, ErrorMessage = "Preferred donation time cannot be longer than 50 characters")]
        [Display(Name = "Preferred donation time (e.g. Morning, Afternoon, Evening)")]
        public string PreferredDonationTime { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// Parameters for filtering, sorting, and paging donor profiles.
    /// All parameters are nullable to allow for fetching all donor profiles by default.
    /// </summary>
    public class DonorProfileParameters : PaginationParameters
    {
        /// <summary>
        /// Filter by blood group name. Null to include all blood groups.
        /// </summary>
        public string? BloodGroup { get; set; }
        
        /// <summary>
        /// Filter by health status. Null to include all health statuses.
        /// </summary>
        public string? HealthStatus { get; set; }
        
        /// <summary>
        /// Filter by minimum number of donations. Null to include all donors regardless of donation count.
        /// </summary>
        public int? MinimumDonations { get; set; }
        
        /// <summary>
        /// Filter by availability now. Null to include all donors regardless of current availability.
        /// </summary>
        public bool? IsAvailableNow { get; set; }
        
        /// <summary>
        /// Filter by emergency availability. Null to include all donors regardless of emergency availability.
        /// </summary>
        public bool? IsAvailableForEmergency { get; set; }
        
        /// <summary>
        /// Filter by availability after a specified date. Null to include all donors regardless of future availability.
        /// </summary>
        public DateTimeOffset? AvailableAfter { get; set; }
        
        /// <summary>
        /// Filter by availability before a specified date. Null to include all donors regardless of past availability.
        /// </summary>
        public DateTimeOffset? AvailableBefore { get; set; }
        
        /// <summary>
        /// Filter by preferred donation time. Null to include all preferred donation times.
        /// </summary>
        public string? PreferredDonationTime { get; set; }
        
        /// <summary>
        /// Filter by eligibility status. If true, only show donors who are eligible to donate.
        /// If false, only show donors who are not eligible. If null, show all donors regardless of eligibility.
        /// Note: This filter is applied in memory after database query, as eligibility is determined at runtime.
        /// </summary>
        public bool? IsEligible { get; set; }
        
        // Location-based search parameters
        /// <summary>
        /// Filter by latitude for location-based searches. Used with Longitude and RadiusKm.
        /// </summary>
        public double? Latitude { get; set; }
        
        /// <summary>
        /// Filter by longitude for location-based searches. Used with Latitude and RadiusKm.
        /// </summary>
        public double? Longitude { get; set; }
        
        /// <summary>
        /// Filter by radius in kilometers for location-based searches. Used with Latitude and Longitude.
        /// </summary>
        public double? RadiusKm { get; set; }
    }

    public class UpdateDonationAvailabilityDto
    {
        [Required]
        public Guid DonorProfileId { get; set; }
        
        public DateTimeOffset? NextAvailableDonationDate { get; set; }
        
        public bool IsAvailableForEmergency { get; set; }
        
        [StringLength(50, ErrorMessage = "Preferred donation time cannot be longer than 50 characters")]
        public string PreferredDonationTime { get; set; }
    }

    public class NearbyDonorSearchDto
    {
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }
        
        [Required(ErrorMessage = "Radius is required")]
        [Range(0.1, 500, ErrorMessage = "Radius must be between 0.1 and 500 kilometers")]
        public double RadiusKm { get; set; } = 10.0;
        
        public Guid? BloodGroupId { get; set; }
        
        public bool? IsAvailableNow { get; set; }
        
        public bool? IsAvailableForEmergency { get; set; }
    }

    public class DonorEligibilityResultDto
    {
        public bool IsEligible { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset? NextAvailableDonationDate { get; set; }
        public PendingAppointmentDto? PendingAppointment { get; set; }
    }

    public class PendingAppointmentDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset PreferredDate { get; set; }
        public string PreferredTimeSlot { get; set; } = string.Empty;
        public string? LocationName { get; set; }
        public string? Notes { get; set; }
    }
}