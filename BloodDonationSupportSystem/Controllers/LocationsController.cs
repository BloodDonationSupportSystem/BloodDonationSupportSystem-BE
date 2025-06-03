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
    public class LocationsController : BaseApiController
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        // GET: api/Locations
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetLocations()
        {
            var response = await _locationService.GetAllLocationsAsync();
            return HandleResponse(response);
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<LocationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetLocation(Guid id)
        {
            var response = await _locationService.GetLocationByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/Locations
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LocationDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostLocation([FromBody] CreateLocationDto locationDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<LocationDto>(ModelState));
            }

            var response = await _locationService.CreateLocationAsync(locationDto);
            return HandleResponse(response);
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<LocationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutLocation(Guid id, [FromBody] UpdateLocationDto locationDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<LocationDto>(ModelState));
            }

            var response = await _locationService.UpdateLocationAsync(id, locationDto);
            return HandleResponse(response);
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteLocation(Guid id)
        {
            var response = await _locationService.DeleteLocationAsync(id);
            return HandleResponse(response);
        }
    }
}