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
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ApiResponse<MemberDashboardDto>> GetMemberDashboardAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting member dashboard for user {UserId}", userId);

                // Get user information
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<MemberDashboardDto>(HttpStatusCode.NotFound, "User not found");
                }

                var donorProfile = await _unitOfWork.DonorProfiles.GetByUserIdAsync(userId);

                var dashboard = new MemberDashboardDto
                {
                    User = _mapper.Map<UserDto>(user),
                    DonorProfile = donorProfile != null ? _mapper.Map<DonorProfileDto>(donorProfile) : null,
                };

                if (donorProfile != null)
                {
                    // Check eligibility
                    var eligibilityCheck = await CheckDonorEligibilityAsync(donorProfile.Id);
                    dashboard.IsEligibleToDonate = eligibilityCheck.IsEligible;
                    dashboard.IneligibilityReason = eligibilityCheck.Reason ?? "";
                    dashboard.NextEligibleDonationDate = eligibilityCheck.NextEligibleDate;

                    // Get recent donations
                    var recentDonations = await _unitOfWork.DonationEvents.FindAsync(de => 
                        de.DonorId == donorProfile.Id && 
                        de.DonationDate >= DateTimeOffset.UtcNow.AddDays(-365));
                    dashboard.RecentDonations = _mapper.Map<List<DonationEventDto>>(recentDonations.Take(10));

                    // Calculate statistics
                    var allDonations = await _unitOfWork.DonationEvents.FindAsync(de => de.DonorId == donorProfile.Id);
                    dashboard.TotalDonationsCount = allDonations.Count();
                    dashboard.TotalDonationVolume = (int)allDonations.Sum(d => d.QuantityDonated ?? 0);
                    dashboard.EstimatedLivesSaved = dashboard.TotalDonationsCount * 3; // Estimate: 1 donation saves ~3 lives

                    // Get upcoming appointments
                    var upcomingAppointments = await _unitOfWork.DonationAppointmentRequests.FindAsync(dar => 
                        dar.DonorId == donorProfile.Id && 
                        dar.PreferredDate >= DateTimeOffset.UtcNow &&
                        dar.Status == "Approved");
                    dashboard.UpcomingAppointments = _mapper.Map<List<DonationAppointmentRequestDto>>(upcomingAppointments);

                    // Get achievement badges
                    dashboard.AchievementBadges = CalculateAchievementBadges(dashboard.TotalDonationsCount, dashboard.TotalDonationVolume);

                    // Get nearby donation centers
                    var nearbyLocations = await _unitOfWork.Locations.GetAllAsync();
                    dashboard.NearbyDonationCenters = _mapper.Map<List<LocationDto>>(nearbyLocations.Take(5));

                    // Get nearby emergency requests
                    var nearbyEmergencies = await _unitOfWork.BloodRequests.FindAsync(br => 
                        br.IsEmergency && 
                        br.IsActive && 
                        br.Status == "Pending");
                    dashboard.NearbyEmergencyRequests = _mapper.Map<List<EmergencyBloodRequestDto>>(nearbyEmergencies.Take(5));
                }

                // Get user's blood requests
                var userBloodRequests = await _unitOfWork.BloodRequests.FindAsync(br => br.RequestedBy == userId);
                dashboard.MyBloodRequests = _mapper.Map<List<BloodRequestDto>>(userBloodRequests);

                // Get recent notifications
                var recentNotifications = await _unitOfWork.Notifications.FindAsync(n => 
                    n.UserId == userId && 
                    n.CreatedTime >= DateTimeOffset.UtcNow.AddDays(-30));
                dashboard.RecentNotifications = _mapper.Map<List<NotificationDto>>(recentNotifications.Take(10));

                return new ApiResponse<MemberDashboardDto>(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member dashboard for user {UserId}", userId);
                return new ApiResponse<MemberDashboardDto>(HttpStatusCode.InternalServerError, "Error retrieving member dashboard");
            }
        }

        public async Task<ApiResponse<StaffDashboardDto>> GetStaffDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Getting staff dashboard");

                var dashboard = new StaffDashboardDto();

                // Get active blood requests using the parameterized method
                var activeRequestsParams = new BloodRequestParameters
                {
                    Status = "Pending",
                    PageNumber = 1,
                    PageSize = 100 // Get a large number for dashboard
                };
                var activeRequestsResult = await _unitOfWork.BloodRequests.GetPagedBloodRequestsAsync(activeRequestsParams);
                var activeRequests = activeRequestsResult.items.Where(br => br.IsActive);
                dashboard.ActiveBloodRequests = _mapper.Map<List<BloodRequestDto>>(activeRequests);

                // Get emergency requests
                var emergencyRequests = activeRequests.Where(br => br.IsEmergency);
                dashboard.EmergencyRequests = _mapper.Map<List<EmergencyBloodRequestDto>>(emergencyRequests);

                // Get request status counts
                dashboard.RequestStatusCounts = await GetRequestStatusCountsAsync();

                // Get inventory summary with proper mapping and calculation
                dashboard.InventorySummary = await GetInventorySummaryWithNamesAsync();

                // Get critical inventory levels with proper data
                //dashboard.CriticalInventoryLevels = await GetCriticalInventoryAsync();

                // Get recent and upcoming appointments using the parameterized method
                var recentAppointmentParams = new AppointmentRequestParameters
                {
                    StartDate = DateTimeOffset.UtcNow.AddDays(-7),
                    EndDate = DateTimeOffset.UtcNow,
                    PageNumber = 1,
                    PageSize = 20,
                    SortBy = "createdtime",
                    SortAscending = false
                };
                var recentAppointmentsResult = await _unitOfWork.DonationAppointmentRequests.GetPagedAppointmentRequestsAsync(recentAppointmentParams);
                dashboard.RecentAppointments = _mapper.Map<List<DonationAppointmentRequestDto>>(recentAppointmentsResult.items);

                var upcomingAppointments = await _unitOfWork.DonationAppointmentRequests.GetRequestsByStatusAsync("Approved");
                var futureAppointments = upcomingAppointments.Where(dar => dar.PreferredDate >= DateTimeOffset.UtcNow);
                dashboard.UpcomingAppointments = _mapper.Map<List<DonationAppointmentRequestDto>>(futureAppointments.Take(20));

                // Get recent donations using the parameterized method
                var recentDonationParams = new DonationEventParameters
                {
                    StartDate = DateTimeOffset.UtcNow.AddDays(-30),
                    EndDate = DateTimeOffset.UtcNow,
                    PageNumber = 1,
                    PageSize = 20,
                    SortBy = "donationdate",
                    SortAscending = false
                };
                var recentDonationsResult = await _unitOfWork.DonationEvents.GetPagedDonationEventsAsync(recentDonationParams);
                dashboard.RecentDonations = _mapper.Map<List<DonationEventDto>>(recentDonationsResult.items);

                // Get expiring inventory using the parameterized method
                var expiringInventoryParams = new BloodInventoryParameters
                {
                    ExpirationStartDate = DateTimeOffset.UtcNow,
                    ExpirationEndDate = DateTimeOffset.UtcNow.AddDays(30),
                    PageNumber = 1,
                    PageSize = 100
                };
                var expiringInventoryResult = await _unitOfWork.BloodInventories.GetPagedBloodInventoriesAsync(expiringInventoryParams);
                var expiringInventory = expiringInventoryResult.items.Where(bi => bi.QuantityUnits > 0);
                dashboard.ExpiringInventory = _mapper.Map<List<BloodInventoryDto>>(expiringInventory);

                // Get today's activity
                dashboard.TodayActivity = await GetTodayActivityAsync();

                // Get available emergency donors using the repository method
                var emergencyDonors = await _unitOfWork.DonorProfiles.GetAvailableDonorsAsync(forEmergency: true);
                dashboard.AvailableEmergencyDonors = _mapper.Map<List<DonorProfileDto>>(emergencyDonors);

                // Get trends with proper implementation
                dashboard.Trends = await GetTrendsDataWithCalculationsAsync();

                return new ApiResponse<StaffDashboardDto>(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff dashboard");
                return new ApiResponse<StaffDashboardDto>(HttpStatusCode.InternalServerError, "Error retrieving staff dashboard");
            }
        }

        private async Task<List<BloodInventorySummaryDto>> GetInventorySummaryWithNamesAsync()
        {
            var inventoryItems = await _unitOfWork.BloodInventories.GetAllAsync();
            var bloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
            var componentTypes = await _unitOfWork.ComponentTypes.GetAllAsync();

            var summary = inventoryItems
                .GroupBy(i => new { i.BloodGroupId, i.ComponentTypeId })
                .Select(g => {
                    var bloodGroup = bloodGroups.FirstOrDefault(bg => bg.Id == g.Key.BloodGroupId);
                    var componentType = componentTypes.FirstOrDefault(ct => ct.Id == g.Key.ComponentTypeId);
                    var availableQuantity = g.Where(i => i.ExpirationDate > DateTimeOffset.UtcNow).Sum(i => i.QuantityUnits);
                    var optimalQuantity = 50; // Default optimal
                    var availabilityPercentage = optimalQuantity > 0 ? (double)availableQuantity / optimalQuantity * 100 : 0;

                    return new BloodInventorySummaryDto
                    {
                        BloodGroupId = g.Key.BloodGroupId,
                        BloodGroupName = bloodGroup?.GroupName ?? "Unknown",
                        ComponentTypeId = g.Key.ComponentTypeId,
                        ComponentTypeName = componentType?.Name ?? "Unknown",
                        AvailableQuantity = availableQuantity,
                        MinimumRecommended = 10, // Default threshold
                        OptimalQuantity = optimalQuantity,
                        AvailabilityPercentage = Math.Round(availabilityPercentage, 2),
                        Status = availableQuantity <= 10 ? "Critical" : "Normal"
                    };
                })
                .ToList();

            return summary;
        }

        private async Task<TrendsDto> GetTrendsDataWithCalculationsAsync()
        {
            var trends = new TrendsDto();
            var now = DateTimeOffset.UtcNow;

            // Calculate donation trends for the last 7 days
            for (int i = 6; i >= 0; i--)
            {
                var date = now.AddDays(-i).Date;
                var nextDate = date.AddDays(1);

                var donationsOnDate = await _unitOfWork.DonationEvents.FindAsync(de =>
                    de.DonationDate >= date && de.DonationDate < nextDate);

                var requestsOnDate = await _unitOfWork.BloodRequests.FindAsync(br =>
                    br.RequestDate >= date && br.RequestDate < nextDate);

                var emergencyRequestsOnDate = requestsOnDate.Where(br => br.IsEmergency);

                trends.DonationTrend[date.ToString("yyyy-MM-dd")] = donationsOnDate.Count();
                trends.RequestTrend[date.ToString("yyyy-MM-dd")] = requestsOnDate.Count();
                trends.EmergencyTrend[date.ToString("yyyy-MM-dd")] = emergencyRequestsOnDate.Count();
            }

            // Calculate inventory trends for the last 7 days
            var bloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
            var componentTypes = await _unitOfWork.ComponentTypes.GetAllAsync();

            foreach (var bloodGroup in bloodGroups)
            {
                var bloodGroupTrend = new Dictionary<string, int>();

                for (int i = 6; i >= 0; i--)
                {
                    var date = now.AddDays(-i).Date;
                    var nextDate = date.AddDays(1);

                    var inventoryOnDate = await _unitOfWork.BloodInventories.FindAsync(bi =>
                        bi.BloodGroupId == bloodGroup.Id &&
                        bi.ExpirationDate > date);

                    var totalQuantity = inventoryOnDate.Sum(bi => bi.QuantityUnits);
                    bloodGroupTrend[date.ToString("yyyy-MM-dd")] = totalQuantity;
                }

                trends.InventoryTrend[bloodGroup.GroupName] = bloodGroupTrend;
            }

            return trends;
        }

        public async Task<ApiResponse<AdminDashboardDto>> GetAdminDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Getting admin dashboard");

                var dashboard = new AdminDashboardDto
                {
                    SystemStats = await GetSystemStatisticsAsync(),
                    UserStats = await GetUserStatisticsAsync(),
                    InventoryStats = (await GetInventoryStatisticsAsync()).Data,
                    DonationStats = (await GetDonationStatisticsAsync(new StatisticsTimeframeDto 
                    { 
                        StartDate = DateTimeOffset.UtcNow.AddDays(-30), 
                        EndDate = DateTimeOffset.UtcNow 
                    })).Data,
                    RequestStats = (await GetRequestStatisticsAsync(new StatisticsTimeframeDto 
                    { 
                        StartDate = DateTimeOffset.UtcNow.AddDays(-30), 
                        EndDate = DateTimeOffset.UtcNow 
                    })).Data,
                    PerformanceMetrics = await GetPerformanceMetricsAsync(),
                    GeographicDistribution = await GetGeographicDistributionAsync()
                };

                // Get recent activities (simplified - you might have a dedicated activity log table)
                dashboard.RecentActivities = new List<ActivityLogDto>();

                // Get active emergency requests
                var emergencyRequests = await _unitOfWork.BloodRequests.FindAsync(br => br.IsEmergency && br.IsActive && br.Status == "Pending");
                dashboard.ActiveEmergencyRequests = _mapper.Map<List<EmergencyBloodRequestDto>>(emergencyRequests);

                return new ApiResponse<AdminDashboardDto>(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard");
                return new ApiResponse<AdminDashboardDto>(HttpStatusCode.InternalServerError, "Error retrieving admin dashboard");
            }
        }

        public async Task<ApiResponse<EmergencyDashboardDto>> GetEmergencyDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Getting emergency dashboard");

                var dashboard = new EmergencyDashboardDto
                {
                    LastUpdated = DateTimeOffset.UtcNow
                };

                // Get active emergency requests
                var emergencyRequests = await _unitOfWork.BloodRequests.FindAsync(br => br.IsEmergency && br.IsActive && br.Status == "Pending");
                dashboard.ActiveEmergencyRequests = _mapper.Map<List<EmergencyBloodRequestDto>>(emergencyRequests);

                // Get available emergency donors
                var emergencyDonors = await _unitOfWork.DonorProfiles.FindAsync(dp => dp.IsAvailableForEmergency);
                dashboard.AvailableEmergencyDonors = _mapper.Map<List<DonorProfileDto>>(emergencyDonors);

                // Get emergency inventory levels
                dashboard.EmergencyInventoryLevels = await GetInventorySummaryAsync();

                return new ApiResponse<EmergencyDashboardDto>(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emergency dashboard");
                return new ApiResponse<EmergencyDashboardDto>(HttpStatusCode.InternalServerError, "Error retrieving emergency dashboard");
            }
        }

        public async Task<ApiResponse<DonationStatisticsDto>> GetDonationStatisticsAsync(StatisticsTimeframeDto timeframe)
        {
            try
            {
                _logger.LogInformation("Getting donation statistics for period {StartDate} to {EndDate}", 
                    timeframe.StartDate, timeframe.EndDate);

                var stats = await CalculateDonationStatisticsAsync(timeframe);
                return new ApiResponse<DonationStatisticsDto>(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donation statistics");
                return new ApiResponse<DonationStatisticsDto>(HttpStatusCode.InternalServerError, "Error retrieving donation statistics");
            }
        }

        public async Task<ApiResponse<RequestStatisticsDto>> GetRequestStatisticsAsync(StatisticsTimeframeDto timeframe)
        {
            try
            {
                _logger.LogInformation("Getting request statistics for period {StartDate} to {EndDate}", 
                    timeframe.StartDate, timeframe.EndDate);

                var stats = await CalculateRequestStatisticsAsync(timeframe);
                return new ApiResponse<RequestStatisticsDto>(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting request statistics");
                return new ApiResponse<RequestStatisticsDto>(HttpStatusCode.InternalServerError, "Error retrieving request statistics");
            }
        }

        public async Task<ApiResponse<InventoryStatisticsDto>> GetInventoryStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting inventory statistics");

                var stats = new InventoryStatisticsDto
                {
                    CurrentInventory = await GetInventorySummaryAsync(),
                    CriticalItems = await GetCriticalInventoryAsync(),
                    ExpiringItems7Days = await CountExpiringItemsAsync(7),
                    ExpiringItems30Days = await CountExpiringItemsAsync(30),
                    WasteRate = await CalculateWasteRateAsync(),
                    TurnoverRate = await CalculateTurnoverRateAsync(),
                    AverageShelfLife = await CalculateAverageShelfLifeAsync()
                };

                return new ApiResponse<InventoryStatisticsDto>(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory statistics");
                return new ApiResponse<InventoryStatisticsDto>(HttpStatusCode.InternalServerError, "Error retrieving inventory statistics");
            }
        }

        public async Task<ApiResponse<DonorStatisticsDto>> GetDonorStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting donor statistics");

                var stats = await CalculateDonorStatisticsAsync();
                return new ApiResponse<DonorStatisticsDto>(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donor statistics");
                return new ApiResponse<DonorStatisticsDto>(HttpStatusCode.InternalServerError, "Error retrieving donor statistics");
            }
        }

        public async Task<ApiResponse<MemberActivityDto>> GetMemberActivityAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting member activity for user {UserId}", userId);

                var activity = new MemberActivityDto
                {
                    UserId = userId
                };

                // Get donor profile
                var donorProfiles = await _unitOfWork.DonorProfiles.FindAsync(dp => dp.UserId == userId);
                var donorProfile = donorProfiles.FirstOrDefault();
                
                if (donorProfile != null)
                {
                    // Get recent donations
                    var recentDonations = await _unitOfWork.DonationEvents.FindAsync(de => 
                        de.DonorId == donorProfile.Id && 
                        de.DonationDate >= DateTimeOffset.UtcNow.AddDays(-90));
                    activity.RecentDonations = _mapper.Map<List<DonationEventDto>>(recentDonations.Take(10));

                    // Check eligibility
                    var eligibilityCheck = await CheckDonorEligibilityAsync(donorProfile.Id);
                    activity.IsEligibleToDonate = eligibilityCheck.IsEligible;
                    activity.NextEligibleDonationDate = eligibilityCheck.NextEligibleDate;

                    // Get upcoming appointments
                    var upcomingAppointments = await _unitOfWork.DonationAppointmentRequests.FindAsync(dar => 
                        dar.DonorId == donorProfile.Id && 
                        dar.PreferredDate >= DateTimeOffset.UtcNow &&
                        dar.Status == "Approved");
                    activity.UpcomingAppointments = _mapper.Map<List<DonationAppointmentRequestDto>>(upcomingAppointments);
                }
                else
                {
                    activity.UpcomingAppointments = new List<DonationAppointmentRequestDto>();
                }

                // Get blood requests
                var bloodRequests = await _unitOfWork.BloodRequests.FindAsync(br => br.RequestedBy == userId);
                activity.BloodRequests = _mapper.Map<List<BloodRequestDto>>(bloodRequests);

                // Get recent notifications
                var recentNotifications = await _unitOfWork.Notifications.FindAsync(n => 
                    n.UserId == userId && 
                    n.CreatedTime >= DateTimeOffset.UtcNow.AddDays(-30));
                activity.RecentNotifications = _mapper.Map<List<NotificationDto>>(recentNotifications.Take(10));

                return new ApiResponse<MemberActivityDto>(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member activity for user {UserId}", userId);
                return new ApiResponse<MemberActivityDto>(HttpStatusCode.InternalServerError, "Error retrieving member activity");
            }
        }

        #region Helper Methods

        private async Task<(bool IsEligible, string? Reason, DateTimeOffset? NextEligibleDate)> CheckDonorEligibilityAsync(Guid donorProfileId)
        {
            // Get the latest donation
            var donations = await _unitOfWork.DonationEvents.FindAsync(de => de.DonorId == donorProfileId);
            var latestDonation = donations.OrderByDescending(d => d.DonationDate).FirstOrDefault();
            
            if (latestDonation == null)
            {
                return (true, null, null);
            }

            // Check if enough time has passed (typically 56 days for whole blood)
            var daysSinceLastDonation = latestDonation.DonationDate.HasValue 
                ? (DateTimeOffset.UtcNow - latestDonation.DonationDate.Value).Days 
                : 0;
            const int minimumDaysBetweenDonations = 56;

            if (daysSinceLastDonation < minimumDaysBetweenDonations)
            {
                var nextEligibleDate = latestDonation.DonationDate.Value.AddDays(minimumDaysBetweenDonations);
                return (false, $"Must wait {minimumDaysBetweenDonations - daysSinceLastDonation} more days", nextEligibleDate);
            }

            return (true, null, null);
        }

        private List<string> CalculateAchievementBadges(int totalDonations, int totalVolume)
        {
            var badges = new List<string>();

            if (totalDonations >= 1) badges.Add("First Time Donor");
            if (totalDonations >= 5) badges.Add("Regular Donor");
            if (totalDonations >= 10) badges.Add("Dedicated Donor");
            if (totalDonations >= 25) badges.Add("Champion Donor");
            if (totalDonations >= 50) badges.Add("Hero Donor");
            if (totalDonations >= 100) badges.Add("Legendary Donor");

            if (totalVolume >= 1000) badges.Add("Liter Club");
            if (totalVolume >= 5000) badges.Add("Five Liter Hero");

            return badges;
        }

        private async Task<Dictionary<string, int>> GetRequestStatusCountsAsync()
        {
            var allRequests = await _unitOfWork.BloodRequests.GetAllAsync();
            return allRequests
                .GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<List<BloodInventorySummaryDto>> GetInventorySummaryAsync()
        {
            var inventoryItems = await _unitOfWork.BloodInventories.GetAllAsync();
            
            var summary = inventoryItems
                .GroupBy(i => new { i.BloodGroupId, i.ComponentTypeId })
                .Select(g => new BloodInventorySummaryDto
                {
                    BloodGroupId = g.Key.BloodGroupId,
                    ComponentTypeId = g.Key.ComponentTypeId,
                    AvailableQuantity = g.Where(i => i.ExpirationDate > DateTimeOffset.UtcNow).Sum(i => i.QuantityUnits),
                    MinimumRecommended = 10, // Default threshold
                    OptimalQuantity = 50, // Default optimal
                    Status = g.Sum(i => i.QuantityUnits) <= 10 ? "Critical" : "Normal"
                })
                .ToList();

            return summary;
        }

        private async Task<List<CriticalInventoryDto>> GetCriticalInventoryAsync()
        {
            var inventoryItems = await _unitOfWork.BloodInventories.GetAllAsync();
            
            var criticalItems = inventoryItems
                .GroupBy(i => new { i.BloodGroupId, i.ComponentTypeId })
                .Where(g => g.Sum(i => i.QuantityUnits) <= 10) // Critical threshold
                .Select(g => new CriticalInventoryDto
                {
                    BloodGroupId = g.Key.BloodGroupId,
                    ComponentTypeId = g.Key.ComponentTypeId,
                    AvailableQuantity = g.Sum(i => i.QuantityUnits),
                    MinimumRecommended = 10,
                    CriticalityLevel = "High",
                    EstimatedDaysUntilDepletion = 5
                })
                .ToList();

            return criticalItems;
        }

        private async Task<DailyActivityDto> GetTodayActivityAsync()
        {
            var today = DateTimeOffset.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayDonations = await _unitOfWork.DonationEvents.FindAsync(de => 
                de.DonationDate >= today && de.DonationDate < tomorrow);
            var todayRequests = await _unitOfWork.BloodRequests.FindAsync(br => 
                br.RequestDate >= today && br.RequestDate < tomorrow);
            var todayDonors = await _unitOfWork.DonorProfiles.FindAsync(dp => 
                dp.CreatedTime >= today && dp.CreatedTime < tomorrow);

            return new DailyActivityDto
            {
                DonationsToday = todayDonations.Count(),
                NewRequestsToday = todayRequests.Count(),
                FulfilledRequestsToday = todayRequests.Count(r => r.Status == "Fulfilled"),
                NewDonorsToday = todayDonors.Count(),
                TotalVolumeCollectedToday = (int)todayDonations.Sum(d => d.QuantityDonated ?? 0),
                EmergencyRequestsToday = todayRequests.Count(r => r.IsEmergency)
            };
        }

        private async Task<TrendsDto> GetTrendsDataAsync()
        {
            var trends = new TrendsDto();
            // This would need actual implementation based on your data structure
            return trends;
        }

        private async Task<SystemStatisticsDto> GetSystemStatisticsAsync()
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var allDonors = await _unitOfWork.DonorProfiles.GetAllAsync();
            var allRequests = await _unitOfWork.BloodRequests.GetAllAsync();
            var allDonations = await _unitOfWork.DonationEvents.GetAllAsync();

            return new SystemStatisticsDto
            {
                TotalUsers = allUsers.Count(),
                TotalDonors = allDonors.Count(),
                TotalBloodRequests = allRequests.Count(),
                TotalDonations = allDonations.Count(),
                TotalVolumeCollected = (int)allDonations.Sum(d => d.QuantityDonated ?? 0),
                TotalLivesSaved = allDonations.Count() * 3, // Estimate
                RequestFulfillmentRate = await CalculateRequestFulfillmentRateAsync(),
                AverageResponseTime = await CalculateAverageResponseTimeAsync()
            };
        }

        private async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var last30Days = DateTimeOffset.UtcNow.AddDays(-30);

            var userStats = new UserStatisticsDto
            {
                NewUsersLast30Days = allUsers.Count(u => u.CreatedTime >= last30Days),
                ActiveUsersLast30Days = allUsers.Count(u => u.CreatedTime >= last30Days) // Simplified
            };

            // Get users by role (simplified)
            userStats.UsersByRole = new Dictionary<string, int>
            {
                { "Member", allUsers.Count(u => u.RoleId == Guid.Parse("11111111-1111-1111-1111-111111111111")) }, // Placeholder
                { "Staff", allUsers.Count(u => u.RoleId == Guid.Parse("22222222-2222-2222-2222-222222222222")) }, // Placeholder
                { "Admin", allUsers.Count(u => u.RoleId == Guid.Parse("33333333-3333-3333-3333-333333333333")) } // Placeholder
            };

            return userStats;
        }

        private async Task<DonationStatisticsDto> CalculateDonationStatisticsAsync(StatisticsTimeframeDto timeframe)
        {
            var donations = await _unitOfWork.DonationEvents.FindAsync(de => 
                de.DonationDate >= timeframe.StartDate && 
                de.DonationDate <= timeframe.EndDate);
            
            // Get first time donors in period
            var firstTimeDonors = new List<Guid>();
            foreach (var donation in donations)
            {
                if (donation.DonorId.HasValue)
                {
                    var previousDonations = await _unitOfWork.DonationEvents.FindAsync(de => 
                        de.DonorId == donation.DonorId && 
                        de.DonationDate < timeframe.StartDate);
                    
                    if (!previousDonations.Any())
                    {
                        firstTimeDonors.Add(donation.DonorId.Value);
                    }
                }
            }

            return new DonationStatisticsDto
            {
                TimePeriod = $"{timeframe.StartDate:yyyy-MM-dd} to {timeframe.EndDate:yyyy-MM-dd}",
                StartDate = timeframe.StartDate,
                EndDate = timeframe.EndDate,
                TotalDonations = donations.Count(),
                TotalVolumeCollected = (int)donations.Sum(d => d.QuantityDonated ?? 0),
                FirstTimeDonors = firstTimeDonors.Count,
                RepeatDonors = donations.Count() - firstTimeDonors.Count
            };
        }

        private async Task<RequestStatisticsDto> CalculateRequestStatisticsAsync(StatisticsTimeframeDto timeframe)
        {
            var requests = await _unitOfWork.BloodRequests.FindAsync(br => 
                br.RequestDate >= timeframe.StartDate && 
                br.RequestDate <= timeframe.EndDate);
            
            return new RequestStatisticsDto
            {
                TimePeriod = $"{timeframe.StartDate:yyyy-MM-dd} to {timeframe.EndDate:yyyy-MM-dd}",
                StartDate = timeframe.StartDate,
                EndDate = timeframe.EndDate,
                TotalRequests = requests.Count(),
                EmergencyRequests = requests.Count(r => r.IsEmergency),
                FulfilledRequests = requests.Count(r => r.Status == "Fulfilled"),
                PendingRequests = requests.Count(r => r.Status == "Pending"),
                CanceledRequests = requests.Count(r => r.Status == "Canceled"),
                FulfillmentRate = requests.Any() ? (double)requests.Count(r => r.Status == "Fulfilled") / requests.Count() * 100 : 0
            };
        }

        private async Task<DonorStatisticsDto> CalculateDonorStatisticsAsync()
        {
            var allDonors = await _unitOfWork.DonorProfiles.GetAllAsync();
            var last30Days = DateTimeOffset.UtcNow.AddDays(-30);
            var last180Days = DateTimeOffset.UtcNow.AddDays(-180);

            var activeDonors = new List<Guid>();
            foreach (var donor in allDonors)
            {
                var recentDonations = await _unitOfWork.DonationEvents.FindAsync(de => 
                    de.DonorId == donor.Id && 
                    de.DonationDate >= last180Days);
                
                if (recentDonations.Any())
                {
                    activeDonors.Add(donor.Id);
                }
            }

            var allDonations = await _unitOfWork.DonationEvents.GetAllAsync();
            var averageDonationsPerDonor = allDonors.Any() ? (double)allDonations.Count() / allDonors.Count() : 0;

            return new DonorStatisticsDto
            {
                TotalDonors = allDonors.Count(),
                ActiveDonors = activeDonors.Count,
                NewDonorsLast30Days = allDonors.Count(d => d.CreatedTime >= last30Days),
                EmergencyAvailableDonors = allDonors.Count(d => d.IsAvailableForEmergency),
                AverageDonationsPerDonor = averageDonationsPerDonor,
                DonorRetentionRate = await CalculateDonorRetentionRateAsync()
            };
        }

        private async Task<PerformanceMetricsDto> GetPerformanceMetricsAsync()
        {
            return new PerformanceMetricsDto
            {
                AverageRequestResponseTime = await CalculateAverageResponseTimeAsync(),
                AverageEmergencyResponseTime = await CalculateAverageEmergencyResponseTimeAsync(),
                DonorRecruitmentRate = await CalculateDonorRecruitmentRateAsync(),
                DonorRetentionRate = await CalculateDonorRetentionRateAsync(),
                InventoryTurnoverRate = await CalculateTurnoverRateAsync(),
                WasteReductionRate = 100 - await CalculateWasteRateAsync(),
                AverageAppointmentWaitTime = await CalculateAverageAppointmentWaitTimeAsync()
            };
        }

        private async Task<GeographicDistributionDto> GetGeographicDistributionAsync()
        {
            // This would need actual implementation based on your geographic data structure
            return new GeographicDistributionDto();
        }

        private async Task<int> CountExpiringItemsAsync(int days)
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(days);
            var expiringItems = await _unitOfWork.BloodInventories.FindAsync(bi => 
                bi.ExpirationDate <= cutoffDate && 
                bi.QuantityUnits > 0);
            return expiringItems.Count();
        }

        // Placeholder implementations for calculation methods
        private async Task<double> CalculateRequestFulfillmentRateAsync()
        {
            var allRequests = await _unitOfWork.BloodRequests.GetAllAsync();
            var fulfilledRequests = allRequests.Count(r => r.Status == "Fulfilled");
            return allRequests.Any() ? (double)fulfilledRequests / allRequests.Count() * 100 : 0;
        }

        private async Task<double> CalculateAverageResponseTimeAsync()
        {
            // This would calculate the average time from request creation to fulfillment
            return 24.0; // Placeholder: 24 hours
        }

        private async Task<double> CalculateAverageEmergencyResponseTimeAsync()
        {
            // This would calculate the average time for emergency requests
            return 4.0; // Placeholder: 4 hours
        }

        private async Task<double> CalculateWasteRateAsync()
        {
            // Calculate percentage of expired inventory
            return 5.0; // Placeholder: 5%
        }

        private async Task<double> CalculateTurnoverRateAsync()
        {
            // Calculate how quickly inventory is used
            return 12.0; // Placeholder: 12 times per year
        }

        private async Task<double> CalculateAverageShelfLifeAsync()
        {
            // Calculate average shelf life of inventory items
            return 35.0; // Placeholder: 35 days
        }

        private async Task<double> CalculateDonorRetentionRateAsync()
        {
            // Calculate percentage of donors who continue donating
            return 75.0; // Placeholder: 75%
        }

        private async Task<double> CalculateDonorRecruitmentRateAsync()
        {
            // Calculate new donors per month
            return 50.0; // Placeholder: 50 new donors per month
        }

        private async Task<double> CalculateAverageAppointmentWaitTimeAsync()
        {
            // Calculate average wait time for appointments
            return 7.0; // Placeholder: 7 days
        }

        #endregion
    }
}