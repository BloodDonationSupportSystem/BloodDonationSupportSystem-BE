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
    public class DonationEventsController : BaseApiController
    {
        private readonly IDonationEventService _donationEventService;

        public DonationEventsController(IDonationEventService donationEventService)
        {
            _donationEventService = donationEventService;
        }

        // GET: api/DonationEvents
        [HttpGet]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem danh sách s? ki?n hi?n máu
        [ProducesResponseType(typeof(PagedApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEvents([FromQuery] DonationEventParameters parameters)
        {
            var response = await _donationEventService.GetPagedDonationEventsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/DonationEvents/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem chi ti?t s? ki?n hi?n máu
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEvent(Guid id)
        {
            var response = await _donationEventService.GetDonationEventByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n t?o s? ki?n hi?n máu m?i
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostDonationEvent([FromBody] CreateDonationEventDto donationEventDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.CreateDonationEventAsync(donationEventDto);
            return HandleResponse(response);
        }

        // PUT: api/DonationEvents/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n c?p nh?t s? ki?n hi?n máu
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutDonationEvent(Guid id, [FromBody] UpdateDonationEventDto donationEventDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.UpdateDonationEventAsync(id, donationEventDto);
            return HandleResponse(response);
        }

        // DELETE: api/DonationEvents/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa s? ki?n hi?n máu
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteDonationEvent(Guid id)
        {
            var response = await _donationEventService.DeleteDonationEventAsync(id);
            return HandleResponse(response);
        }
    }
}