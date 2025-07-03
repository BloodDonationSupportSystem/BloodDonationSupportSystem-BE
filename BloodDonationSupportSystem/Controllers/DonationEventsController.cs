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
    [Authorize]
    public class DonationEventsController : BaseApiController
    {
        private readonly IDonationEventService _donationEventService;
        private readonly IRealTimeNotificationService _realTimeNotificationService;

        public DonationEventsController(
            IDonationEventService donationEventService,
            IRealTimeNotificationService realTimeNotificationService)
        {
            _donationEventService = donationEventService;
            _realTimeNotificationService = realTimeNotificationService;
        }

        // GET: api/DonationEvents
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(PagedApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEvents([FromQuery] DonationEventParameters parameters)
        {
            var response = await _donationEventService.GetPagedDonationEventsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/DonationEvents/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Member")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEvent(Guid id)
        {
            var response = await _donationEventService.GetDonationEventByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/DonationEvents/request/{requestId}
        [HttpGet("request/{requestId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationEventDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEventsByRequest(Guid requestId, [FromQuery] string requestType)
        {
            var response = await _donationEventService.GetDonationEventsByRequestAsync(requestId, requestType);
            return HandleResponse(response);
        }

        // GET: api/DonationEvents/donor/{donorId}
        [HttpGet("donor/{donorId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationEventDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEventsByDonor(Guid donorId)
        {
            var response = await _donationEventService.GetDonationEventsByDonorAsync(donorId);
            return HandleResponse(response);
        }

        // GET: api/DonationEvents/appointment/{appointmentId}
        [HttpGet("appointment/{appointmentId}")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationEventByAppointment(Guid appointmentId)
        {
            var response = await _donationEventService.GetDonationEventByAppointmentIdAsync(appointmentId);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreateDonationEvent([FromBody] CreateDonationEventDto donationEventDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.CreateDonationEventAsync(donationEventDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/walk-in
        [HttpPost("walk-in")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreateWalkInDonationEvent([FromBody] CreateWalkInDonationEventDto walkInDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.CreateWalkInDonationEventAsync(walkInDto);
            return HandleResponse(response);
        }

        // PUT: api/DonationEvents/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateDonationEvent(Guid id, [FromBody] UpdateDonationEventDto donationEventDto)
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
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteDonationEvent(Guid id)
        {
            var response = await _donationEventService.DeleteDonationEventAsync(id);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/{id}/check-in
        [HttpPost("{id}/check-in")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CheckInAppointment(Guid id, [FromBody] CheckInAppointmentDto checkInDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            checkInDto.AppointmentId = id;

            var response = await _donationEventService.CheckInAppointmentAsync(checkInDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/{id}/health-check
        [HttpPost("{id}/health-check")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PerformHealthCheck(Guid id, [FromBody] DonorHealthCheckDto healthCheckDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            healthCheckDto.DonationEventId = id;

            var response = await _donationEventService.PerformHealthCheckAsync(healthCheckDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/{id}/start-donation
        [HttpPost("{id}/start-donation")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> StartDonation(Guid id, [FromBody] StartDonationDto startDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            startDto.DonationEventId = id;

            var response = await _donationEventService.StartDonationProcessAsync(startDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/{id}/complication
        [HttpPost("{id}/complication")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RecordComplication(Guid id, [FromBody] DonationComplicationDto complicationDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            complicationDto.DonationEventId = id;

            var response = await _donationEventService.RecordDonationComplicationAsync(complicationDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/{id}/complete
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CompleteDonation(Guid id, [FromBody] CompleteDonationDto completionDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            completionDto.DonationEventId = id;

            var response = await _donationEventService.CompleteDonationAsync(completionDto);
            return HandleResponse(response);
        }
    }
}