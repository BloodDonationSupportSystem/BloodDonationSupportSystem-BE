using AutoMapper;
using BusinessObjects.Dtos;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<DashboardOverviewDto>> GetDashboardOverviewAsync()
        {
            try
            {
                var overview = await _unitOfWork.Analytics.GetDashboardOverviewAsync();
                return new ApiResponse<DashboardOverviewDto>(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard overview");
                return new ApiResponse<DashboardOverviewDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting dashboard overview");
            }
        }

        public async Task<ApiResponse<List<BloodInventorySummaryDto>>> GetBloodInventorySummaryAsync()
        {
            try
            {
                var summary = await _unitOfWork.Analytics.GetBloodInventorySummaryAsync();
                return new ApiResponse<List<BloodInventorySummaryDto>>(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting blood inventory summary");
                return new ApiResponse<List<BloodInventorySummaryDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting blood inventory summary");
            }
        }

        public async Task<ApiResponse<List<TopDonorDto>>> GetTopDonorsAsync(int count = 10)
        {
            try
            {
                var topDonors = await _unitOfWork.Analytics.GetTopDonorsAsync(count);
                return new ApiResponse<List<TopDonorDto>>(topDonors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting top donors");
                return new ApiResponse<List<TopDonorDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting top donors");
            }
        }

        public async Task<ApiResponse<DonationStatisticsDto>> GetDonationStatisticsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            try
            {
                var statistics = await _unitOfWork.Analytics.GetDonationStatisticsAsync(startDate, endDate);
                return new ApiResponse<DonationStatisticsDto>(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation statistics");
                return new ApiResponse<DonationStatisticsDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation statistics");
            }
        }

        public async Task<ApiResponse<List<RecentActivityDto>>> GetRecentActivitiesAsync(int count = 10)
        {
            try
            {
                var activities = await _unitOfWork.Analytics.GetRecentActivitiesAsync(count);
                return new ApiResponse<List<RecentActivityDto>>(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recent activities");
                return new ApiResponse<List<RecentActivityDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting recent activities");
            }
        }
    }
}