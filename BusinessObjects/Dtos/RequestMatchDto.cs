using System;
using Shared.Models;

namespace BusinessObjects.Dtos
{
    public class RequestMatchDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset MatchDate { get; set; }
        public int UnitsAssigned { get; set; }
        public Guid RequestId { get; set; }
        public string RequestInfo { get; set; }
        public Guid EmergencyRequestId { get; set; }
        public string EmergencyRequestInfo { get; set; }
        public Guid DonationEventId { get; set; }
        public string DonationEventInfo { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

    public class CreateRequestMatchDto
    {
        public Guid RequestId { get; set; }
        public Guid EmergencyRequestId { get; set; }
        public Guid DonationEventId { get; set; }
        public int UnitsAssigned { get; set; }
    }

    public class UpdateRequestMatchDto
    {
        public int UnitsAssigned { get; set; }
    }
    
    public class RequestMatchParameters : PaginationParameters
    {
        public Guid? RequestId { get; set; }
        public Guid? EmergencyRequestId { get; set; }
        public Guid? DonationEventId { get; set; }
        public DateTimeOffset? MatchDateFrom { get; set; }
        public DateTimeOffset? MatchDateTo { get; set; }
    }
}