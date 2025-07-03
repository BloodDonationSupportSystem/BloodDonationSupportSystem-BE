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

        // Extended donation event information
        public DonationEventInfoDto DonationEvent { get; set; }
    }



    // Helper DTO for inventory check results
    public class InventoryCheckResultDto
    {
        public Guid RequestId { get; set; }
        public int RequestedUnits { get; set; }
        public int AvailableUnits { get; set; }
        public bool HasSufficientInventory { get; set; }
        public List<InventoryItemDto> InventoryItems { get; set; } = new List<InventoryItemDto>();
    }

    public class InventoryItemDto
    {
        public int Id { get; set; }
        public string BloodGroupName { get; set; }
        public string ComponentTypeName { get; set; }
        public int QuantityUnits { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public int DaysUntilExpiration { get; set; }
    }

    // Simplified DTO with essential donation event information
    public class DonationEventInfoDto
    {
        public Guid Id { get; set; }
        
        // Donor Information
        public Guid? DonorId { get; set; }
        public string DonorName { get; set; }
        public string DonorPhone { get; set; }
        
        // Donation Details
        public DateTimeOffset? DonationDate { get; set; }
        public double? QuantityDonated { get; set; }
        public double? QuantityUnits { get; set; }
        public bool IsUsable { get; set; }
        
        // Location
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
        
        // Timestamps
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
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

    // Thêm DTO mới cho việc cập nhật trạng thái kho máu
    public class UpdateBloodInventoryStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
        public string Notes { get; set; }
    }

    public class BloodInventoryParameters : PaginationParameters
    {
        public string? Status { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public DateTimeOffset? ExpirationStartDate { get; set; }
        public DateTimeOffset? ExpirationEndDate { get; set; }
        public bool? IsExpired { get; set; }
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = false;
    }
}