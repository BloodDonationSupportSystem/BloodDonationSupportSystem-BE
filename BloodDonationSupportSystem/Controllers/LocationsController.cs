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
    public class LocationsController : BaseApiController
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        // GET: api/Locations
        [HttpGet]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem thông tin các ??a ?i?m
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetLocations()
        {
            var response = await _locationService.GetAllLocationsAsync();
            return HandleResponse(response);
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem thông tin chi ti?t ??a ?i?m
        [ProducesResponseType(typeof(ApiResponse<LocationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetLocation(Guid id)
        {
            var response = await _locationService.GetLocationByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/Locations/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem thông tin ??a ?i?m theo user ID
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LocationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetLocationsByUserId(Guid userId)
        {
            var response = await _locationService.GetLocationsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // POST: api/Locations
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n t?o ??a ?i?m m?i
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
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n c?p nh?t thông tin ??a ?i?m
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
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa ??a ?i?m
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