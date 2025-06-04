using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/Notifications
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNotifications()
        {
            var response = await _notificationService.GetAllNotificationsAsync();
            return HandleResponse(response);
        }

        // GET: api/Notifications/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNotifications([FromQuery] NotificationParameters parameters)
        {
            var response = await _notificationService.GetPagedNotificationsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNotification(Guid id)
        {
            var response = await _notificationService.GetNotificationByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNotificationsByUserId(Guid userId)
        {
            var response = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/paged?pageNumber=1&pageSize=10
        [HttpGet("user/{userId}/paged")]
        [ProducesResponseType(typeof(PagedApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNotificationsByUserId(Guid userId, [FromQuery] NotificationParameters parameters)
        {
            var response = await _notificationService.GetPagedNotificationsByUserIdAsync(userId, parameters);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/unread
        [HttpGet("user/{userId}/unread")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUnreadNotificationsByUserId(Guid userId)
        {
            var response = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/unread/count
        [HttpGet("user/{userId}/unread/count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUnreadCountByUserId(Guid userId)
        {
            var response = await _notificationService.GetUnreadCountByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // POST: api/Notifications
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostNotification([FromBody] CreateNotificationDto notificationDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<NotificationDto>(ModelState));
            }

            var response = await _notificationService.CreateNotificationAsync(notificationDto);
            return HandleResponse(response);
        }

        // PUT: api/Notifications/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutNotification(Guid id, [FromBody] UpdateNotificationDto notificationDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<NotificationDto>(ModelState));
            }

            var response = await _notificationService.UpdateNotificationAsync(id, notificationDto);
            return HandleResponse(response);
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var response = await _notificationService.DeleteNotificationAsync(id);
            return HandleResponse(response);
        }

        // POST: api/Notifications/user/{userId}/mark-all-read
        [HttpPost("user/{userId}/mark-all-read")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> MarkAllAsReadForUser(Guid userId)
        {
            var response = await _notificationService.MarkAllAsReadForUserAsync(userId);
            return HandleResponse(response);
        }
    }
}