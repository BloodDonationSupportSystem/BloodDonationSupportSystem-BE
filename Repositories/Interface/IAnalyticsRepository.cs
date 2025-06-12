using BusinessObjects.Dtos;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IAnalyticsRepository
    {
        // Dashboard
        Task<DashboardOverviewDto> GetDashboardOverviewAsync();
        Task<List<BloodInventorySummaryDto>> GetBloodInventorySummaryAsync();
        Task<List<TopDonorDto>> GetTopDonorsAsync(int count = 10);
        Task<DonationStatisticsDto> GetDonationStatisticsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate);
        Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10);
        
        // Reports
        Task<BloodDonationReportDto> GetBloodDonationReportAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid? bloodGroupId = null, Guid? locationId = null);
        Task<BloodRequestReportDto> GetBloodRequestReportAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid? bloodGroupId = null, Guid? locationId = null);
        Task<InventoryReportDto> GetInventoryReportAsync(DateTimeOffset? asOfDate = null);
        Task<DonorDemographicsReportDto> GetDonorDemographicsReportAsync();
    }
}