using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Request ID is required")]
        public Guid RequestId { get; set; }

        [Required(ErrorMessage = "Emergency request ID is required")]
        public Guid EmergencyRequestId { get; set; }

        [Required(ErrorMessage = "Donation event ID is required")]
        public Guid DonationEventId { get; set; }

        [Required(ErrorMessage = "Units assigned is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Units assigned must be greater than 0")]
        public int UnitsAssigned { get; set; }
    }

    public class UpdateRequestMatchDto
    {
        [Required(ErrorMessage = "Units assigned is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Units assigned must be greater than 0")]
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