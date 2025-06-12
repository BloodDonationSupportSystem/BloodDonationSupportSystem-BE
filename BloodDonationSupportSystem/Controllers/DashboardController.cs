using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: api/Dashboard/overview
        [HttpGet("overview")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem thông tin t?ng quan
        [ProducesResponseType(typeof(ApiResponse<DashboardOverviewDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var response = await _dashboardService.GetDashboardOverviewAsync();
            return HandleResponse(response);
        }

        // GET: api/Dashboard/inventory-summary
        [HttpGet("inventory-summary")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem thông tin t?n kho
        [ProducesResponseType(typeof(ApiResponse<List<BloodInventorySummaryDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetInventorySummary()
        {
            var response = await _dashboardService.GetBloodInventorySummaryAsync();
            return HandleResponse(response);
        }

        // GET: api/Dashboard/top-donors?count=10
        [HttpGet("top-donors")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem thông tin ng??i hi?n máu hàng ??u
        [ProducesResponseType(typeof(ApiResponse<List<TopDonorDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetTopDonors([FromQuery] int count = 10)
        {
            var response = await _dashboardService.GetTopDonorsAsync(count);
            return HandleResponse(response);
        }

        // GET: api/Dashboard/donation-statistics?startDate=2023-01-01&endDate=2023-12-31
        [HttpGet("donation-statistics")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem th?ng kê hi?n máu
        [ProducesResponseType(typeof(ApiResponse<DonationStatisticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationStatistics(
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null)
        {
            var response = await _dashboardService.GetDonationStatisticsAsync(startDate, endDate);
            return HandleResponse(response);
        }

        // GET: api/Dashboard/recent-activities?count=10
        [HttpGet("recent-activities")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem ho?t ??ng g?n ?ây
        [ProducesResponseType(typeof(ApiResponse<List<RecentActivityDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10)
        {
            var response = await _dashboardService.GetRecentActivitiesAsync(count);
            return HandleResponse(response);
        }
    }
}