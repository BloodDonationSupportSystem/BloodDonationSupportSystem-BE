using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BloodDonationSupportSystem.Extensions;
using System.Security.Claims;

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

        /// <summary>
        /// Get dashboard data for Member users
        /// </summary>
        [HttpGet("member")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<MemberDashboardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMemberDashboard()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var response = await _dashboardService.GetMemberDashboardAsync(userId);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<MemberDashboardDto>(ex));
            }
        }

        /// <summary>
        /// Get dashboard data for Staff users
        /// </summary>
        [HttpGet("staff")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(ApiResponse<StaffDashboardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetStaffDashboard()
        {
            try
            {
                var response = await _dashboardService.GetStaffDashboardAsync();
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<StaffDashboardDto>(ex));
            }
        }

        /// <summary>
        /// Get dashboard data for Admin users
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<AdminDashboardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var response = await _dashboardService.GetAdminDashboardAsync();
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<AdminDashboardDto>(ex));
            }
        }

        /// <summary>
        /// Get emergency dashboard data for Staff and Admin users
        /// </summary>
        [HttpGet("emergency")]
        [Authorize(Roles = "Staff,Admin")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyDashboardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetEmergencyDashboard()
        {
            try
            {
                var response = await _dashboardService.GetEmergencyDashboardAsync();
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<EmergencyDashboardDto>(ex));
            }
        }

        /// <summary>
        /// Get member activity data for a specific user (Member can only access their own, Staff and Admin can access any)
        /// </summary>
        [HttpGet("member-activity/{userId?}")]
        [ProducesResponseType(typeof(ApiResponse<MemberActivityDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMemberActivity(Guid? userId = null)
        {
            try
            {
                var currentUserId = HttpContext.GetUserId();
                var isStaffOrAdmin = HttpContext.IsInRole("Staff") || HttpContext.IsInRole("Admin");

                // If no userId is provided, use the current user's ID
                var targetUserId = userId ?? currentUserId;

                // Check permissions: Members can only access their own data, Staff and Admin can access any
                if (targetUserId != currentUserId && !isStaffOrAdmin)
                {
                    return Forbid("You can only access your own activity data");
                }

                var response = await _dashboardService.GetMemberActivityAsync(targetUserId);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<MemberActivityDto>(ex));
            }
        }

        #region Statistics Endpoints

        /// <summary>
        /// Get donation statistics for a specific time period (Staff and Admin only)
        /// </summary>
        [HttpPost("statistics/donations")]
        [Authorize(Roles = "Staff,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DonationStatisticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationStatistics([FromBody] StatisticsTimeframeDto timeframe)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return HandleResponse(HandleValidationErrors<DonationStatisticsDto>(ModelState));
                }

                var response = await _dashboardService.GetDonationStatisticsAsync(timeframe);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<DonationStatisticsDto>(ex));
            }
        }

        /// <summary>
        /// Get blood request statistics for a specific time period (Staff and Admin only)
        /// </summary>
        [HttpPost("statistics/requests")]
        [Authorize(Roles = "Staff,Admin")]
        [ProducesResponseType(typeof(ApiResponse<RequestStatisticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestStatistics([FromBody] StatisticsTimeframeDto timeframe)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return HandleResponse(HandleValidationErrors<RequestStatisticsDto>(ModelState));
                }

                var response = await _dashboardService.GetRequestStatisticsAsync(timeframe);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<RequestStatisticsDto>(ex));
            }
        }

        /// <summary>
        /// Get inventory statistics (Staff and Admin only)
        /// </summary>
        [HttpGet("statistics/inventory")]
        [Authorize(Roles = "Staff,Admin")]
        [ProducesResponseType(typeof(ApiResponse<InventoryStatisticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetInventoryStatistics()
        {
            try
            {
                var response = await _dashboardService.GetInventoryStatisticsAsync();
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<InventoryStatisticsDto>(ex));
            }
        }

        /// <summary>
        /// Get donor statistics (Staff and Admin only)
        /// </summary>
        [HttpGet("statistics/donors")]
        [Authorize(Roles = "Staff,Admin")]
        [ProducesResponseType(typeof(ApiResponse<DonorStatisticsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorStatistics()
        {
            try
            {
                var response = await _dashboardService.GetDonorStatisticsAsync();
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<DonorStatisticsDto>(ex));
            }
        }

        #endregion

        #region Quick Statistics Endpoints for Public Access

        /// <summary>
        /// Get basic system statistics for public view (anonymous access allowed)
        /// </summary>
        [HttpGet("public/quick-stats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPublicQuickStats()
        {
            try
            {
                // Get basic statistics that can be shown publicly
                var donorStats = await _dashboardService.GetDonorStatisticsAsync();
                var inventoryStats = await _dashboardService.GetInventoryStatisticsAsync();

                var publicStats = new
                {
                    TotalDonors = donorStats.Data?.TotalDonors ?? 0,
                    ActiveDonors = donorStats.Data?.ActiveDonors ?? 0,
                    AvailableBloodUnits = inventoryStats.Data?.CurrentInventory?.Sum(i => i.AvailableQuantity) ?? 0,
                    //CriticalBloodTypes = inventoryStats.Data?.CriticalItems?.Count ?? 0,
                    LastUpdated = DateTimeOffset.UtcNow
                };

                return HandleResponse(new ApiResponse<object>(publicStats));
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<object>(ex));
            }
        }

        #endregion

        #region Convenience Endpoints

        /// <summary>
        /// Get dashboard based on current user's role
        /// </summary>
        [HttpGet("my-dashboard")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMyDashboard()
        {
            try
            {
                var userRole = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                return userRole switch
                {
                    "Member" => await GetMemberDashboard(),
                    "Staff" => await GetStaffDashboard(),
                    "Admin" => await GetAdminDashboard(),
                    _ => HandleResponse(new ApiResponse(System.Net.HttpStatusCode.BadRequest, "Invalid user role"))
                };
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<object>(ex));
            }
        }

        /// <summary>
        /// Get current user's activity data
        /// </summary>
        [HttpGet("my-activity")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<MemberActivityDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMyActivity()
        {
            try
            {
                var userId = HttpContext.GetUserId();
                var response = await _dashboardService.GetMemberActivityAsync(userId);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleResponse(HandleException<MemberActivityDto>(ex));
            }
        }

        #endregion
    }
}