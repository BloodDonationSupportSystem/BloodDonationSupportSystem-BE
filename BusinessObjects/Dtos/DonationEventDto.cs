using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class DonationEventDto
    {
        public Guid Id { get; set; }
        public int QuantityUnits { get; set; }
        public string Status { get; set; }
        public string CollectedAt { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        
        public Guid DonorId { get; set; }
        public string DonorName { get; set; }
        
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }
        
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
    }

    public class CreateDonationEventDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Collection location is required")]
        public string CollectedAt { get; set; }
        
        [Required(ErrorMessage = "Donor ID is required")]
        public Guid DonorId { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
    }

    public class UpdateDonationEventDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Collection location is required")]
        public string CollectedAt { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
    }

    public class DonationEventParameters : PaginationParameters
    {
        public string Status { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public Guid? LocationId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string SortBy { get; set; }
        public bool SortAscending { get; set; } = false;
    }
}