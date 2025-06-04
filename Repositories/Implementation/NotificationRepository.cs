using BusinessObjects.Data;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Notification> GetByIdWithUserAsync(Guid id)
        {
            return await _dbSet
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(n => n.User)
                .Where(n => n.UserId == userId && n.DeletedTime == null)
                .OrderByDescending(n => n.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(n => n.User)
                .Where(n => n.UserId == userId && !n.IsRead && n.DeletedTime == null)
                .OrderByDescending(n => n.CreatedTime)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead && n.DeletedTime == null)
                .CountAsync();
        }

        public async Task<(IEnumerable<Notification> notifications, int totalCount)> GetPagedNotificationsAsync(NotificationParameters parameters)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Type))
            {
                query = query.Where(n => n.Type == parameters.Type);
            }

            if (parameters.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == parameters.IsRead.Value);
            }

            // Filter soft deleted
            query = query.Where(n => n.DeletedTime == null);

            // Get total count
            var totalCount = await query.CountAsync();

            // Include related entities
            query = query.Include(n => n.User);

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "type" => parameters.SortAscending 
                    ? query.OrderBy(n => n.Type) 
                    : query.OrderByDescending(n => n.Type),
                "read" => parameters.SortAscending 
                    ? query.OrderBy(n => n.IsRead) 
                    : query.OrderByDescending(n => n.IsRead),
                "user" => parameters.SortAscending 
                    ? query.OrderBy(n => n.User.FirstName).ThenBy(n => n.User.LastName) 
                    : query.OrderByDescending(n => n.User.FirstName).ThenByDescending(n => n.User.LastName),
                _ => parameters.SortAscending 
                    ? query.OrderBy(n => n.CreatedTime) 
                    : query.OrderByDescending(n => n.CreatedTime)
            };

            // Apply pagination
            var notifications = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public async Task<(IEnumerable<Notification> notifications, int totalCount)> GetPagedNotificationsByUserIdAsync(Guid userId, NotificationParameters parameters)
        {
            var query = _dbSet.AsQueryable();

            // Apply user filter
            query = query.Where(n => n.UserId == userId);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Type))
            {
                query = query.Where(n => n.Type == parameters.Type);
            }

            if (parameters.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == parameters.IsRead.Value);
            }

            // Filter soft deleted
            query = query.Where(n => n.DeletedTime == null);

            // Get total count
            var totalCount = await query.CountAsync();

            // Include related entities
            query = query.Include(n => n.User);

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "type" => parameters.SortAscending 
                    ? query.OrderBy(n => n.Type) 
                    : query.OrderByDescending(n => n.Type),
                "read" => parameters.SortAscending 
                    ? query.OrderBy(n => n.IsRead) 
                    : query.OrderByDescending(n => n.IsRead),
                _ => parameters.SortAscending 
                    ? query.OrderBy(n => n.CreatedTime) 
                    : query.OrderByDescending(n => n.CreatedTime)
            };

            // Apply pagination
            var notifications = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            var unreadNotifications = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead && n.DeletedTime == null)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.LastUpdatedTime = DateTimeOffset.Now;
            }
        }
    }
}