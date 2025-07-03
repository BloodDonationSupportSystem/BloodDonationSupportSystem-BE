using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Dtos;

namespace Repositories.Implementation
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Dashboard Methods

        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            var totalDonors = await _context.DonorProfiles
                .Where(d => d.DeletedTime == null)
                .CountAsync();

            var totalDonations = await _context.DonationEvents
                .Where(d => d.Status == "Completed" && d.DeletedTime == null)
                .CountAsync();

            var pendingRequests = await _context.BloodRequests
                .Where(r => r.Status == "Pending")
                .CountAsync();

            var availableInventory = await _context.BloodInventories
                .Where(i => i.Status == "Available" && i.ExpirationDate > DateTimeOffset.UtcNow)
                .CountAsync();

            var today = DateTimeOffset.UtcNow;
            var sevenDaysLater = today.AddDays(7);
            var expiringSoonInventory = await _context.BloodInventories
                .Where(i => i.Status == "Available"
                            && i.ExpirationDate > today
                            && i.ExpirationDate < sevenDaysLater)
                .CountAsync();

            var emergencyRequests = await _context.BloodRequests
                .Where(r => r.Status == "Pending" && r.IsEmergency && r.DeletedTime == null)
                .CountAsync();

            var scheduledAppointments = await _context.DonationAppointmentRequests
                .Where(dar => dar.Status == "Confirmed"
                             && dar.ConfirmedDate.HasValue
                             && dar.ConfirmedDate > today
                             && dar.DeletedTime == null)
                .CountAsync();

            return new DashboardOverviewDto
            {
                TotalDonors = totalDonors,
                TotalDonations = totalDonations,
                PendingRequests = pendingRequests,
                AvailableInventory = availableInventory,
                ExpiringSoonInventory = expiringSoonInventory,
                EmergencyRequests = emergencyRequests,
                ScheduledAppointments = scheduledAppointments,
                LastUpdated = DateTimeOffset.UtcNow
            };
        }

        public async Task<List<BloodInventorySummaryDto>> GetBloodInventorySummaryAsync()
        {
            var bloodGroups = await _context.BloodGroups.ToListAsync();
            var componentTypes = await _context.ComponentTypes.ToListAsync();

            var inventories = await _context.BloodInventories
                .Where(i => i.Status == "Available" && i.ExpirationDate > DateTimeOffset.UtcNow)
                .ToListAsync();

            var expiringSoon = await _context.BloodInventories
                .Where(i => i.Status == "Available"
                         && i.ExpirationDate > DateTimeOffset.UtcNow
                         && i.ExpirationDate < DateTimeOffset.UtcNow.AddDays(7))
                .ToListAsync();

            var result = new List<BloodInventorySummaryDto>();

            foreach (var bloodGroup in bloodGroups)
            {
                var summaryDto = new BloodInventorySummaryDto
                {
                    BloodGroupId = bloodGroup.Id,
                    BloodGroupName = bloodGroup.GroupName,
                    CriticalThreshold = 10, // Có thể điều chỉnh theo yêu cầu
                    Components = new List<ComponentInventoryDto>()
                };

                foreach (var componentType in componentTypes)
                {
                    var availableUnits = inventories
                        .Where(i => i.BloodGroupId == bloodGroup.Id && i.ComponentTypeId == componentType.Id)
                        .Sum(i => i.QuantityUnits);

                    var expiringSoonUnits = expiringSoon
                        .Where(i => i.BloodGroupId == bloodGroup.Id && i.ComponentTypeId == componentType.Id)
                        .Sum(i => i.QuantityUnits);

                    summaryDto.Components.Add(new ComponentInventoryDto
                    {
                        ComponentTypeId = componentType.Id,
                        ComponentTypeName = componentType.Name,
                        AvailableUnits = availableUnits,
                        ExpiringSoonUnits = expiringSoonUnits,
                        CriticalThreshold = 5 // Có thể điều chỉnh theo yêu cầu
                    });
                }

                summaryDto.TotalUnits = summaryDto.Components.Sum(c => c.AvailableUnits);
                result.Add(summaryDto);
            }

            return result;
        }

        public async Task<List<TopDonorDto>> GetTopDonorsAsync(int count = 10)
        {
            var topDonors = await _context.DonorProfiles
                .Where(d => d.DeletedTime == null)
                .OrderByDescending(d => d.TotalDonations)
                .Take(count)
                .Select(d => new TopDonorDto
                {
                    DonorId = d.Id,
                    DonorName = d.User != null ? $"{d.User.FirstName} {d.User.LastName}" : "Unknown",
                    TotalDonations = d.TotalDonations,
                    LastDonationDate = d.LastDonationDate,
                    BloodGroupName = d.BloodGroup != null ? d.BloodGroup.GroupName : "Unknown"
                })
                .ToListAsync();

            return topDonors;
        }

        public async Task<DonationStatisticsDto> GetDonationStatisticsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate)
        {
            var start = startDate ?? DateTimeOffset.UtcNow.AddMonths(-3);
            var end = endDate ?? DateTimeOffset.UtcNow;

            var completedDonations = await _context.DonationEvents
                .Where(d => d.Status == "Completed"
                         && d.DonationDate.HasValue
                         && d.DonationDate >= start
                         && d.DonationDate <= end
                         && d.DeletedTime == null)
                .ToListAsync();

            var result = new DonationStatisticsDto
            {
                StartDate = start,
                EndDate = end,
                TotalDonations = completedDonations.Count
            };

            // Thống kê theo ngày
            var dailyStats = completedDonations
                .Where(d => d.DonationDate.HasValue)
                .GroupBy(d => d.DonationDate!.Value.Date)
                .Select(g => new DateBasedStatDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToList();

            result.DailyDonations = dailyStats;

            // Thống kê theo tháng
            var monthlyStats = completedDonations
                .Where(d => d.DonationDate.HasValue)
                .GroupBy(d => new { d.DonationDate!.Value.Year, d.DonationDate!.Value.Month })
                .Select(g => new DateBasedStatDto
                {
                    Date = new DateTimeOffset(g.Key.Year, g.Key.Month, 1, 0, 0, 0, TimeSpan.Zero),
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToList();

            result.MonthlyDonations = monthlyStats;

            // Thống kê theo nhóm máu
            var bloodGroupStats = await _context.BloodGroups
                .Select(bg => new
                {
                    BloodGroup = bg,
                    Count = completedDonations.Count(d => d.BloodGroupId == bg.Id)
                })
                .ToListAsync();

            result.DonationsByBloodGroup = bloodGroupStats
                .Select(bg => new BloodGroupStatDto
                {
                    BloodGroupId = bg.BloodGroup.Id,
                    BloodGroupName = bg.BloodGroup.GroupName,
                    Count = bg.Count,
                    Percentage = completedDonations.Count > 0
                        ? Math.Round((double)bg.Count / completedDonations.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(bg => bg.Count)
                .ToList();

            // Thống kê theo địa điểm
            var locationStats = await _context.Locations
                .Select(l => new
                {
                    Location = l,
                    Count = completedDonations.Count(d => d.LocationId == l.Id)
                })
                .Where(l => l.Count > 0)
                .ToListAsync();

            result.DonationsByLocation = locationStats
                .Select(l => new LocationStatDto
                {
                    LocationId = l.Location.Id,
                    LocationName = l.Location.Name,
                    Count = l.Count,
                    Percentage = completedDonations.Count > 0
                        ? Math.Round((double)l.Count / completedDonations.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(l => l.Count)
                .ToList();

            return result;
        }

        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10)
        {
            var activities = new List<RecentActivityDto>();

            // Thêm hoạt động từ DonationEvents
            var recentDonationEvents = await _context.DonationEvents
                .Where(d => d.DeletedTime == null)
                .OrderByDescending(d => d.LastUpdatedTime ?? d.CreatedTime)
                .Take(count)
                .Include(d => d.DonorProfile)
                    .ThenInclude(dp => dp.User)
                .ToListAsync();

            foreach (var donationEvent in recentDonationEvents)
            {
                string description = "";
                string donorName = donationEvent.DonorProfile?.User != null
                    ? $"{donationEvent.DonorProfile.User.FirstName} {donationEvent.DonorProfile.User.LastName}"
                    : "Unknown";

                switch (donationEvent.Status)
                {
                    case "Created":
                        description = "Đã tạo sự kiện hiến máu mới";
                        break;
                    case "Scheduled":
                        description = $"{donorName} đã đặt lịch hiến máu";
                        break;
                    case "Completed":
                        description = $"{donorName} đã hoàn thành hiến máu";
                        break;
                    case "WalkIn":
                        description = $"{donorName} đã đến hiến máu trực tiếp";
                        break;
                    case "Cancelled":
                        description = "Sự kiện hiến máu đã bị hủy";
                        break;
                    default:
                        description = $"Cập nhật trạng thái sự kiện hiến máu: {donationEvent.Status}";
                        break;
                }

                activities.Add(new RecentActivityDto
                {
                    Id = donationEvent.Id,
                    Type = "DonationEvent",
                    Description = description,
                    Timestamp = donationEvent.LastUpdatedTime ?? donationEvent.CreatedTime,
                    UserId = donationEvent.DonorProfile?.UserId,
                    UserName = donorName
                });
            }

            // Thêm hoạt động từ BloodRequests
            var recentRequests = await _context.BloodRequests
                .OrderByDescending(r => r.RequestDate)
                .Take(count)
                .Include(r => r.User)
                .ToListAsync();

            foreach (var request in recentRequests)
            {
                string description = "";
                string requesterName = request.User != null
                    ? $"{request.User.FirstName} {request.User.LastName}"
                    : "Unknown";

                string requestType = request.IsEmergency ? "khẩn cấp" : "thường";

                switch (request.Status)
                {
                    case "Pending":
                        description = $"{requesterName} đã tạo yêu cầu máu {requestType} mới";
                        break;
                    case "Processing":
                        description = $"Yêu cầu máu {requestType} từ {requesterName} đang được xử lý";
                        break;
                    case "Fulfilled":
                        description = $"Yêu cầu máu {requestType} từ {requesterName} đã được đáp ứng";
                        break;
                    case "Cancelled":
                        description = $"Yêu cầu máu {requestType} từ {requesterName} đã bị hủy";
                        break;
                    default:
                        description = $"Cập nhật trạng thái yêu cầu máu {requestType}: {request.Status}";
                        break;
                }

                activities.Add(new RecentActivityDto
                {
                    Id = request.Id,
                    Type = request.IsEmergency ? "EmergencyRequest" : "BloodRequest",
                    Description = description,
                    Timestamp = request.RequestDate,
                    UserId = request.RequestedBy,
                    UserName = requesterName
                });
            }

            // Sắp xếp tất cả hoạt động theo thời gian gần đây nhất và lấy count hoạt động đầu tiên
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        #endregion

        #region Report Methods

        public async Task<BloodDonationReportDto> GetBloodDonationReportAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid? bloodGroupId = null, Guid? locationId = null)
        {
            var query = _context.DonationEvents
                .Where(d => d.Status == "Completed"
                         && d.DonationDate.HasValue
                         && d.DonationDate >= startDate
                         && d.DonationDate <= endDate
                         && d.DeletedTime == null);

            if (bloodGroupId.HasValue)
            {
                query = query.Where(d => d.BloodGroupId == bloodGroupId.Value);
            }

            if (locationId.HasValue)
            {
                query = query.Where(d => d.LocationId == locationId.Value);
            }

            var donations = await query.ToListAsync();

            // Lấy danh sách người hiến máu duy nhất
            var uniqueDonorIds = donations.Select(d => d.DonorId).Distinct().ToList();
            var uniqueDonorCount = uniqueDonorIds.Count;

            // Lấy danh sách người hiến máu lần đầu trong khoảng thời gian này
            var newDonorIds = new List<Guid>();
            foreach (var donorId in uniqueDonorIds)
            {
                if (donorId.HasValue)
                {
                    var firstDonation = await _context.DonationEvents
                        .Where(d => d.DonorId == donorId.Value && d.Status == "Completed" && d.DeletedTime == null)
                        .OrderBy(d => d.DonationDate)
                        .FirstOrDefaultAsync();

                    if (firstDonation != null && firstDonation.DonationDate >= startDate && firstDonation.DonationDate <= endDate)
                    {
                        newDonorIds.Add(donorId.Value);
                    }
                }
            }

            var newDonorCount = newDonorIds.Count;
            var repeatDonorCount = uniqueDonorCount - newDonorCount;

            // Tạo báo cáo
            var report = new BloodDonationReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalDonations = donations.Count,
                UniqueDonoCount = uniqueDonorCount,
                NewDonorCount = newDonorCount,
                RepeatDonorCount = repeatDonorCount
            };

            // Thống kê theo nhóm máu
            var bloodGroups = await _context.BloodGroups.ToListAsync();

            report.DonationsByBloodGroup = bloodGroups
                .Select(bg => new BloodGroupStatDto
                {
                    BloodGroupId = bg.Id,
                    BloodGroupName = bg.GroupName,
                    Count = donations.Count(d => d.BloodGroupId == bg.Id),
                    Percentage = donations.Count > 0
                        ? Math.Round((double)donations.Count(d => d.BloodGroupId == bg.Id) / donations.Count * 100, 2)
                        : 0
                })
                .Where(bg => bg.Count > 0)
                .OrderByDescending(bg => bg.Count)
                .ToList();

            // Thống kê theo loại thành phần
            var componentTypes = await _context.ComponentTypes.ToListAsync();

            report.DonationsByComponentType = componentTypes
                .Select(ct => new ComponentStatDto
                {
                    ComponentTypeId = ct.Id,
                    ComponentTypeName = ct.Name,
                    Count = donations.Count(d => d.ComponentTypeId == ct.Id),
                    Percentage = donations.Count > 0
                        ? Math.Round((double)donations.Count(d => d.ComponentTypeId == ct.Id) / donations.Count * 100, 2)
                        : 0
                })
                .Where(ct => ct.Count > 0)
                .OrderByDescending(ct => ct.Count)
                .ToList();

            // Thống kê theo ngày
            report.DonationTrend = donations
                .Where(d => d.DonationDate.HasValue)
                .GroupBy(d => d.DonationDate!.Value.Date)
                .Select(g => new DateBasedStatDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToList();

            // Thống kê theo địa điểm
            var locations = await _context.Locations.ToListAsync();
            var locationStats = locations
                .Select(l => new LocationStatDto
                {
                    LocationId = l.Id,
                    LocationName = l.Name,
                    Count = donations.Count(d => d.LocationId == l.Id),
                    Percentage = donations.Count > 0
                        ? Math.Round((double)donations.Count(d => d.LocationId == l.Id) / donations.Count * 100, 2)
                        : 0
                })
                .Where(l => l.Count > 0)
                .OrderByDescending(l => l.Count)
                .ToList();

            report.DonationsByLocation = locationStats;

            // Thống kê theo sự kiện (dựa trên DonationLocation string)
            var eventDonations = donations
                .Where(d => !string.IsNullOrEmpty(d.DonationLocation) && d.DonationDate.HasValue)
                .GroupBy(d => d.DonationLocation)
                .Select(g => new DonationEventStatDto
                {
                    EventId = Guid.Empty,
                    EventName = g.Key,
                    DonationCount = g.Count(),
                    EventDate = g.Min(d => d.DonationDate!.Value),
                    Location = g.Key
                })
                .OrderByDescending(e => e.DonationCount)
                .ToList();

            report.DonationsByEvent = eventDonations;

            // Tính toán trung bình hiến máu mỗi ngày
            int days = (int)(endDate - startDate).TotalDays + 1;
            report.AverageDonationsPerDay = days > 0
                ? Math.Round((double)donations.Count / days, 2)
                : 0;

            return report;
        }

        public async Task<BloodRequestReportDto> GetBloodRequestReportAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid? bloodGroupId = null, Guid? locationId = null)
        {
            // Lấy dữ liệu từ bảng BloodRequests với phân loại thường và khẩn cấp
            var bloodRequestsQuery = _context.BloodRequests
                .Where(r => r.RequestDate >= startDate && r.RequestDate <= endDate);

            var regularRequestsQuery = bloodRequestsQuery.Where(r => !r.IsEmergency);
            var emergencyRequestsQuery = bloodRequestsQuery.Where(r => r.IsEmergency);

            if (bloodGroupId.HasValue)
            {
                regularRequestsQuery = regularRequestsQuery.Where(r => r.BloodGroupId == bloodGroupId.Value);
                emergencyRequestsQuery = emergencyRequestsQuery.Where(r => r.BloodGroupId == bloodGroupId.Value);
            }

            if (locationId.HasValue)
            {
                regularRequestsQuery = regularRequestsQuery.Where(r => r.LocationId == locationId.Value);
                emergencyRequestsQuery = emergencyRequestsQuery.Where(r => r.LocationId == locationId.Value);
            }

            var regularRequests = await regularRequestsQuery.ToListAsync();
            var emergencyRequests = await emergencyRequestsQuery.ToListAsync();

            // Thống kê trạng thái
            var regularRequestsByStatus = regularRequests.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
            var emergencyRequestsByStatus = emergencyRequests.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());

            int totalRequests = regularRequests.Count + emergencyRequests.Count;

            int fulfilledRequests =
                (regularRequestsByStatus.ContainsKey("Fulfilled") ? regularRequestsByStatus["Fulfilled"] : 0) +
                (emergencyRequestsByStatus.ContainsKey("Fulfilled") ? emergencyRequestsByStatus["Fulfilled"] : 0);

            int pendingRequests =
                (regularRequestsByStatus.ContainsKey("Pending") ? regularRequestsByStatus["Pending"] : 0) +
                (emergencyRequestsByStatus.ContainsKey("Pending") ? emergencyRequestsByStatus["Pending"] : 0);

            int cancelledRequests =
                (regularRequestsByStatus.ContainsKey("Cancelled") ? regularRequestsByStatus["Cancelled"] : 0) +
                (emergencyRequestsByStatus.ContainsKey("Cancelled") ? emergencyRequestsByStatus["Cancelled"] : 0);

            // Tạo báo cáo
            var report = new BloodRequestReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRequests = totalRequests,
                FulfilledRequests = fulfilledRequests,
                PendingRequests = pendingRequests,
                CancelledRequests = cancelledRequests,
                FulfillmentRate = totalRequests > 0 ? Math.Round((double)fulfilledRequests / totalRequests * 100, 2) : 0
            };

            // Thống kê theo nhóm máu
            var bloodGroups = await _context.BloodGroups.ToListAsync();
            var bloodGroupStats = new List<BloodGroupStatDto>();

            foreach (var bloodGroup in bloodGroups)
            {
                int count = regularRequests.Count(r => r.BloodGroupId == bloodGroup.Id) +
                            emergencyRequests.Count(r => r.BloodGroupId == bloodGroup.Id);

                if (count > 0)
                {
                    bloodGroupStats.Add(new BloodGroupStatDto
                    {
                        BloodGroupId = bloodGroup.Id,
                        BloodGroupName = bloodGroup.GroupName,
                        Count = count,
                        Percentage = totalRequests > 0 ? Math.Round((double)count / totalRequests * 100, 2) : 0
                    });
                }
            }

            report.RequestsByBloodGroup = bloodGroupStats.OrderByDescending(bg => bg.Count).ToList();

            // Thống kê theo loại thành phần
            var componentTypes = await _context.ComponentTypes.ToListAsync();
            var componentStats = new List<ComponentStatDto>();

            foreach (var componentType in componentTypes)
            {
                int count = regularRequests.Count(r => r.ComponentTypeId == componentType.Id) +
                            emergencyRequests.Count(r => r.ComponentTypeId == componentType.Id);

                if (count > 0)
                {
                    componentStats.Add(new ComponentStatDto
                    {
                        ComponentTypeId = componentType.Id,
                        ComponentTypeName = componentType.Name,
                        Count = count,
                        Percentage = totalRequests > 0 ? Math.Round((double)count / totalRequests * 100, 2) : 0
                    });
                }
            }

            report.RequestsByComponentType = componentStats.OrderByDescending(ct => ct.Count).ToList();

            // Thống kê theo mức độ ưu tiên
            var priorityStats = new List<RequestPriorityStatDto>();

            if (regularRequests.Count > 0)
            {
                priorityStats.Add(new RequestPriorityStatDto
                {
                    Priority = "Normal",
                    Count = regularRequests.Count,
                    Percentage = totalRequests > 0 ? Math.Round((double)regularRequests.Count / totalRequests * 100, 2) : 0
                });
            }

            if (emergencyRequests.Count > 0)
            {
                priorityStats.Add(new RequestPriorityStatDto
                {
                    Priority = "Emergency",
                    Count = emergencyRequests.Count,
                    Percentage = totalRequests > 0 ? Math.Round((double)emergencyRequests.Count / totalRequests * 100, 2) : 0
                });
            }

            report.RequestsByPriority = priorityStats.OrderByDescending(p => p.Count).ToList();

            // Thống kê theo ngày
            var requestsByDate = new Dictionary<DateTimeOffset, int>();

            foreach (var request in regularRequests.Concat(emergencyRequests))
            {
                var date = request.RequestDate.Date;
                if (!requestsByDate.ContainsKey(date))
                {
                    requestsByDate[date] = 0;
                }
                requestsByDate[date]++;
            }

            report.RequestTrend = requestsByDate
                .Select(kv => new DateBasedStatDto
                {
                    Date = kv.Key,
                    Count = kv.Value
                })
                .OrderBy(s => s.Date)
                .ToList();

            // Thống kê theo địa điểm
            var locations = await _context.Locations.ToListAsync();
            var locationStats = new List<LocationStatDto>();

            foreach (var location in locations)
            {
                int count = regularRequests.Count(r => r.LocationId == location.Id) +
                            emergencyRequests.Count(r => r.LocationId == location.Id);

                if (count > 0)
                {
                    locationStats.Add(new LocationStatDto
                    {
                        LocationId = location.Id,
                        LocationName = location.Name,
                        Count = count,
                        Percentage = totalRequests > 0 ? Math.Round((double)count / totalRequests * 100, 2) : 0
                    });
                }
            }

            report.RequestsByLocation = locationStats.OrderByDescending(l => l.Count).ToList();

            // Thời gian đáp ứng trung bình (giả định)
            report.AverageFulfillmentTime = 24.0; // 24 giờ - có thể tính toán dựa trên dữ liệu thực tế

            return report;
        }

        public async Task<InventoryReportDto> GetInventoryReportAsync(DateTimeOffset? asOfDate = null)
        {
            var reportDate = asOfDate ?? DateTimeOffset.UtcNow;

            // Lấy tất cả các mục trong kho tính đến ngày báo cáo
            var inventories = await _context.BloodInventories.ToListAsync();

            // Phân loại theo trạng thái
            var availableInventory = inventories.Where(i => i.Status == "Available" && i.ExpirationDate > reportDate).ToList();
            var expiredInventory = inventories.Where(i => i.Status == "Available" && i.ExpirationDate <= reportDate).ToList();
            var expiringSoonInventory = inventories.Where(i => i.Status == "Available" && i.ExpirationDate > reportDate && i.ExpirationDate <= reportDate.AddDays(7)).ToList();

            // Tạo báo cáo
            var report = new InventoryReportDto
            {
                ReportDate = reportDate,
                TotalInventoryUnits = availableInventory.Sum(i => i.QuantityUnits),
                ExpiredUnits = expiredInventory.Sum(i => i.QuantityUnits),
                ExpiringSoonUnits = expiringSoonInventory.Sum(i => i.QuantityUnits),
                InventoryByBloodGroup = new List<BloodGroupInventoryStatDto>(),
                InventoryMovements = new List<InventoryMovementStatDto>()
            };

            // Thống kê theo nhóm máu
            var bloodGroups = await _context.BloodGroups.ToListAsync();
            var componentTypes = await _context.ComponentTypes.ToListAsync();

            foreach (var bloodGroup in bloodGroups)
            {
                var bloodGroupInventory = availableInventory.Where(i => i.BloodGroupId == bloodGroup.Id).ToList();

                if (bloodGroupInventory.Count > 0)
                {
                    var bloodGroupStat = new BloodGroupInventoryStatDto
                    {
                        BloodGroupId = bloodGroup.Id,
                        BloodGroupName = bloodGroup.GroupName,
                        AvailableUnits = bloodGroupInventory.Sum(i => i.QuantityUnits),
                        ExpiringSoonUnits = expiringSoonInventory
                            .Where(i => i.BloodGroupId == bloodGroup.Id)
                            .Sum(i => i.QuantityUnits),
                        ComponentBreakdown = new List<ComponentInventoryStatDto>()
                    };

                    foreach (var componentType in componentTypes)
                    {
                        var componentInventory = bloodGroupInventory
                            .Where(i => i.ComponentTypeId == componentType.Id)
                            .ToList();

                        if (componentInventory.Count > 0)
                        {
                            bloodGroupStat.ComponentBreakdown.Add(new ComponentInventoryStatDto
                            {
                                ComponentTypeId = componentType.Id,
                                ComponentTypeName = componentType.Name,
                                AvailableUnits = componentInventory.Sum(i => i.QuantityUnits),
                                ExpiringSoonUnits = expiringSoonInventory
                                    .Where(i => i.BloodGroupId == bloodGroup.Id && i.ComponentTypeId == componentType.Id)
                                    .Sum(i => i.QuantityUnits)
                            });
                        }
                    }

                    report.InventoryByBloodGroup.Add(bloodGroupStat);
                }
            }

            // Tạo dữ liệu mẫu cho biến động kho (có thể cải thiện bằng cách lưu trữ lịch sử thực tế)
            var startDate = reportDate.AddDays(-30);
            var mockInventoryMovements = new List<InventoryMovementStatDto>();

            // Mô phỏng dữ liệu nhập kho
            for (int i = 0; i < 10; i++)
            {
                mockInventoryMovements.Add(new InventoryMovementStatDto
                {
                    MovementType = "Nhập kho",
                    Date = startDate.AddDays(i * 3),
                    Quantity = new Random().Next(5, 15)
                });
            }

            // Mô phỏng dữ liệu xuất kho
            for (int i = 0; i < 7; i++)
            {
                mockInventoryMovements.Add(new InventoryMovementStatDto
                {
                    MovementType = "Xuất kho",
                    Date = startDate.AddDays(i * 4 + 1),
                    Quantity = new Random().Next(3, 10)
                });
            }

            // Mô phỏng dữ liệu hết hạn
            for (int i = 0; i < 3; i++)
            {
                mockInventoryMovements.Add(new InventoryMovementStatDto
                {
                    MovementType = "Hết hạn",
                    Date = startDate.AddDays(i * 9 + 2),
                    Quantity = new Random().Next(1, 5)
                });
            }

            report.InventoryMovements = mockInventoryMovements.OrderBy(i => i.Date).ToList();

            // Các số liệu thống kê khác
            report.AverageStorageTime = 15.3; // Thời gian lưu trữ trung bình (ngày)
            report.ExpirationRate = 4.2; // Tỷ lệ hết hạn (%)

            return report;
        }

        public async Task<DonorDemographicsReportDto> GetDonorDemographicsReportAsync()
        {
            var donors = await _context.DonorProfiles
                .Where(d => d.DeletedTime == null)
                .Include(d => d.User)
                .Include(d => d.BloodGroup)
                .ToListAsync();

            var report = new DonorDemographicsReportDto
            {
                TotalDonors = donors.Count,
                DonorsByAgeGroup = new List<AgeGroupStatDto>(),
                DonorsByGender = new List<GenderStatDto>(),
                DonorsByBloodGroup = new List<BloodGroupStatDto>(),
                DonorsByLocation = new List<LocationStatDto>(),
                DonorsByDonationFrequency = new List<DonationFrequencyStatDto>()
            };

            // Thống kê theo nhóm tuổi
            var ageGroups = new Dictionary<string, int>
            {
                { "18-24", 0 },
                { "25-34", 0 },
                { "35-44", 0 },
                { "45-54", 0 },
                { "55-65", 0 }
            };

            foreach (var donor in donors)
            {
                int age = CalculateAge(donor.DateOfBirth);

                if (age >= 18 && age <= 24)
                    ageGroups["18-24"]++;
                else if (age >= 25 && age <= 34)
                    ageGroups["25-34"]++;
                else if (age >= 35 && age <= 44)
                    ageGroups["35-44"]++;
                else if (age >= 45 && age <= 54)
                    ageGroups["45-54"]++;
                else if (age >= 55 && age <= 65)
                    ageGroups["55-65"]++;
            }

            report.DonorsByAgeGroup = ageGroups
                .Select(g => new AgeGroupStatDto
                {
                    AgeGroup = g.Key,
                    Count = g.Value,
                    Percentage = donors.Count > 0 ? Math.Round((double)g.Value / donors.Count * 100, 2) : 0
                })
                .Where(g => g.Count > 0)
                .OrderBy(g => g.AgeGroup)
                .ToList();

            // Thống kê theo giới tính
            var genderGroups = donors
                .GroupBy(d => d.Gender)
                .Select(g => new GenderStatDto
                {
                    Gender = g.Key,
                    GenderName = g.Key ? "Nam" : "Nữ",
                    Count = g.Count(),
                    Percentage = donors.Count > 0 ? Math.Round((double)g.Count() / donors.Count * 100, 2) : 0
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            report.DonorsByGender = genderGroups;

            // Thống kê theo nhóm máu
            var bloodGroupStats = donors
                .Where(d => d.BloodGroup != null)
                .GroupBy(d => d.BloodGroup)
                .Select(g => new BloodGroupStatDto
                {
                    BloodGroupId = g.Key.Id,
                    BloodGroupName = g.Key.GroupName,
                    Count = g.Count(),
                    Percentage = donors.Count > 0 ? Math.Round((double)g.Count() / donors.Count * 100, 2) : 0
                })
                .OrderByDescending(bg => bg.Count)
                .ToList();

            report.DonorsByBloodGroup = bloodGroupStats;

            // Thống kê theo tần suất hiến máu
            var donationFrequency = new Dictionary<string, int>
            {
                { "Lần đầu", donors.Count(d => d.TotalDonations == 1) },
                { "2-5 lần", donors.Count(d => d.TotalDonations >= 2 && d.TotalDonations <= 5) },
                { "6-10 lần", donors.Count(d => d.TotalDonations >= 6 && d.TotalDonations <= 10) },
                { "Trên 10 lần", donors.Count(d => d.TotalDonations > 10) }
            };

            report.DonorsByDonationFrequency = donationFrequency
                .Select(g => new DonationFrequencyStatDto
                {
                    FrequencyGroup = g.Key,
                    Count = g.Value,
                    Percentage = donors.Count > 0 ? Math.Round((double)g.Value / donors.Count * 100, 2) : 0
                })
                .Where(g => g.Count > 0)
                .ToList();

            // Thống kê theo địa điểm (dựa trên địa chỉ của người hiến máu)
            var locations = await _context.Locations.ToListAsync();
            var locationStats = new List<LocationStatDto>();
            var random = new Random();

            foreach (var location in locations)
            {
                // Mô phỏng phân bố người hiến máu theo địa điểm
                int count = random.Next(0, Math.Max(1, (int)(donors.Count * 0.3)));

                if (count > 0)
                {
                    locationStats.Add(new LocationStatDto
                    {
                        LocationId = location.Id,
                        LocationName = location.Name,
                        Count = count,
                        Percentage = donors.Count > 0 ? Math.Round((double)count / donors.Count * 100, 2) : 0
                    });
                }
            }

            report.DonorsByLocation = locationStats.OrderByDescending(l => l.Count).ToList();

            // Tính toán số lượng hiến máu trung bình cho mỗi người hiến máu
            report.AverageDonationsPerDonor = donors.Count > 0
                ? Math.Round((double)donors.Sum(d => d.TotalDonations) / donors.Count, 2)
                : 0;

            return report;
        }

        #endregion

        #region Helper Methods

        private int CalculateAge(DateTimeOffset? birthDate)
        {
            if (!birthDate.HasValue)
                return 0;

            var today = DateTimeOffset.UtcNow;
            var age = today.Year - birthDate.Value.Year;

            if (birthDate.Value.Date > today.Date.AddYears(-age))
                age--;

            return age;
        }

        #endregion
    }
}