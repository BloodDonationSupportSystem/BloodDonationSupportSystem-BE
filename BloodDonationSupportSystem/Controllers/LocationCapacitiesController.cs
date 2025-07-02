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
    [Route("api/locations/{locationId}/capacities")]
    [ApiController]
    public class LocationCapacitiesController : BaseApiController
    {
        private readonly ILocationCapacityService _locationCapacityService;

        public LocationCapacitiesController(ILocationCapacityService locationCapacityService)
        {
            _locationCapacityService = locationCapacityService;
        }

        // GET: api/locations/{locationId}/capacities
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationCapacityDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetLocationCapacities(Guid locationId)
        {
            var response = await _locationCapacityService.GetByLocationIdAsync(locationId);
            return HandleResponse(response);
        }

        // GET: api/locations/{locationId}/capacities/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LocationCapacityDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetLocationCapacity(Guid locationId, Guid id)
        {
            var response = await _locationCapacityService.GetByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/locations/{locationId}/capacities
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<LocationCapacityDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostLocationCapacity(Guid locationId, [FromBody] CreateLocationCapacityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<LocationCapacityDto>(ModelState));
            }
            dto.LocationId = locationId;
            var response = await _locationCapacityService.CreateAsync(dto);
            return HandleResponse(response);
        }

        // PUT: api/locations/{locationId}/capacities/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<LocationCapacityDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutLocationCapacity(Guid locationId, Guid id, [FromBody] UpdateLocationCapacityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<LocationCapacityDto>(ModelState));
            }
            var response = await _locationCapacityService.UpdateAsync(id, dto);
            return HandleResponse(response);
        }

        [HttpGet("by-location-date")]
        [ProducesResponseType(typeof(ApiResponse<LocationCapacityDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetByLocationAndDate(Guid locationId, [FromQuery] DateTimeOffset date)
        {
            var response = await _locationCapacityService.GetByLocationAndDateAsync(locationId, date);
            return HandleResponse(response);
        }

        // POST: api/locations/{locationId}/capacities/bulk
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationCapacityDto>>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreateMultipleCapacities(Guid locationId, [FromBody] CreateMultipleLocationCapacityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<IEnumerable<LocationCapacityDto>>(ModelState));
            }
            
            // Ensure the locationId from route matches the DTO
            dto.LocationId = locationId;
            
            var response = await _locationCapacityService.CreateMultipleAsync(dto);
            return HandleResponse(response);
        }

        // DELETE: api/locations/{locationId}/capacities/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteLocationCapacity(Guid locationId, Guid id)
        {
            var response = await _locationCapacityService.DeleteAsync(id);
            return HandleResponse(response);
        }
    }
}