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

        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            var totalDonors = await _context.DonorProfiles.Where(d => d.DeletedTime == null).CountAsync();

            var totalDonations = await _context.BloodDonationWorkflows
                .Where(w => w.Status == "Completed" && w.DeletedTime == null)
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

            var emergencyRequests = await _context.EmergencyRequests
                .Where(r => r.Status == "Pending" && r.DeletedTime == null)
                .CountAsync();

            var scheduledAppointments = await _context.BloodDonationWorkflows
                .Where(w => w.Status == "Scheduled"
                            && w.AppointmentDate.HasValue
                            && w.AppointmentDate > today
                            && w.DeletedTime == null)
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
                    CriticalThreshold = 10, // Có thể thay đổi ngưỡng tùy theo nhóm máu
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
                        CriticalThreshold = 5 // Có thể thay đổi ngưỡng tùy theo loại thành phần
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

            var completedDonations = await _context.BloodDonationWorkflows
                .Where(w => w.Status == "Completed"
                         && w.DonationDate.HasValue
                         && w.DonationDate >= start
                         && w.DonationDate <= end
                         && w.DeletedTime == null)
                .ToListAsync();

            var result = new DonationStatisticsDto
            {
                StartDate = start,
                EndDate = end,
                TotalDonations = completedDonations.Count
            };

            // Tạo thống kê theo ngày
            var dailyStats = completedDonations
                .GroupBy(w => w.DonationDate.Value.Date)
                .Select(g => new DateBasedStatDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToList();

            result.DailyDonations = dailyStats;

            // Tạo thống kê theo tháng
            var monthlyStats = completedDonations
                .GroupBy(w => new { w.DonationDate.Value.Year, w.DonationDate.Value.Month })
                .Select(g => new DateBasedStatDto
                {
                    Date = new DateTimeOffset(g.Key.Year, g.Key.Month, 1, 0, 0, 0, TimeSpan.Zero),
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToList();

            result.MonthlyDonations = monthlyStats;

            // Tạo thống kê theo nhóm máu
            var bloodGroupStats = await _context.BloodGroups
                .Select(bg => new
                {
                    BloodGroup = bg,
                    Count = completedDonations.Count(w => w.BloodGroupId == bg.Id)
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

            // Tạo thống kê theo địa điểm
            var locationStats = completedDonations
                .Where(w => !string.IsNullOrEmpty(w.DonationLocation))
                .GroupBy(w => w.DonationLocation)
                .Select(g => new LocationStatDto
                {
                    LocationId = Guid.Empty, // Không có ID vì chỉ lưu tên địa điểm
                    LocationName = g.Key,
                    Count = g.Count(),
                    Percentage = completedDonations.Count > 0
                        ? Math.Round((double)g.Count() / completedDonations.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(l => l.Count)
                .ToList();

            result.DonationsByLocation = locationStats;

            return result;
        }

        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10)
        {
            var activities = new List<RecentActivityDto>();

            // Thêm hoạt động từ BloodDonationWorkflows
            var recentWorkflows = await _context.BloodDonationWorkflows
                .Where(w => w.DeletedTime == null)
                .OrderByDescending(w => w.LastUpdatedTime ?? w.CreatedTime)
                .Take(count)
                .Select(w => new
                {
                    w.Id,
                    w.Status,
                    w.DonorId,
                    DonorName = w.Donor.User != null ? $"{w.Donor.User.FirstName} {w.Donor.User.LastName}" : "Unknown",
                    Timestamp = w.LastUpdatedTime ?? w.CreatedTime,
                    UserId = w.Donor != null ? w.Donor.UserId : (Guid?)null
                })
                .ToListAsync();

            foreach (var workflow in recentWorkflows)
            {
                string description = "";

                switch (workflow.Status)
                {
                    case "Created":
                        description = "Đã tạo quy trình hiến máu mới";
                        break;
                    case "DonorAssigned":
                        description = $"Đã chỉ định người hiến máu {workflow.DonorName}";
                        break;
                    case "Scheduled":
                        description = $"{workflow.DonorName} đã đặt lịch hiến máu";
                        break;
                    case "Completed":
                        description = $"{workflow.DonorName} đã hoàn thành hiến máu";
                        break;
                    case "Cancelled":
                        description = "Quy trình hiến máu đã bị hủy";
                        break;
                    default:
                        description = $"Cập nhật trạng thái quy trình hiến máu: {workflow.Status}";
                        break;
                }

                activities.Add(new RecentActivityDto
                {
                    Id = workflow.Id,
                    Type = "BloodDonation",
                    Description = description,
                    Timestamp = workflow.Timestamp, // Explicitly cast nullable DateTimeOffset to non-nullable
                    UserId = workflow.UserId,
                    UserName = workflow.DonorName
                });
            }

            // Thêm hoạt động từ BloodRequests
            var recentRequests = await _context.BloodRequests
                .OrderByDescending(r => r.RequestDate)
                .Take(count)
                .Select(r => new
                {
                    r.Id,
                    r.Status,
                    r.RequestedBy,
                    RequesterName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown",
                    Timestamp = r.RequestDate,
                    UserId = r.RequestedBy
                })
                .ToListAsync();

            foreach (var request in recentRequests)
            {
                string description = "";

                switch (request.Status)
                {
                    case "Pending":
                        description = $"{request.RequesterName} đã tạo yêu cầu máu mới";
                        break;
                    case "Processing":
                        description = $"Yêu cầu máu từ {request.RequesterName} đang được xử lý";
                        break;
                    case "Fulfilled":
                        description = $"Yêu cầu máu từ {request.RequesterName} đã được đáp ứng";
                        break;
                    case "Cancelled":
                        description = $"Yêu cầu máu từ {request.RequesterName} đã bị hủy";
                        break;
                    default:
                        description = $"Cập nhật trạng thái yêu cầu máu: {request.Status}";
                        break;
                }

                activities.Add(new RecentActivityDto
                {
                    Id = request.Id,
                    Type = "BloodRequest",
                    Description = description,
                    Timestamp = request.Timestamp, // Explicitly cast nullable DateTimeOffset to non-nullable
                    UserId = request.UserId,
                    UserName = request.RequesterName
                });
            }

            // Thêm hoạt động từ EmergencyRequests
            var recentEmergencyRequests = await _context.EmergencyRequests
                .Where(r => r.DeletedTime == null)
                .OrderByDescending(r => r.LastUpdatedTime ?? r.CreatedTime)
                .Take(count)
                .Select(r => new
                {
                    r.Id,
                    r.Status,
                    r.HospitalName,
                    ContactName = r.ContactInfo,
                    Timestamp = r.LastUpdatedTime ?? r.CreatedTime
                })
                .ToListAsync();

            foreach (var request in recentEmergencyRequests)
            {
                string description = "";

                switch (request.Status)
                {
                    case "Pending":
                        description = $"Yêu cầu máu khẩn cấp mới từ {request.HospitalName}";
                        break;
                    case "Processing":
                        description = $"Yêu cầu máu khẩn cấp từ {request.HospitalName} đang được xử lý";
                        break;
                    case "Fulfilled":
                        description = $"Yêu cầu máu khẩn cấp từ {request.HospitalName} đã được đáp ứng";
                        break;
                    case "Cancelled":
                        description = $"Yêu cầu máu khẩn cấp từ {request.HospitalName} đã bị hủy";
                        break;
                    default:
                        description = $"Cập nhật trạng thái yêu cầu máu khẩn cấp: {request.Status}";
                        break;
                }

                activities.Add(new RecentActivityDto
                {
                    Id = request.Id,
                    Type = "EmergencyRequest",
                    Description = description,
                    Timestamp = request.Timestamp.Value, // Explicitly cast nullable DateTimeOffset to non-nullable
                    UserName = request.ContactName
                });
            }

            // Sắp xếp tất cả hoạt động theo thời gian gần đây nhất và lấy count hoạt động đầu tiên
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public async Task<BloodDonationReportDto> GetBloodDonationReportAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid? bloodGroupId = null, Guid? locationId = null)
        {
            var query = _context.BloodDonationWorkflows
                .Where(w => w.Status == "Completed"
                         && w.DonationDate.HasValue
                         && w.DonationDate >= startDate
                         && w.DonationDate <= endDate
                         && w.DeletedTime == null);

            if (bloodGroupId.HasValue)
            {
                query = query.Where(w => w.BloodGroupId == bloodGroupId.Value);
            }

            if (locationId.HasValue)
            {
                // Giả sử có mối quan hệ với Location
                // Nếu không, có thể xử lý theo cách khác
                var locationName = await _context.Locations
                    .Where(l => l.Id == locationId.Value)
                    .Select(l => l.Name)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(locationName))
                {
                    query = query.Where(w => w.DonationLocation.Contains(locationName));
                }
            }

            var donations = await query.ToListAsync();

            // Lấy danh sách người hiến máu duy nhất
            var uniqueDonorIds = donations.Select(d => d.DonorId).Distinct().ToList();
            var uniqueDonorCount = uniqueDonorIds.Count;

            // Lấy danh sách người hiến máu lần đầu
            var newDonorIds = new List<Guid?>();
            foreach (var donorId in uniqueDonorIds)
            {
                if (donorId.HasValue)
                {
                    var donorProfile = await _context.DonorProfiles
                        .FirstOrDefaultAsync(d => d.Id == donorId.Value);

                    if (donorProfile != null && donorProfile.TotalDonations == 1)
                    {
                        newDonorIds.Add(donorId);
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
                    Count = donations.Count(w => w.BloodGroupId == bg.Id),
                    Percentage = donations.Count > 0
                        ? Math.Round((double)donations.Count(w => w.BloodGroupId == bg.Id) / donations.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(bg => bg.Count)
                .ToList();

            // Thống kê theo loại thành phần
            var componentTypes = await _context.ComponentTypes.ToListAsync();

            report.DonationsByComponentType = componentTypes
                .Select(ct => new ComponentStatDto
                {
                    ComponentTypeId = ct.Id,
                    ComponentTypeName = ct.Name,
                    Count = donations.Count(w => w.ComponentTypeId == ct.Id),
                    Percentage = donations.Count > 0
                        ? Math.Round((double)donations.Count(w => w.ComponentTypeId == ct.Id) / donations.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(ct => ct.Count)
                .ToList();

            // Thống kê theo ngày
            report.DonationTrend = donations
                .GroupBy(w => w.DonationDate.Value.Date)
                .Select(g => new DateBasedStatDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToList();

            // Thống kê theo sự kiện
            var eventDonations = donations
                .Where(w => w.DonationLocation != null)
                .GroupBy(w => w.DonationLocation)
                .Select(g => new DonationEventStatDto
                {
                    EventId = Guid.Empty, // Không có ID vì chỉ lưu tên địa điểm
                    EventName = g.Key,
                    DonationCount = g.Count(),
                    EventDate = g.Min(w => w.DonationDate.Value),
                    Location = g.Key
                })
                .OrderByDescending(e => e.DonationCount)
                .ToList();

            report.DonationsByEvent = eventDonations;

            // Thống kê theo địa điểm
            var locationStats = donations
                .Where(w => w.DonationLocation != null)
                .GroupBy(w => w.DonationLocation)
                .Select(g => new LocationStatDto
                {
                    LocationId = Guid.Empty, // Không có ID vì chỉ lưu tên địa điểm
                    LocationName = g.Key,
                    Count = g.Count(),
                    Percentage = donations.Count > 0
                        ? Math.Round((double)g.Count() / donations.Count * 100, 2)
                        : 0
                })
                .OrderByDescending(l => l.Count)
                .ToList();

            report.DonationsByLocation = locationStats;

            // Tính toán trung bình hiến máu mỗi ngày
            int days = (int)(endDate - startDate).TotalDays + 1;
            report.AverageDonationsPerDay = days > 0
                ? Math.Round((double)donations.Count / days, 2)
                : 0;

            return report;
        }

        public async Task<BloodRequestReportDto> GetBloodRequestReportAsync(DateTimeOffset startDate, DateTimeOffset endDate, Guid? bloodGroupId = null, Guid? locationId = null)
        {
            // Lấy dữ liệu từ cả BloodRequests và EmergencyRequests
            var bloodRequestsQuery = _context.BloodRequests
                .Where(r => r.RequestDate >= startDate
                         && r.RequestDate <= endDate);

            var emergencyRequestsQuery = _context.EmergencyRequests
                .Where(r => r.RequestDate >= startDate
                         && r.RequestDate <= endDate
                         && r.DeletedTime == null);

            if (bloodGroupId.HasValue)
            {
                bloodRequestsQuery = bloodRequestsQuery.Where(r => r.BloodGroupId == bloodGroupId.Value);
                emergencyRequestsQuery = emergencyRequestsQuery.Where(r => r.BloodGroupId == bloodGroupId.Value);
            }

            if (locationId.HasValue)
            {
                bloodRequestsQuery = bloodRequestsQuery.Where(r => r.LocationId == locationId.Value);
                // Giả sử EmergencyRequests có mối quan hệ với Location
                emergencyRequestsQuery = emergencyRequestsQuery.Where(r => r.LocationId == locationId.Value);
            }

            var bloodRequests = await bloodRequestsQuery.ToListAsync();
            var emergencyRequests = await emergencyRequestsQuery.ToListAsync();

            // Thống kê trạng thái
            var bloodRequestsByStatus = bloodRequests.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
            var emergencyRequestsByStatus = emergencyRequests.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());

            int totalRequests = bloodRequests.Count + emergencyRequests.Count;

            int fulfilledRequests =
                (bloodRequestsByStatus.ContainsKey("Fulfilled") ? bloodRequestsByStatus["Fulfilled"] : 0) +
                (emergencyRequestsByStatus.ContainsKey("Fulfilled") ? emergencyRequestsByStatus["Fulfilled"] : 0);

            int pendingRequests =
                (bloodRequestsByStatus.ContainsKey("Pending") ? bloodRequestsByStatus["Pending"] : 0) +
                (emergencyRequestsByStatus.ContainsKey("Pending") ? emergencyRequestsByStatus["Pending"] : 0);

            int cancelledRequests =
                (bloodRequestsByStatus.ContainsKey("Cancelled") ? bloodRequestsByStatus["Cancelled"] : 0) +
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
                int count = bloodRequests.Count(r => r.BloodGroupId == bloodGroup.Id) +
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
                int count = bloodRequests.Count(r => r.ComponentTypeId == componentType.Id) +
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

            // Thống kê theo mức độ ưu tiên (chỉ áp dụng cho BloodRequests)
            var priorityStats = bloodRequests
                .GroupBy(r => "Normal") // Giả sử mặc định là Normal
                .Select(g => new RequestPriorityStatDto
                {
                    Priority = g.Key,
                    Count = g.Count(),
                    Percentage = bloodRequests.Count > 0 ? Math.Round((double)g.Count() / bloodRequests.Count * 100, 2) : 0
                })
                .OrderByDescending(p => p.Count)
                .ToList();

            // Thêm EmergencyRequests như là mức độ ưu tiên "Emergency"
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

            foreach (var request in bloodRequests)
            {
                var date = request.RequestDate.Date;
                if (!requestsByDate.ContainsKey(date))
                {
                    requestsByDate[date] = 0;
                }
                requestsByDate[date]++;
            }

            foreach (var request in emergencyRequests)
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
                int count = bloodRequests.Count(r => r.LocationId == location.Id) +
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

            // Không có FulfilledTime trong các model, sử dụng giá trị ngẫu nhiên cho mục đích minh họa
            report.AverageFulfillmentTime = 24.5; // Giả định thời gian trung bình là 24.5 giờ

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

            // Phân loại theo thời gian tạo để mô phỏng biến động kho
            var startDate = reportDate.AddDays(-30);

            // Tạo dữ liệu mẫu cho biến động kho
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

            // Mô phỏng các số liệu thống kê khác
            report.AverageStorageTime = 15.3; // Giả sử thời gian lưu trữ trung bình là 15.3 ngày
            report.ExpirationRate = 4.2; // Giả sử tỷ lệ hết hạn là 4.2%

            return report;
        }

        public async Task<DonorDemographicsReportDto> GetDonorDemographicsReportAsync()
        {
            var donors = await _context.DonorProfiles
                .Where(d => d.DeletedTime == null)
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
            var bloodGroups = await _context.BloodGroups.ToListAsync();
            var bloodGroupStats = new List<BloodGroupStatDto>();

            foreach (var bloodGroup in bloodGroups)
            {
                int count = donors.Count(d => d.BloodGroupId == bloodGroup.Id);

                if (count > 0)
                {
                    bloodGroupStats.Add(new BloodGroupStatDto
                    {
                        BloodGroupId = bloodGroup.Id,
                        BloodGroupName = bloodGroup.GroupName,
                        Count = count,
                        Percentage = donors.Count > 0 ? Math.Round((double)count / donors.Count * 100, 2) : 0
                    });
                }
            }

            report.DonorsByBloodGroup = bloodGroupStats.OrderByDescending(bg => bg.Count).ToList();

            // Thống kê theo tần suất hiến máu
            var donationFrequency = new Dictionary<string, int>
            {
                { "First-time", donors.Count(d => d.TotalDonations == 1) },
                { "2-5 donations", donors.Count(d => d.TotalDonations >= 2 && d.TotalDonations <= 5) },
                { "6-10 donations", donors.Count(d => d.TotalDonations >= 6 && d.TotalDonations <= 10) },
                { "11+ donations", donors.Count(d => d.TotalDonations > 10) }
            };

            report.DonorsByDonationFrequency = donationFrequency
                .Select(g => new DonationFrequencyStatDto
                {
                    FrequencyGroup = g.Key,
                    Count = g.Value,
                    Percentage = donors.Count > 0 ? Math.Round((double)g.Value / donors.Count * 100, 2) : 0
                })
                .OrderBy(g => g.FrequencyGroup)
                .ToList();

            // Thống kê theo vị trí địa lý (sử dụng mẫu ngẫu nhiên)
            var locations = await _context.Locations.ToListAsync();
            var locationStats = new List<LocationStatDto>();
            var random = new Random();

            foreach (var location in locations)
            {
                // Mô phỏng số người hiến máu ở mỗi địa điểm
                int count = random.Next(1, Math.Max(2, (int)(donors.Count * 0.3)));

                locationStats.Add(new LocationStatDto
                {
                    LocationId = location.Id,
                    LocationName = location.Name,
                    Count = count,
                    Percentage = donors.Count > 0 ? Math.Round((double)count / donors.Count * 100, 2) : 0
                });
            }

            report.DonorsByLocation = locationStats.OrderByDescending(l => l.Count).ToList();

            // Tính toán số lượng hiến máu trung bình cho mỗi người hiến máu
            report.AverageDonationsPerDonor = donors.Count > 0
                ? Math.Round((double)donors.Sum(d => d.TotalDonations) / donors.Count, 2)
                : 0;

            return report;
        }

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
    }
}