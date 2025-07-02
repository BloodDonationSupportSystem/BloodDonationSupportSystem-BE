using System;
using System.Collections.Generic;
using Shared.Models;

namespace BusinessObjects.Dtos
{
    #region Dashboard DTOs
    
    public class DashboardOverviewDto
    {
        public int TotalDonors { get; set; }
        public int TotalDonations { get; set; }
        public int PendingRequests { get; set; }
        public int AvailableInventory { get; set; }
        public int ExpiringSoonInventory { get; set; }
        public int EmergencyRequests { get; set; }
        public int ScheduledAppointments { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }

    public class BloodInventorySummaryDto
    {
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; } = string.Empty;
        public List<ComponentInventoryDto> Components { get; set; } = new List<ComponentInventoryDto>();
        public int TotalUnits { get; set; }
        public int CriticalThreshold { get; set; }
        public bool IsBelowThreshold => TotalUnits < CriticalThreshold;
    }

    public class ComponentInventoryDto
    {
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; } = string.Empty;
        public int AvailableUnits { get; set; }
        public int ExpiringSoonUnits { get; set; }
        public int CriticalThreshold { get; set; }
        public bool IsBelowThreshold => AvailableUnits < CriticalThreshold;
    }

    public class TopDonorDto
    {
        public Guid DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public int TotalDonations { get; set; }
        public DateTimeOffset? LastDonationDate { get; set; }
        public string BloodGroupName { get; set; } = string.Empty;
    }

    public class DonationStatisticsDto
    {
        public List<DateBasedStatDto> DailyDonations { get; set; } = new List<DateBasedStatDto>();
        public List<DateBasedStatDto> MonthlyDonations { get; set; } = new List<DateBasedStatDto>();
        public List<BloodGroupStatDto> DonationsByBloodGroup { get; set; } = new List<BloodGroupStatDto>();
        public List<LocationStatDto> DonationsByLocation { get; set; } = new List<LocationStatDto>();
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int TotalDonations { get; set; }
    }

    public class DateBasedStatDto
    {
        public DateTimeOffset Date { get; set; }
        public int Count { get; set; }
    }

    public class BloodGroupStatDto
    {
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class LocationStatDto
    {
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class RecentActivityDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    #endregion

    #region Report DTOs

    public class BloodDonationReportDto
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int TotalDonations { get; set; }
        public int UniqueDonoCount { get; set; }
        public int NewDonorCount { get; set; }
        public int RepeatDonorCount { get; set; }
        public List<BloodGroupStatDto> DonationsByBloodGroup { get; set; } = new List<BloodGroupStatDto>();
        public List<ComponentStatDto> DonationsByComponentType { get; set; } = new List<ComponentStatDto>();
        public List<DateBasedStatDto> DonationTrend { get; set; } = new List<DateBasedStatDto>();
        public List<DonationEventStatDto> DonationsByEvent { get; set; } = new List<DonationEventStatDto>();
        public List<LocationStatDto> DonationsByLocation { get; set; } = new List<LocationStatDto>();
        public double AverageDonationsPerDay { get; set; }
    }

    public class ComponentStatDto
    {
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class DonationEventStatDto
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int DonationCount { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class BloodRequestReportDto
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int TotalRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public int PendingRequests { get; set; }
        public int CancelledRequests { get; set; }
        public List<BloodGroupStatDto> RequestsByBloodGroup { get; set; } = new List<BloodGroupStatDto>();
        public List<ComponentStatDto> RequestsByComponentType { get; set; } = new List<ComponentStatDto>();
        public List<RequestPriorityStatDto> RequestsByPriority { get; set; } = new List<RequestPriorityStatDto>();
        public List<DateBasedStatDto> RequestTrend { get; set; } = new List<DateBasedStatDto>();
        public List<LocationStatDto> RequestsByLocation { get; set; } = new List<LocationStatDto>();
        public double AverageFulfillmentTime { get; set; } // Th?i gian trung bình ?? ?áp ?ng yêu c?u (gi?)
        public double FulfillmentRate { get; set; } // T? l? ?áp ?ng (%)
    }

    public class RequestPriorityStatDto
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class InventoryReportDto
    {
        public DateTimeOffset ReportDate { get; set; }
        public int TotalInventoryUnits { get; set; }
        public int ExpiredUnits { get; set; }
        public int ExpiringSoonUnits { get; set; }
        public List<BloodGroupInventoryStatDto> InventoryByBloodGroup { get; set; } = new List<BloodGroupInventoryStatDto>();
        public List<InventoryMovementStatDto> InventoryMovements { get; set; } = new List<InventoryMovementStatDto>();
        public double AverageStorageTime { get; set; } // Th?i gian trung bình l?u tr? (ngày)
        public double ExpirationRate { get; set; } // T? l? h?t h?n (%)
    }

    public class BloodGroupInventoryStatDto
    {
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; } = string.Empty;
        public int AvailableUnits { get; set; }
        public int ExpiringSoonUnits { get; set; }
        public List<ComponentInventoryStatDto> ComponentBreakdown { get; set; } = new List<ComponentInventoryStatDto>();
    }

    public class ComponentInventoryStatDto
    {
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; } = string.Empty;
        public int AvailableUnits { get; set; }
        public int ExpiringSoonUnits { get; set; }
    }

    public class InventoryMovementStatDto
    {
        public string MovementType { get; set; } = string.Empty; // Nh?p kho, Xu?t kho, H?t h?n
        public DateTimeOffset Date { get; set; }
        public int Quantity { get; set; }
    }
    
    public class DonorDemographicsReportDto
    {
        public int TotalDonors { get; set; }
        public List<AgeGroupStatDto> DonorsByAgeGroup { get; set; } = new List<AgeGroupStatDto>();
        public List<GenderStatDto> DonorsByGender { get; set; } = new List<GenderStatDto>();
        public List<BloodGroupStatDto> DonorsByBloodGroup { get; set; } = new List<BloodGroupStatDto>();
        public List<LocationStatDto> DonorsByLocation { get; set; } = new List<LocationStatDto>();
        public List<DonationFrequencyStatDto> DonorsByDonationFrequency { get; set; } = new List<DonationFrequencyStatDto>();
        public double AverageDonationsPerDonor { get; set; }
    }

    public class AgeGroupStatDto
    {
        public string AgeGroup { get; set; } = string.Empty; // e.g., "18-24", "25-34", "35-44", "45-54", "55-65"
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class GenderStatDto
    {
        public bool Gender { get; set; } // true: Nam, false: N?
        public string GenderName { get; set; } = string.Empty; // "Nam" ho?c "N?"
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class DonationFrequencyStatDto
    {
        public string FrequencyGroup { get; set; } = string.Empty; // e.g., "First-time", "2-5 times", "6-10 times", "11+ times"
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    // Parameters for Dashboard and Report
    public class ReportParameters
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public Guid? LocationId { get; set; }
        public string ReportType { get; set; } = string.Empty; // "Donation", "Request", "Inventory", "Donor"
        public string GroupBy { get; set; } = string.Empty; // "Day", "Week", "Month", "Year"
        public string Format { get; set; } = string.Empty; // "PDF", "Excel", "CSV"
    }

    #endregion
}