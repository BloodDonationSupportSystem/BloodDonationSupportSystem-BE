using System;
using Shared.Models;

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
        public string Type { get; set; }
        public string Message { get; set; }
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