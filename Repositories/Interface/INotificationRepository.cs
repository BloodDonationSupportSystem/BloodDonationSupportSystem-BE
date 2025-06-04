using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<Notification> GetByIdWithUserAsync(Guid id);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task<int> GetUnreadCountByUserIdAsync(Guid userId);
        Task<(IEnumerable<Notification> notifications, int totalCount)> GetPagedNotificationsAsync(NotificationParameters parameters);
        Task<(IEnumerable<Notification> notifications, int totalCount)> GetPagedNotificationsByUserIdAsync(Guid userId, NotificationParameters parameters);
        Task MarkAllAsReadForUserAsync(Guid userId);
    }
}