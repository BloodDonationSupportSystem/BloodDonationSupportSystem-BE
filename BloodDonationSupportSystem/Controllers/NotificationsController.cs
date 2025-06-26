using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/Notifications
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem t?t c? thông báo
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNotifications()
        {
            var response = await _notificationService.GetAllNotificationsAsync();
            return HandleResponse(response);
        }

        // GET: api/Notifications/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem danh sách thông báo phân trang
        [ProducesResponseType(typeof(PagedApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNotifications([FromQuery] NotificationParameters parameters)
        {
            var response = await _notificationService.GetPagedNotificationsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem chi ti?t thông báo
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNotification(Guid id)
        {
            var response = await _notificationService.GetNotificationByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem thông báo theo userId
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNotificationsByUserId(Guid userId)
        {
            var response = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/paged?pageNumber=1&pageSize=10
        [HttpGet("user/{userId}/paged")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem thông báo phân trang theo userId
        [ProducesResponseType(typeof(PagedApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNotificationsByUserId(Guid userId, [FromQuery] NotificationParameters? parameters = null)
        {
            // Provide default parameters if null
            parameters ??= new NotificationParameters();

            var response = await _notificationService.GetPagedNotificationsByUserIdAsync(userId, parameters);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/unread
        [HttpGet("user/{userId}/unread")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem thông báo ch?a ??c theo userId
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUnreadNotificationsByUserId(Guid userId)
        {
            var response = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/unread/count
        [HttpGet("user/{userId}/unread/count")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem s? l??ng thông báo ch?a ??c
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUnreadCountByUserId(Guid userId)
        {
            var response = await _notificationService.GetUnreadCountByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // POST: api/Notifications
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n t?o thông báo m?i
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
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
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n c?p nh?t thông báo
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
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
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa thông báo
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var response = await _notificationService.DeleteNotificationAsync(id);
            return HandleResponse(response);
        }

        // POST: api/Notifications/user/{userId}/mark-all-read
        [HttpPost("user/{userId}/mark-all-read")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? ?ánh d?u ?ã ??c t?t c? thông báo
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> MarkAllAsReadForUser(Guid userId)
        {
            var response = await _notificationService.MarkAllAsReadForUserAsync(userId);
            return HandleResponse(response);
        }
    }
}