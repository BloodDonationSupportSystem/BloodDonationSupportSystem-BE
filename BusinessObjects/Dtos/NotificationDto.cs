using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

    public class CreateNotificationDto
    {
        [Required(ErrorMessage = "Notification type is required")]
        [StringLength(50, ErrorMessage = "Type cannot be longer than 50 characters")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters")]
        public string Message { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }
    }

    public class UpdateNotificationDto
    {
        public bool IsRead { get; set; }
    }

    public class NotificationParameters : PaginationParameters
    {
        public string Type { get; set; }
        public bool? IsRead { get; set; }
    }
}