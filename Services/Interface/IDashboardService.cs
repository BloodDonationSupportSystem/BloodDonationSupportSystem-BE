using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDashboardService
    {
        /// <summary>
        /// L?y thông tin t?ng quan cho dashboard
        /// </summary>
        Task<ApiResponse<DashboardOverviewDto>> GetDashboardOverviewAsync();

        /// <summary>
        /// L?y thông tin t?n kho máu
        /// </summary>
        Task<ApiResponse<List<BloodInventorySummaryDto>>> GetBloodInventorySummaryAsync();

        /// <summary>
        /// L?y danh sách ng??i hi?n máu hàng ??u
        /// </summary>
        /// <param name="count">S? l??ng ng??i hi?n máu mu?n l?y</param>
        Task<ApiResponse<List<TopDonorDto>>> GetTopDonorsAsync(int count = 10);

        /// <summary>
        /// L?y th?ng kê hi?n máu trong kho?ng th?i gian
        /// </summary>
        /// <param name="startDate">Th?i gian b?t ??u</param>
        /// <param name="endDate">Th?i gian k?t thúc</param>
        Task<ApiResponse<DonationStatisticsDto>> GetDonationStatisticsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);

        /// <summary>
        /// L?y các ho?t ??ng g?n ?ây
        /// </summary>
        /// <param name="count">S? l??ng ho?t ??ng mu?n l?y</param>
        Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync(int count = 10);
    }
}