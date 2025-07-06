using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    /// <summary>
    /// Service interface for dashboard-related functionality
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Get dashboard data for a Member user
        /// </summary>
        /// <param name="userId">The ID of the member</param>
        /// <returns>ApiResponse containing member dashboard data</returns>
        Task<ApiResponse<MemberDashboardDto>> GetMemberDashboardAsync(Guid userId);

        /// <summary>
        /// Get dashboard data for a Staff user
        /// </summary>
        /// <returns>ApiResponse containing staff dashboard data</returns>
        Task<ApiResponse<StaffDashboardDto>> GetStaffDashboardAsync();

        /// <summary>
        /// Get dashboard data for an Admin user
        /// </summary>
        /// <returns>ApiResponse containing admin dashboard data</returns>
        Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync();

        /// <summary>
        /// Get emergency dashboard data with real-time updates for Staff
        /// </summary>
        /// <returns>ApiResponse containing emergency dashboard data</returns>
        Task<ApiResponse<EmergencyDashboardDto>> GetEmergencyDashboardAsync();

        /// <summary>
        /// Get donation statistics for a specific time period
        /// </summary>
        /// <param name="timeframe">The timeframe for the statistics</param>
        /// <returns>ApiResponse containing donation statistics</returns>
        Task<ApiResponse<DonationStatisticsDto>> GetDonationStatisticsAsync(StatisticsTimeframeDto timeframe);

        /// <summary>
        /// Get blood request statistics for a specific time period
        /// </summary>
        /// <param name="timeframe">The timeframe for the statistics</param>
        /// <returns>ApiResponse containing blood request statistics</returns>
        Task<ApiResponse<RequestStatisticsDto>> GetRequestStatisticsAsync(StatisticsTimeframeDto timeframe);

        /// <summary>
        /// Get blood inventory statistics
        /// </summary>
        /// <returns>ApiResponse containing inventory statistics</returns>
        Task<ApiResponse<InventoryStatisticsDto>> GetInventoryStatisticsAsync();

        /// <summary>
        /// Get donor statistics
        /// </summary>
        /// <returns>ApiResponse containing donor statistics</returns>
        Task<ApiResponse<DonorStatisticsDto>> GetDonorStatisticsAsync();

        /// <summary>
        /// Get activity statistics for a member
        /// </summary>
        /// <param name="userId">The ID of the member</param>
        /// <returns>ApiResponse containing member activity data</returns>
        Task<ApiResponse<MemberActivityDto>> GetMemberActivityAsync(Guid userId);
    }
}