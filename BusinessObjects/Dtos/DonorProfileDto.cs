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
        public DateTimeOffset LastDonationDate { get; set; }
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
    }

    public class CreateDonorProfileDto
    {
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTimeOffset DateOfBirth { get; set; }

        public bool Gender { get; set; }

        [Required(ErrorMessage = "Last donation date is required")]
        public DateTimeOffset LastDonationDate { get; set; }

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

        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public string Latitude { get; set; }

        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public string Longitude { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Blood group ID is required")]
        public Guid BloodGroupId { get; set; }
    }

    public class UpdateDonorProfileDto
    {
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTimeOffset DateOfBirth { get; set; }

        public bool Gender { get; set; }

        [Required(ErrorMessage = "Last donation date is required")]
        public DateTimeOffset LastDonationDate { get; set; }

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

        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public string Latitude { get; set; }

        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public string Longitude { get; set; }

        [Required(ErrorMessage = "Blood group ID is required")]
        public Guid BloodGroupId { get; set; }
    }

    public class DonorProfileParameters : PaginationParameters
    {
        public string BloodGroup { get; set; }
        public string HealthStatus { get; set; }
        public int? MinimumDonations { get; set; }
    }
}