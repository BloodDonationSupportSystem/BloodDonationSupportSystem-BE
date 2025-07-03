using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class BloodRequestDto
    {
        public Guid Id { get; set; }
        public int QuantityUnits { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public string Status { get; set; }
        
        // Phân biệt loại yêu cầu
        public bool IsEmergency { get; set; }
        
        // Thông tin cho yêu cầu thường
        public DateTimeOffset? NeededByDate { get; set; }
        public Guid? RequestedBy { get; set; }
        public string RequesterName { get; set; }
        
        // Thông tin cho yêu cầu khẩn cấp
        public string PatientName { get; set; }
        public string UrgencyLevel { get; set; } // Chỉ có giá trị khi IsEmergency = true
        public string ContactInfo { get; set; }
        public string HospitalName { get; set; }
        
        // Thông tin máu
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }
        
        // Thông tin vị trí
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public double? DistanceKm { get; set; } // Added for distance-based search results
        
        // Ghi chú y tế
        public string MedicalNotes { get; set; }
        
        // Trạng thái hoạt động
        public bool IsActive { get; set; }
        
        // Thông tin audit từ BaseEntity
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

    public class EmergencyBloodRequestDto
    {
        public Guid Id { get; set; }
        public string PatientName { get; set; }
        public string UrgencyLevel { get; set; }
        public string ContactInfo { get; set; }
        public string HospitalName { get; set; }
        public int QuantityUnits { get; set; }

        // Blood information
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }

        // Location information
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        // Status and additional information
        public string MedicalNotes { get; set; }
        public string Status { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
    }

    public class CreateBloodRequestDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        // Phân biệt loại yêu cầu
        public bool IsEmergency { get; set; } = false;
        
        // Thông tin cho yêu cầu thường
        public DateTimeOffset? NeededByDate { get; set; } // Required nếu IsEmergency = false
        public Guid? RequestedBy { get; set; } // Required nếu IsEmergency = false
        
        // Thông tin cho yêu cầu khẩn cấp
        [StringLength(100, ErrorMessage = "Patient name cannot be longer than 100 characters")]
        public string PatientName { get; set; } = string.Empty; // Required nếu IsEmergency = true
        
        public string? UrgencyLevel { get; set; } = string.Empty; // Required nếu IsEmergency = true
        
        [StringLength(200, ErrorMessage = "Contact information cannot be longer than 200 characters")]
        public string ContactInfo { get; set; } = string.Empty; // Required nếu IsEmergency = true
        
        [StringLength(200, ErrorMessage = "Hospital name cannot be longer than 200 characters")]
        public string HospitalName { get; set; } = string.Empty;
        
        // Thông tin máu (bắt buộc cho cả hai loại)
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        // Thông tin vị trí (either LocationId or Address with coordinates should be provided)
        public Guid LocationId { get; set; }
        
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; } = string.Empty;
        
        public string Latitude { get; set; } = string.Empty;
        
        public string Longitude { get; set; } = string.Empty;
        
        // Ghi chú y tế
        [StringLength(1000, ErrorMessage = "Medical notes cannot be longer than 1000 characters")]
        public string MedicalNotes { get; set; } = string.Empty;
    }

    public class UpdateBloodRequestDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        // Phân biệt loại yêu cầu
        public bool IsEmergency { get; set; }
        
        // Thông tin cho yêu cầu thường
        public DateTimeOffset? NeededByDate { get; set; }
        
        // Thông tin cho yêu cầu khẩn cấp
        [StringLength(100, ErrorMessage = "Patient name cannot be longer than 100 characters")]
        public string PatientName { get; set; } = string.Empty;
        
        public string UrgencyLevel { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Contact information cannot be longer than 200 characters")]
        public string ContactInfo { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Hospital name cannot be longer than 200 characters")]
        public string HospitalName { get; set; } = string.Empty;
        
        // Thông tin máu
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        // Thông tin vị trí
        public Guid LocationId { get; set; }
        
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; } = string.Empty;
        
        public string Latitude { get; set; } = string.Empty;
        
        public string Longitude { get; set; } = string.Empty;
        
        // Ghi chú y tế
        [StringLength(1000, ErrorMessage = "Medical notes cannot be longer than 1000 characters")]
        public string MedicalNotes { get; set; } = string.Empty;
        
        // Trạng thái hoạt động
        public bool IsActive { get; set; } = true;
    }

    // Thêm DTO mới cho việc cập nhật trạng thái yêu cầu
    public class UpdateBloodRequestStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
        public string Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class BloodRequestParameters : PaginationParameters
    {
        public string? Status { get; set; }
        public string? UrgencyLevel { get; set; } // Thêm để lọc theo mức độ khẩn cấp
        public bool? IsEmergency { get; set; } // Thêm để lọc theo loại yêu cầu
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public Guid? LocationId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string SortBy { get; set; } = "CreatedTime"; // Mặc định sắp xếp theo thời gian tạo
        public bool SortAscending { get; set; } = false;
        public bool? IsActive { get; set; }
        
        // Location-based search parameters
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusKm { get; set; }
    }
    
    public class NearbyBloodRequestSearchDto
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
        public string Status { get; set; }
        public string UrgencyLevel { get; set; } // Thêm để lọc theo mức độ khẩn cấp
        public bool? IsEmergency { get; set; } // Thêm để lọc theo loại yêu cầu
        public DateTimeOffset? NeededBefore { get; set; }
        public bool? IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for creating a public blood request (emergency request from non-authenticated users)
    /// </summary>
    public class PublicBloodRequestDto
    {
        [Required(ErrorMessage = "Patient name is required")]
        [StringLength(100, ErrorMessage = "Patient name cannot exceed 100 characters")]
        public string PatientName { get; set; }

        [Required(ErrorMessage = "Urgency level is required")]
        [StringLength(20, ErrorMessage = "Urgency level cannot exceed 20 characters")]
        public string UrgencyLevel { get; set; } // Critical, High, Medium

        [Required(ErrorMessage = "Contact information is required")]
        [StringLength(100, ErrorMessage = "Contact information cannot exceed 100 characters")]
        public string ContactInfo { get; set; }

        [Required(ErrorMessage = "Hospital name is required")]
        [StringLength(100, ErrorMessage = "Hospital name cannot exceed 100 characters")]
        public string HospitalName { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }

        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }

        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }

        [Required(ErrorMessage = "Location ID is required")]
        public Guid LocationId { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string Address { get; set; }

        [StringLength(30, ErrorMessage = "Latitude cannot exceed 30 characters")]
        public string Latitude { get; set; }

        [StringLength(30, ErrorMessage = "Longitude cannot exceed 30 characters")]
        public string Longitude { get; set; }

        [StringLength(1000, ErrorMessage = "Medical notes cannot exceed 1000 characters")]
        public string MedicalNotes { get; set; }
    }
}