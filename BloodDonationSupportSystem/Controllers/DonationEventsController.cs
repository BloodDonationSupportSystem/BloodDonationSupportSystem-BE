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
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
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
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
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
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
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
        [Authorize(Roles = "Admin")]
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

        // POST: api/DonationEvents/check-in
        [HttpPost("check-in")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CheckInAppointment([FromBody] CheckInAppointmentDto checkInDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.CheckInAppointmentAsync(checkInDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/health-check
        [HttpPost("health-check")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PerformHealthCheck([FromBody] DonorHealthCheckDto healthCheckDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.PerformHealthCheckAsync(healthCheckDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/start
        [HttpPost("start")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> StartDonationProcess([FromBody] StartDonationDto startDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.StartDonationProcessAsync(startDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/complication
        [HttpPost("complication")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RecordComplication([FromBody] DonationComplicationDto complicationDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.RecordDonationComplicationAsync(complicationDto);
            return HandleResponse(response);
        }

        // POST: api/DonationEvents/complete
        [HttpPost("complete")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationEventDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CompleteDonation([FromBody] CompleteDonationDto completionDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationEventDto>(ModelState));
            }

            var response = await _donationEventService.CompleteDonationAsync(completionDto);
            return HandleResponse(response);
        }
    }
}