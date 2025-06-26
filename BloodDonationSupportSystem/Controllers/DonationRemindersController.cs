using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Default login required for all endpoints
    public class DonationRemindersController : BaseApiController
    {
        private readonly IDonationReminderService _donationReminderService;

        public DonationRemindersController(IDonationReminderService donationReminderService)
        {
            _donationReminderService = donationReminderService;
        }

        // GET: api/DonationReminders/settings/{donorId}
        [HttpGet("settings/{donorId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // All logged-in users can view reminder settings
        [ProducesResponseType(typeof(ApiResponse<DonorReminderSettingsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetReminderSettings(Guid donorId)
        {
            var response = await _donationReminderService.GetReminderSettingsAsync(donorId);
            return HandleResponse(response);
        }

        // POST: api/DonationReminders/settings
        [HttpPost("settings")]
        [Authorize(Roles = "Admin,Staff,Member")] // All logged-in users can create reminder settings
        [ProducesResponseType(typeof(ApiResponse<DonorReminderSettingsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreateReminderSettings([FromBody] CreateDonorReminderSettingsDto reminderSettingsDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonorReminderSettingsDto>(ModelState));
            }

            var response = await _donationReminderService.CreateReminderSettingsAsync(reminderSettingsDto);
            return HandleResponse(response);
        }

        // PUT: api/DonationReminders/settings
        [HttpPut("settings")]
        [Authorize(Roles = "Admin,Staff,Member")] // All logged-in users can update reminder settings
        [ProducesResponseType(typeof(ApiResponse<DonorReminderSettingsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateReminderSettings([FromBody] UpdateDonorReminderSettingsDto reminderSettingsDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonorReminderSettingsDto>(ModelState));
            }

            var response = await _donationReminderService.UpdateReminderSettingsAsync(reminderSettingsDto);
            return HandleResponse(response);
        }

        // POST: api/DonationReminders/send/{donorId}
        [HttpPost("send/{donorId}")]
        [Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can send reminders manually
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> SendReminder(Guid donorId, [FromQuery] int daysBeforeEligible = 7)
        {
            var response = await _donationReminderService.SendReminderAsync(donorId, daysBeforeEligible);
            return HandleResponse(response);
        }

        // POST: api/DonationReminders/check-and-send
        [HttpPost("check-and-send")]
        [Authorize(Roles = "Admin")] // Only Admin can check and send reminders to all donors
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CheckAndSendReminders([FromQuery] int daysBeforeEligible = 7)
        {
            var response = await _donationReminderService.CheckAndSendRemindersAsync(daysBeforeEligible);
            return HandleResponse(response);
        }
    }
}