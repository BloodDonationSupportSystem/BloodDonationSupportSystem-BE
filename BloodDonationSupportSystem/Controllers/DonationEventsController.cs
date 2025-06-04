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
    public class DonationEventsController : BaseApiController
    {
        private readonly IDonationEventService _donationEventService;

        public DonationEventsController(IDonationEventService donationEventService)
        {
            _donationEventService = donationEventService;
        }

        // GET: api/DonationEvents
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEvents([FromQuery] DonationEventParameters parameters)
        {
            var response = await _donationEventService.GetPagedDonationEventsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/DonationEvents/5
        [HttpGet("{id}")]
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
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
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
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
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
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteDonationEvent(Guid id)
        {
            var response = await _donationEventService.DeleteDonationEventAsync(id);
            return HandleResponse(response);
        }
    }
}