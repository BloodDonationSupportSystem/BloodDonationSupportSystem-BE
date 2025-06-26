using System;
using Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
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
    }

    public class DonorProfileParameters : PaginationParameters
    {
        public string BloodGroup { get; set; }
        public string HealthStatus { get; set; }
        public int? MinimumDonations { get; set; }
        public bool? IsAvailableNow { get; set; }
        public bool? IsAvailableForEmergency { get; set; }
        public DateTimeOffset? AvailableAfter { get; set; }
        public DateTimeOffset? AvailableBefore { get; set; }
        public string PreferredDonationTime { get; set; }
        
        // Location-based search parameters
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
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
}