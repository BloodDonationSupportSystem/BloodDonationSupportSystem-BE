using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    /// <summary>
    /// DTO containing all dashboard information for a Member user
    /// </summary>
    public class MemberDashboardDto
    {
        /// <summary>
        /// Basic user information
        /// </summary>
        public UserDto User { get; set; }

        /// <summary>
        /// The member's donor profile information
        /// </summary>
        public DonorProfileDto DonorProfile { get; set; }

        /// <summary>
        /// The eligibility status of the member for donation
        /// </summary>
        public bool IsEligibleToDonate { get; set; }

        /// <summary>
        /// Reason for ineligibility if the member is not eligible to donate
        /// </summary>
        public string IneligibilityReason { get; set; }

        /// <summary>
        /// Date when the member will be eligible to donate again
        /// </summary>
        public DateTimeOffset? NextEligibleDonationDate { get; set; }

        /// <summary>
        /// Member's donation history
        /// </summary>
        public List<DonationEventDto> RecentDonations { get; set; } = new List<DonationEventDto>();

        /// <summary>
        /// Total lifetime donations made by the member
        /// </summary>
        public int TotalDonationsCount { get; set; }

        /// <summary>
        /// Total lifetime donation volume in milliliters
        /// </summary>
        public int TotalDonationVolume { get; set; }

        /// <summary>
        /// Number of lives potentially saved through donations
        /// </summary>
        public int EstimatedLivesSaved { get; set; }

        /// <summary>
        /// Upcoming donation appointments for the member
        /// </summary>
        public List<DonationAppointmentRequestDto> UpcomingAppointments { get; set; } = new List<DonationAppointmentRequestDto>();

        /// <summary>
        /// Blood requests made by the member
        /// </summary>
        public List<BloodRequestDto> MyBloodRequests { get; set; } = new List<BloodRequestDto>();

        /// <summary>
        /// Recent notifications for the member
        /// </summary>
        public List<NotificationDto> RecentNotifications { get; set; } = new List<NotificationDto>();

        /// <summary>
        /// Recent public emergency requests in the member's area
        /// </summary>
        public List<EmergencyBloodRequestDto> NearbyEmergencyRequests { get; set; } = new List<EmergencyBloodRequestDto>();

        /// <summary>
        /// Achievement badges earned by the member
        /// </summary>
        public List<string> AchievementBadges { get; set; } = new List<string>();

        /// <summary>
        /// Donation centers near the member's location
        /// </summary>
        public List<LocationDto> NearbyDonationCenters { get; set; } = new List<LocationDto>();

        /// <summary>
        /// Blood donation tips and educational resources
        /// </summary>
        public List<BlogPostDto> EducationalResources { get; set; } = new List<BlogPostDto>();
    }

    /// <summary>
    /// DTO containing all dashboard information for a Staff user
    /// </summary>
    public class StaffDashboardDto
    {
        /// <summary>
        /// List of active blood requests including emergency requests
        /// </summary>
        public List<BloodRequestDto> ActiveBloodRequests { get; set; } = new List<BloodRequestDto>();

        /// <summary>
        /// List of emergency blood requests that need immediate attention
        /// </summary>
        public List<EmergencyBloodRequestDto> EmergencyRequests { get; set; } = new List<EmergencyBloodRequestDto>();

        /// <summary>
        /// Total count of blood requests by status
        /// </summary>
        public Dictionary<string, int> RequestStatusCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Summary of current blood inventory levels by blood group and component type
        /// </summary>
        public List<BloodInventorySummaryDto> InventorySummary { get; set; } = new List<BloodInventorySummaryDto>();

        /// <summary>
        /// List of blood groups with critical inventory levels
        /// </summary>
        public List<CriticalInventoryDto> CriticalInventoryLevels { get; set; } = new List<CriticalInventoryDto>();

        /// <summary>
        /// Recent donation appointments
        /// </summary>
        public List<DonationAppointmentRequestDto> RecentAppointments { get; set; } = new List<DonationAppointmentRequestDto>();

        /// <summary>
        /// Upcoming donation appointments
        /// </summary>
        public List<DonationAppointmentRequestDto> UpcomingAppointments { get; set; } = new List<DonationAppointmentRequestDto>();

        /// <summary>
        /// Recent donation events
        /// </summary>
        public List<DonationEventDto> RecentDonations { get; set; } = new List<DonationEventDto>();

        /// <summary>
        /// Expired or nearly expired blood inventory items
        /// </summary>
        public List<BloodInventoryDto> ExpiringInventory { get; set; } = new List<BloodInventoryDto>();

        /// <summary>
        /// Donation activity statistics for today
        /// </summary>
        public DailyActivityDto TodayActivity { get; set; } = new DailyActivityDto();

        /// <summary>
        /// Available donors nearby for emergency requests
        /// </summary>
        public List<DonorProfileDto> AvailableEmergencyDonors { get; set; } = new List<DonorProfileDto>();

        /// <summary>
        /// Blood donation and request trends over time
        /// </summary>
        public TrendsDto Trends { get; set; } = new TrendsDto();
    }

    /// <summary>
    /// DTO containing all dashboard information for an Admin user
    /// </summary>
    public class AdminDashboardDto
    {
        /// <summary>
        /// Overall system statistics
        /// </summary>
        public SystemStatisticsDto SystemStats { get; set; } = new SystemStatisticsDto();

        /// <summary>
        /// User activity statistics
        /// </summary>
        public UserStatisticsDto UserStats { get; set; } = new UserStatisticsDto();

        /// <summary>
        /// Detailed inventory statistics
        /// </summary>
        public InventoryStatisticsDto InventoryStats { get; set; } = new InventoryStatisticsDto();

        /// <summary>
        /// Donation statistics
        /// </summary>
        public DonationStatisticsDto DonationStats { get; set; } = new DonationStatisticsDto();

        /// <summary>
        /// Blood request statistics
        /// </summary>
        public RequestStatisticsDto RequestStats { get; set; } = new RequestStatisticsDto();

        /// <summary>
        /// Recent system activities
        /// </summary>
        public List<ActivityLogDto> RecentActivities { get; set; } = new List<ActivityLogDto>();

        /// <summary>
        /// Active emergency requests
        /// </summary>
        public List<EmergencyBloodRequestDto> ActiveEmergencyRequests { get; set; } = new List<EmergencyBloodRequestDto>();

        /// <summary>
        /// Performance metrics
        /// </summary>
        public PerformanceMetricsDto PerformanceMetrics { get; set; } = new PerformanceMetricsDto();

        /// <summary>
        /// Geographic distribution of donors and requests
        /// </summary>
        public GeographicDistributionDto GeographicDistribution { get; set; } = new GeographicDistributionDto();
    }

    /// <summary>
    /// DTO containing emergency dashboard information for real-time updates
    /// </summary>
    public class EmergencyDashboardDto
    {
        /// <summary>
        /// Active emergency blood requests
        /// </summary>
        public List<EmergencyBloodRequestDto> ActiveEmergencyRequests { get; set; } = new List<EmergencyBloodRequestDto>();

        /// <summary>
        /// Available donors for emergency requests
        /// </summary>
        public List<DonorProfileDto> AvailableEmergencyDonors { get; set; } = new List<DonorProfileDto>();

        /// <summary>
        /// Inventory levels for blood types needed in emergencies
        /// </summary>
        public List<BloodInventorySummaryDto> EmergencyInventoryLevels { get; set; } = new List<BloodInventorySummaryDto>();

        /// <summary>
        /// Timestamp of the last update
        /// </summary>
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// DTO for blood inventory summary by blood group and component type
    /// </summary>
    public class BloodInventorySummaryDto
    {
        /// <summary>
        /// The blood group ID
        /// </summary>
        public Guid BloodGroupId { get; set; }

        /// <summary>
        /// The blood group name (e.g., A+, O-, etc.)
        /// </summary>
        public string BloodGroupName { get; set; }

        /// <summary>
        /// The component type ID
        /// </summary>
        public Guid ComponentTypeId { get; set; }

        /// <summary>
        /// The component type name (e.g., Whole Blood, Plasma, etc.)
        /// </summary>
        public string ComponentTypeName { get; set; }

        /// <summary>
        /// Available quantity in units
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Minimum recommended quantity
        /// </summary>
        public int MinimumRecommended { get; set; }

        /// <summary>
        /// Optimal quantity
        /// </summary>
        public int OptimalQuantity { get; set; }

        /// <summary>
        /// Status of the inventory level (Critical, Low, Normal, Surplus)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Percentage of available quantity compared to optimal quantity
        /// </summary>
        public double AvailabilityPercentage { get; set; }
    }

    /// <summary>
    /// DTO for critical inventory levels
    /// </summary>
    public class CriticalInventoryDto
    {
        /// <summary>
        /// The blood group ID
        /// </summary>
        public Guid BloodGroupId { get; set; }

        /// <summary>
        /// The blood group name (e.g., A+, O-, etc.)
        /// </summary>
        public string BloodGroupName { get; set; }

        /// <summary>
        /// The component type ID
        /// </summary>
        public Guid ComponentTypeId { get; set; }

        /// <summary>
        /// The component type name (e.g., Whole Blood, Plasma, etc.)
        /// </summary>
        public string ComponentTypeName { get; set; }

        /// <summary>
        /// Available quantity in units
        /// </summary>
        public int AvailableQuantity { get; set; }

        /// <summary>
        /// Minimum recommended quantity
        /// </summary>
        public int MinimumRecommended { get; set; }

        /// <summary>
        /// Criticality level (High, Medium, Low)
        /// </summary>
        public string CriticalityLevel { get; set; }

        /// <summary>
        /// Days until depletion at current usage rate
        /// </summary>
        public int EstimatedDaysUntilDepletion { get; set; }
    }

    /// <summary>
    /// DTO for daily donation and request activities
    /// </summary>
    public class DailyActivityDto
    {
        /// <summary>
        /// Number of donations made today
        /// </summary>
        public int DonationsToday { get; set; }

        /// <summary>
        /// Number of new blood requests today
        /// </summary>
        public int NewRequestsToday { get; set; }

        /// <summary>
        /// Number of fulfilled requests today
        /// </summary>
        public int FulfilledRequestsToday { get; set; }

        /// <summary>
        /// Number of new donors registered today
        /// </summary>
        public int NewDonorsToday { get; set; }

        /// <summary>
        /// Total volume of blood collected today in milliliters
        /// </summary>
        public int TotalVolumeCollectedToday { get; set; }

        /// <summary>
        /// Number of emergency requests today
        /// </summary>
        public int EmergencyRequestsToday { get; set; }
    }

    /// <summary>
    /// DTO for blood donation and request trends
    /// </summary>
    public class TrendsDto
    {
        /// <summary>
        /// Donation counts for the last 7 days
        /// </summary>
        public Dictionary<string, int> DonationTrend { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Request counts for the last 7 days
        /// </summary>
        public Dictionary<string, int> RequestTrend { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Emergency request counts for the last 7 days
        /// </summary>
        public Dictionary<string, int> EmergencyTrend { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Inventory level trends for the last 7 days
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> InventoryTrend { get; set; } = new Dictionary<string, Dictionary<string, int>>();
    }

    /// <summary>
    /// DTO for system-wide statistics
    /// </summary>
    public class SystemStatisticsDto
    {
        /// <summary>
        /// Total number of registered users
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Total number of donors
        /// </summary>
        public int TotalDonors { get; set; }

        /// <summary>
        /// Total number of blood requests
        /// </summary>
        public int TotalBloodRequests { get; set; }

        /// <summary>
        /// Total number of donations
        /// </summary>
        public int TotalDonations { get; set; }

        /// <summary>
        /// Total volume of blood collected in milliliters
        /// </summary>
        public int TotalVolumeCollected { get; set; }

        /// <summary>
        /// Total number of lives saved (estimated)
        /// </summary>
        public int TotalLivesSaved { get; set; }

        /// <summary>
        /// Request fulfillment rate (percentage)
        /// </summary>
        public double RequestFulfillmentRate { get; set; }

        /// <summary>
        /// Average response time for requests in hours
        /// </summary>
        public double AverageResponseTime { get; set; }
    }

    /// <summary>
    /// DTO for user statistics
    /// </summary>
    public class UserStatisticsDto
    {
        /// <summary>
        /// Number of active users by role
        /// </summary>
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Number of new users in the last 30 days
        /// </summary>
        public int NewUsersLast30Days { get; set; }

        /// <summary>
        /// Number of active users in the last 30 days
        /// </summary>
        public int ActiveUsersLast30Days { get; set; }

        /// <summary>
        /// User activity by hour of day
        /// </summary>
        public Dictionary<int, int> UserActivityByHour { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// User activity by day of week
        /// </summary>
        public Dictionary<string, int> UserActivityByDayOfWeek { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// DTO for inventory statistics
    /// </summary>
    public class InventoryStatisticsDto
    {
        /// <summary>
        /// Current inventory levels by blood group and component type
        /// </summary>
        public List<BloodInventorySummaryDto> CurrentInventory { get; set; } = new List<BloodInventorySummaryDto>();

        /// <summary>
        /// Critical inventory items
        /// </summary>
        public List<CriticalInventoryDto> CriticalItems { get; set; } = new List<CriticalInventoryDto>();

        /// <summary>
        /// Number of expiring items in the next 7 days
        /// </summary>
        public int ExpiringItems7Days { get; set; }

        /// <summary>
        /// Number of expiring items in the next 30 days
        /// </summary>
        public int ExpiringItems30Days { get; set; }

        /// <summary>
        /// Waste rate (percentage of expired/discarded items)
        /// </summary>
        public double WasteRate { get; set; }

        /// <summary>
        /// Inventory turnover rate
        /// </summary>
        public double TurnoverRate { get; set; }

        /// <summary>
        /// Average shelf life of inventory items in days
        /// </summary>
        public double AverageShelfLife { get; set; }
    }

    /// <summary>
    /// DTO for donation statistics
    /// </summary>
    public class DonationStatisticsDto
    {
        /// <summary>
        /// Time period for the statistics
        /// </summary>
        public string TimePeriod { get; set; }

        /// <summary>
        /// Start date of the statistics period
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date of the statistics period
        /// </summary>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Total number of donations in the period
        /// </summary>
        public int TotalDonations { get; set; }

        /// <summary>
        /// Total volume collected in milliliters
        /// </summary>
        public int TotalVolumeCollected { get; set; }

        /// <summary>
        /// Number of first-time donors
        /// </summary>
        public int FirstTimeDonors { get; set; }

        /// <summary>
        /// Number of repeat donors
        /// </summary>
        public int RepeatDonors { get; set; }

        /// <summary>
        /// Donation counts by blood group
        /// </summary>
        public Dictionary<string, int> DonationsByBloodGroup { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Donation counts by component type
        /// </summary>
        public Dictionary<string, int> DonationsByComponentType { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Donation counts by location
        /// </summary>
        public Dictionary<string, int> DonationsByLocation { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Donation counts by day
        /// </summary>
        public Dictionary<string, int> DonationsByDay { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// DTO for blood request statistics
    /// </summary>
    public class RequestStatisticsDto
    {
        /// <summary>
        /// Time period for the statistics
        /// </summary>
        public string TimePeriod { get; set; }

        /// <summary>
        /// Start date of the statistics period
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date of the statistics period
        /// </summary>
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Total number of requests in the period
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Number of emergency requests
        /// </summary>
        public int EmergencyRequests { get; set; }

        /// <summary>
        /// Number of fulfilled requests
        /// </summary>
        public int FulfilledRequests { get; set; }

        /// <summary>
        /// Number of pending requests
        /// </summary>
        public int PendingRequests { get; set; }

        /// <summary>
        /// Number of canceled requests
        /// </summary>
        public int CanceledRequests { get; set; }

        /// <summary>
        /// Average response time in hours
        /// </summary>
        public double AverageResponseTime { get; set; }

        /// <summary>
        /// Request fulfillment rate (percentage)
        /// </summary>
        public double FulfillmentRate { get; set; }

        /// <summary>
        /// Request counts by blood group
        /// </summary>
        public Dictionary<string, int> RequestsByBloodGroup { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Request counts by component type
        /// </summary>
        public Dictionary<string, int> RequestsByComponentType { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Request counts by day
        /// </summary>
        public Dictionary<string, int> RequestsByDay { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// DTO for donor statistics
    /// </summary>
    public class DonorStatisticsDto
    {
        /// <summary>
        /// Total number of registered donors
        /// </summary>
        public int TotalDonors { get; set; }

        /// <summary>
        /// Number of active donors (donated in the last 6 months)
        /// </summary>
        public int ActiveDonors { get; set; }

        /// <summary>
        /// Number of first-time donors in the last 30 days
        /// </summary>
        public int NewDonorsLast30Days { get; set; }

        /// <summary>
        /// Number of donors by blood group
        /// </summary>
        public Dictionary<string, int> DonorsByBloodGroup { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Number of eligible donors by blood group
        /// </summary>
        public Dictionary<string, int> EligibleDonorsByBloodGroup { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Number of donors available for emergency donations
        /// </summary>
        public int EmergencyAvailableDonors { get; set; }

        /// <summary>
        /// Average donations per donor
        /// </summary>
        public double AverageDonationsPerDonor { get; set; }

        /// <summary>
        /// Donor retention rate (percentage)
        /// </summary>
        public double DonorRetentionRate { get; set; }
    }

    /// <summary>
    /// DTO for activity logs
    /// </summary>
    public class ActivityLogDto
    {
        /// <summary>
        /// Unique identifier for the activity log
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User who performed the activity
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User's role
        /// </summary>
        public string UserRole { get; set; }

        /// <summary>
        /// Type of activity (e.g., Login, Create, Update, Delete)
        /// </summary>
        public string ActivityType { get; set; }

        /// <summary>
        /// Description of the activity
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Entity type affected (e.g., User, DonorProfile, BloodRequest)
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Entity ID affected
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// IP address of the user
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Timestamp of the activity
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// DTO for performance metrics
    /// </summary>
    public class PerformanceMetricsDto
    {
        /// <summary>
        /// Average response time for blood requests in hours
        /// </summary>
        public double AverageRequestResponseTime { get; set; }

        /// <summary>
        /// Average time to fulfill emergency requests in hours
        /// </summary>
        public double AverageEmergencyResponseTime { get; set; }

        /// <summary>
        /// Donor recruitment rate (new donors per month)
        /// </summary>
        public double DonorRecruitmentRate { get; set; }

        /// <summary>
        /// Donor retention rate (percentage)
        /// </summary>
        public double DonorRetentionRate { get; set; }

        /// <summary>
        /// Inventory turnover rate
        /// </summary>
        public double InventoryTurnoverRate { get; set; }

        /// <summary>
        /// Waste reduction rate (percentage)
        /// </summary>
        public double WasteReductionRate { get; set; }

        /// <summary>
        /// Average appointment wait time in days
        /// </summary>
        public double AverageAppointmentWaitTime { get; set; }
    }

    /// <summary>
    /// DTO for geographic distribution of donors and requests
    /// </summary>
    public class GeographicDistributionDto
    {
        /// <summary>
        /// Number of donors by geographic region
        /// </summary>
        public Dictionary<string, int> DonorsByRegion { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Number of requests by geographic region
        /// </summary>
        public Dictionary<string, int> RequestsByRegion { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Areas with high demand and low donor availability
        /// </summary>
        public List<HighDemandAreaDto> HighDemandAreas { get; set; } = new List<HighDemandAreaDto>();

        /// <summary>
        /// Areas with high donor concentration
        /// </summary>
        public List<HighDonorAreaDto> HighDonorAreas { get; set; } = new List<HighDonorAreaDto>();
    }

    /// <summary>
    /// DTO for areas with high blood demand and low donor availability
    /// </summary>
    public class HighDemandAreaDto
    {
        /// <summary>
        /// Region name
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Number of requests in the region
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Number of donors in the region
        /// </summary>
        public int DonorCount { get; set; }

        /// <summary>
        /// Ratio of requests to donors
        /// </summary>
        public double RequestToDonorRatio { get; set; }

        /// <summary>
        /// Most requested blood group in the region
        /// </summary>
        public string MostRequestedBloodGroup { get; set; }
    }

    /// <summary>
    /// DTO for areas with high donor concentration
    /// </summary>
    public class HighDonorAreaDto
    {
        /// <summary>
        /// Region name
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Number of donors in the region
        /// </summary>
        public int DonorCount { get; set; }

        /// <summary>
        /// Number of active donors in the region
        /// </summary>
        public int ActiveDonorCount { get; set; }

        /// <summary>
        /// Percentage of donors who are active
        /// </summary>
        public double ActiveDonorPercentage { get; set; }

        /// <summary>
        /// Most common blood group among donors in the region
        /// </summary>
        public string MostCommonBloodGroup { get; set; }
    }

    /// <summary>
    /// DTO for member activity statistics
    /// </summary>
    public class MemberActivityDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Recent donation history
        /// </summary>
        public List<DonationEventDto> RecentDonations { get; set; } = new List<DonationEventDto>();

        /// <summary>
        /// Upcoming appointments
        /// </summary>
        public List<DonationAppointmentRequestDto> UpcomingAppointments { get; set; } = new List<DonationAppointmentRequestDto>();

        /// <summary>
        /// Blood requests made by the member
        /// </summary>
        public List<BloodRequestDto> BloodRequests { get; set; } = new List<BloodRequestDto>();

        /// <summary>
        /// Recent notifications
        /// </summary>
        public List<NotificationDto> RecentNotifications { get; set; } = new List<NotificationDto>();

        /// <summary>
        /// Eligibility status
        /// </summary>
        public bool IsEligibleToDonate { get; set; }

        /// <summary>
        /// Next eligible donation date
        /// </summary>
        public DateTimeOffset? NextEligibleDonationDate { get; set; }
    }

    /// <summary>
    /// DTO for specifying the timeframe for statistics
    /// </summary>
    public class StatisticsTimeframeDto
    {
        /// <summary>
        /// Start date for the statistics period
        /// </summary>
        [Required]
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// End date for the statistics period
        /// </summary>
        [Required]
        public DateTimeOffset EndDate { get; set; }

        /// <summary>
        /// Optional grouping interval (Day, Week, Month)
        /// </summary>
        public string Interval { get; set; } = "Day";
    }
}