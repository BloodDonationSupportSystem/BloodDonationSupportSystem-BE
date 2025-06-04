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
    }
}