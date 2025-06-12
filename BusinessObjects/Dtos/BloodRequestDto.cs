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
        public DateTimeOffset NeededByDate { get; set; }
        
        public Guid RequestedBy { get; set; }
        public string RequesterName { get; set; }
        
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }
        
        public Guid LocationId { get; set; }
        public string LocationName { get; set; }
        
        // Location details for distance calculation
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public double? DistanceKm { get; set; } // Added for distance-based search results
    }

    public class CreateBloodRequestDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Needed by date is required")]
        public DateTimeOffset NeededByDate { get; set; }
        
        [Required(ErrorMessage = "Requester ID is required")]
        public Guid RequestedBy { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
    }

    public class UpdateBloodRequestDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int QuantityUnits { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "Needed by date is required")]
        public DateTimeOffset NeededByDate { get; set; }
        
        [Required(ErrorMessage = "Blood group is required")]
        public Guid BloodGroupId { get; set; }
        
        [Required(ErrorMessage = "Component type is required")]
        public Guid ComponentTypeId { get; set; }
        
        [Required(ErrorMessage = "Location is required")]
        public Guid LocationId { get; set; }
    }

    public class BloodRequestParameters : PaginationParameters
    {
        public string Status { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public Guid? LocationId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        
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
        
        public DateTimeOffset? NeededBefore { get; set; }
    }
}