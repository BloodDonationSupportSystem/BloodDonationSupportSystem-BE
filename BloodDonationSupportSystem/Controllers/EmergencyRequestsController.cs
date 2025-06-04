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
    public class EmergencyRequestsController : BaseApiController
    {
        private readonly IEmergencyRequestService _emergencyRequestService;

        public EmergencyRequestsController(IEmergencyRequestService emergencyRequestService)
        {
            _emergencyRequestService = emergencyRequestService;
        }

        // GET: api/EmergencyRequests
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetEmergencyRequests([FromQuery] EmergencyRequestParameters parameters)
        {
            var response = await _emergencyRequestService.GetPagedEmergencyRequestsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/EmergencyRequests/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetEmergencyRequest(Guid id)
        {
            var response = await _emergencyRequestService.GetEmergencyRequestByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/EmergencyRequests
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostEmergencyRequest([FromBody] CreateEmergencyRequestDto emergencyRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<EmergencyRequestDto>(ModelState));
            }

            var response = await _emergencyRequestService.CreateEmergencyRequestAsync(emergencyRequestDto);
            return HandleResponse(response);
        }

        // PUT: api/EmergencyRequests/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutEmergencyRequest(Guid id, [FromBody] UpdateEmergencyRequestDto emergencyRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<EmergencyRequestDto>(ModelState));
            }

            var response = await _emergencyRequestService.UpdateEmergencyRequestAsync(id, emergencyRequestDto);
            return HandleResponse(response);
        }

        // DELETE: api/EmergencyRequests/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteEmergencyRequest(Guid id)
        {
            var response = await _emergencyRequestService.DeleteEmergencyRequestAsync(id);
            return HandleResponse(response);
        }
    }
}