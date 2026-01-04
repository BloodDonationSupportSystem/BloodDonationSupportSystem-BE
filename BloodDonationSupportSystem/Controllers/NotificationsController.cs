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
    [Authorize] // M?c ??nh y�u c?u ??ng nh?p cho t?t c? c�c endpoints
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/Notifications
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin v� Staff c� quy?n xem t?t c? th�ng b�o
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
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin v� Staff c� quy?n xem danh s�ch th�ng b�o ph�n trang
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
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i d�ng ?� ??ng nh?p ??u c� th? xem chi ti?t th�ng b�o
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
        [Authorize(Roles = "Admin,Staff,Member")] // All logged-in users can view notifications by userId
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNotificationsByUserId(Guid userId)
        {
            // Members can only view their own notifications
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currentUserIdClaim != null && User.IsInRole("Member"))
            {
                var currentUserId = Guid.Parse(currentUserIdClaim.Value);
                if (currentUserId != userId)
                {
                    return Forbid();
                }
            }

            var response = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/paged?pageNumber=1&pageSize=10
        [HttpGet("user/{userId}/paged")]
        [Authorize(Roles = "Admin,Staff,Member")] // All logged-in users can view paged notifications by userId
        [ProducesResponseType(typeof(PagedApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNotificationsByUserId(Guid userId, [FromQuery] NotificationParameters? parameters = null)
        {
            // Members can only view their own notifications
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currentUserIdClaim != null && User.IsInRole("Member"))
            {
                var currentUserId = Guid.Parse(currentUserIdClaim.Value);
                if (currentUserId != userId)
                {
                    return Forbid();
                }
            }

            // Provide default parameters if null
            parameters ??= new NotificationParameters();

            var response = await _notificationService.GetPagedNotificationsByUserIdAsync(userId, parameters);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/unread
        [HttpGet("user/{userId}/unread")]
        [Authorize(Roles = "Admin,Staff,Member")] // All logged-in users can view unread notifications by userId
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUnreadNotificationsByUserId(Guid userId)
        {
            // Members can only view their own unread notifications
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currentUserIdClaim != null && User.IsInRole("Member"))
            {
                var currentUserId = Guid.Parse(currentUserIdClaim.Value);
                if (currentUserId != userId)
                {
                    return Forbid();
                }
            }

            var response = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/Notifications/user/{userId}/unread/count
        [HttpGet("user/{userId}/unread/count")]
        [Authorize(Roles = "Admin,Staff,Member")] // All logged-in users can view unread count by userId
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUnreadCountByUserId(Guid userId)
        {
            // Members can only view their own unread count
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currentUserIdClaim != null && User.IsInRole("Member"))
            {
                var currentUserId = Guid.Parse(currentUserIdClaim.Value);
                if (currentUserId != userId)
                {
                    return Forbid();
                }
            }

            var response = await _notificationService.GetUnreadCountByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // POST: api/Notifications
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin v� Staff c� quy?n t?o th�ng b�o m?i
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
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin v� Staff c� quy?n c?p nh?t th�ng b�o
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

        // POST: api/Notifications/{id}/mark-read
        [HttpPost("{id}/mark-read")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i d�ng ?� ??ng nh?p ??u c� th? ?�nh d?u th�ng b�o l� ?� ??c
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> MarkNotificationAsRead(Guid id)
        {
            var response = await _notificationService.MarkNotificationAsReadAsync(id);
            return HandleResponse(response);
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Ch? Admin m?i c� quy?n x�a th�ng b�o
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
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i d�ng ?� ??ng nh?p ??u c� th? ?�nh d?u ?� ??c t?t c? th�ng b�o
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