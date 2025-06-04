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
    public class RequestMatchesController : BaseApiController
    {
        private readonly IRequestMatchService _requestMatchService;

        public RequestMatchesController(IRequestMatchService requestMatchService)
        {
            _requestMatchService = requestMatchService;
        }

        // GET: api/RequestMatches
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RequestMatchDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestMatches()
        {
            var response = await _requestMatchService.GetAllRequestMatchesAsync();
            return HandleResponse(response);
        }

        // GET: api/RequestMatches/paged?pageNumber=1&pageSize=10
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedApiResponse<RequestMatchDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedRequestMatches([FromQuery] RequestMatchParameters parameters)
        {
            var response = await _requestMatchService.GetPagedRequestMatchesAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/RequestMatches/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RequestMatchDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestMatch(Guid id)
        {
            var response = await _requestMatchService.GetRequestMatchByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/RequestMatches/request/{requestId}
        [HttpGet("request/{requestId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RequestMatchDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestMatchesByRequestId(Guid requestId)
        {
            var response = await _requestMatchService.GetRequestMatchesByRequestIdAsync(requestId);
            return HandleResponse(response);
        }

        // GET: api/RequestMatches/emergency-request/{emergencyRequestId}
        [HttpGet("emergency-request/{emergencyRequestId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RequestMatchDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestMatchesByEmergencyRequestId(Guid emergencyRequestId)
        {
            var response = await _requestMatchService.GetRequestMatchesByEmergencyRequestIdAsync(emergencyRequestId);
            return HandleResponse(response);
        }

        // GET: api/RequestMatches/donation-event/{donationEventId}
        [HttpGet("donation-event/{donationEventId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RequestMatchDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestMatchesByDonationEventId(Guid donationEventId)
        {
            var response = await _requestMatchService.GetRequestMatchesByDonationEventIdAsync(donationEventId);
            return HandleResponse(response);
        }

        // POST: api/RequestMatches
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RequestMatchDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostRequestMatch([FromBody] CreateRequestMatchDto requestMatchDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<RequestMatchDto>(ModelState));
            }

            var response = await _requestMatchService.CreateRequestMatchAsync(requestMatchDto);
            return HandleResponse(response);
        }

        // PUT: api/RequestMatches/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RequestMatchDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutRequestMatch(Guid id, [FromBody] UpdateRequestMatchDto requestMatchDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<RequestMatchDto>(ModelState));
            }

            var response = await _requestMatchService.UpdateRequestMatchAsync(id, requestMatchDto);
            return HandleResponse(response);
        }

        // DELETE: api/RequestMatches/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteRequestMatch(Guid id)
        {
            var response = await _requestMatchService.DeleteRequestMatchAsync(id);
            return HandleResponse(response);
        }
    }
}