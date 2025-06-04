using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class BloodInventoryDto
    {
        public int Id { get; set; }
        public int QuantityUnits { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public string Status { get; set; }
        public string InventorySource { get; set; }
        
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }
        
        public Guid DonationEventId { get; set; }
        public string DonorName { get; set; }
    }

    public class CreateBloodInventoryDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Expiration date is required")]
        public DateTimeOffset ExpirationDate { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Inventory source is required")]
        public string InventorySource { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        [Required(ErrorMessage = "Donation event is required")]
        public Guid DonationEventId { get; set; }
    }

    public class UpdateBloodInventoryDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Expiration date is required")]
        public DateTimeOffset ExpirationDate { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Inventory source is required")]
        public string InventorySource { get; set; }
    }

    public class BloodInventoryParameters : PaginationParameters
    {
        public string Status { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public DateTimeOffset? ExpirationStartDate { get; set; }
        public DateTimeOffset? ExpirationEndDate { get; set; }
        public bool? IsExpired { get; set; }
        public string SortBy { get; set; }
        public bool SortAscending { get; set; } = false;
    }
}