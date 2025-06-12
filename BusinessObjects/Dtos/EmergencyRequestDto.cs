using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class EmergencyRequestDto
    {
        public Guid Id { get; set; }
        public string PatientName { get; set; }
        public string ContactInfo { get; set; }
        public int QuantityUnits { get; set; }
        public string Status { get; set; }
        public string UrgencyLevel { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }
        
        // Location information
        public Guid? LocationId { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public double? DistanceKm { get; set; }
        
        // Hospital information
        public string HospitalName { get; set; }
        
        // Medical notes
        public string MedicalNotes { get; set; }
        
        // Is active
        public bool IsActive { get; set; }
    }

    public class CreateEmergencyRequestDto
    {
        [Required(ErrorMessage = "Patient name is required")]
        [StringLength(100, ErrorMessage = "Patient name cannot be longer than 100 characters")]
        public string PatientName { get; set; }
        
        [Required(ErrorMessage = "Contact information is required")]
        [StringLength(200, ErrorMessage = "Contact information cannot be longer than 200 characters")]
        public string ContactInfo { get; set; }
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Urgency level is required")]
        public string UrgencyLevel { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        // Location information (either LocationId or Address with coordinates should be provided)
        public Guid? LocationId { get; set; }
        
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; }
        
        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public string Latitude { get; set; }
        
        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public string Longitude { get; set; }
        
        // Hospital information
        [StringLength(200, ErrorMessage = "Hospital name cannot be longer than 200 characters")]
        public string HospitalName { get; set; }
        
        // Medical notes
        [StringLength(1000, ErrorMessage = "Medical notes cannot be longer than 1000 characters")]
        public string MedicalNotes { get; set; }
    }

    public class UpdateEmergencyRequestDto
    {
        [Required(ErrorMessage = "Patient name is required")]
        [StringLength(100, ErrorMessage = "Patient name cannot be longer than 100 characters")]
        public string PatientName { get; set; }
        
        [Required(ErrorMessage = "Contact information is required")]
        [StringLength(200, ErrorMessage = "Contact information cannot be longer than 200 characters")]
        public string ContactInfo { get; set; }
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Urgency level is required")]
        public string UrgencyLevel { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        // Location information
        public Guid? LocationId { get; set; }
        
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; }
        
        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public string Latitude { get; set; }
        
        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public string Longitude { get; set; }
        
        // Hospital information
        [StringLength(200, ErrorMessage = "Hospital name cannot be longer than 200 characters")]
        public string HospitalName { get; set; }
        
        // Medical notes
        [StringLength(1000, ErrorMessage = "Medical notes cannot be longer than 1000 characters")]
        public string MedicalNotes { get; set; }
        
        // Is active
        public bool IsActive { get; set; } = true;
    }

    // Thêm DTO mới cho việc cập nhật trạng thái yêu cầu khẩn cấp
    public class UpdateEmergencyRequestStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
        public string Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class EmergencyRequestParameters : PaginationParameters
    {
        public string Status { get; set; }
        public string UrgencyLevel { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string SortBy { get; set; }
        public bool SortAscending { get; set; } = false;
        public bool? IsActive { get; set; }
        
        // Location-based search parameters
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusKm { get; set; }
    }
    
    public class NearbyEmergencyRequestSearchDto
    {
        [Required(ErrorMessage = "Latitude is required")]
        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public double Latitude { get; set; }
        
        [Required(ErrorMessage = "Longitude is required")]
        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public double Longitude { get; set; }
        
        [Required(ErrorMessage = "Radius is required")]
        [Range(0.1, 500, ErrorMessage = "Radius must be between 0.1 and 500 kilometers")]
        public double RadiusKm { get; set; } = 10.0;
        
        public Guid? BloodGroupId { get; set; }
        public string UrgencyLevel { get; set; }
        public bool? IsActive { get; set; } = true;
    }
    
    public class PublicEmergencyRequestDto
    {
        [Required(ErrorMessage = "Patient name is required")]
        [StringLength(100, ErrorMessage = "Patient name cannot be longer than 100 characters")]
        public string PatientName { get; set; }
        
        [Required(ErrorMessage = "Contact information is required")]
        [StringLength(200, ErrorMessage = "Contact information cannot be longer than 200 characters")]
        [Phone(ErrorMessage = "Please provide a valid phone number")]
        public string ContactInfo { get; set; }
        
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; }
        
        [RegularExpression(@"^-?([1-8]?[1-9]|[1-9]0)\.{1}\d{1,6}$", ErrorMessage = "Latitude must be in decimal format (e.g. 41.123456)")]
        public string Latitude { get; set; }
        
        [RegularExpression(@"^-?([1]?[1-7][1-9]|[1]?[1-8][0]|[1-9]?[0-9])\.{1}\d{1,6}$", ErrorMessage = "Longitude must be in decimal format (e.g. -71.123456)")]
        public string Longitude { get; set; }
        
        [Required(ErrorMessage = "Hospital name is required")]
        [StringLength(200, ErrorMessage = "Hospital name cannot be longer than 200 characters")]
        public string HospitalName { get; set; }
        
        [StringLength(1000, ErrorMessage = "Medical notes cannot be longer than 1000 characters")]
        public string MedicalNotes { get; set; }
        
        // Simple CAPTCHA verification
        [Required(ErrorMessage = "CAPTCHA verification is required")]
        public string CaptchaToken { get; set; }
    }
}