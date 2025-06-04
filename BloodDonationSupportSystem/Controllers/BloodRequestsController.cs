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
    public class BloodRequestsController : BaseApiController
    {
        private readonly IBloodRequestService _bloodRequestService;

        public BloodRequestsController(IBloodRequestService bloodRequestService)
        {
            _bloodRequestService = bloodRequestService;
        }

        // GET: api/BloodRequests
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodRequests([FromQuery] BloodRequestParameters parameters)
        {
            var response = await _bloodRequestService.GetPagedBloodRequestsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/BloodRequests/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodRequest(Guid id)
        {
            var response = await _bloodRequestService.GetBloodRequestByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/BloodRequests
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostBloodRequest([FromBody] CreateBloodRequestDto bloodRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodRequestDto>(ModelState));
            }

            var response = await _bloodRequestService.CreateBloodRequestAsync(bloodRequestDto);
            return HandleResponse(response);
        }

        // PUT: api/BloodRequests/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutBloodRequest(Guid id, [FromBody] UpdateBloodRequestDto bloodRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodRequestDto>(ModelState));
            }

            var response = await _bloodRequestService.UpdateBloodRequestAsync(id, bloodRequestDto);
            return HandleResponse(response);
        }

        // DELETE: api/BloodRequests/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteBloodRequest(Guid id)
        {
            var response = await _bloodRequestService.DeleteBloodRequestAsync(id);
            return HandleResponse(response);
        }
    }
}