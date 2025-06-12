using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class DonorProfilesController : BaseApiController
    {
        private readonly IDonorProfileService _donorProfileService;

        public DonorProfilesController(IDonorProfileService donorProfileService)
        {
            _donorProfileService = donorProfileService;
        }

        // GET: api/DonorProfiles
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có th? xem t?t c? h? s? ng??i hi?n máu (phân trang)
        [ProducesResponseType(typeof(PagedApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfiles([FromQuery] DonorProfileParameters parameters)
        {
            var response = await _donorProfileService.GetPagedDonorProfilesAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có th? xem t?t c? h? s? ng??i hi?n máu
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAllDonorProfiles()
        {
            var response = await _donorProfileService.GetAllDonorProfilesAsync();
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem thông tin chi ti?t m?t h? s?
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfile(Guid id)
        {
            var response = await _donorProfileService.GetDonorProfileByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/user/5
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem h? s? theo userId
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfileByUserId(Guid userId)
        {
            var response = await _donorProfileService.GetDonorProfileByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/bloodgroup/5
        [HttpGet("bloodgroup/{bloodGroupId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? tìm ki?m theo nhóm máu
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfilesByBloodGroupId(Guid bloodGroupId)
        {
            var response = await _donorProfileService.GetDonorProfilesByBloodGroupIdAsync(bloodGroupId);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/available
        [HttpGet("available")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? tìm ki?m ng??i hi?n máu kh? d?ng
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAvailableDonors([FromQuery] DateTimeOffset? date = null, [FromQuery] bool? forEmergency = null)
        {
            var response = await _donorProfileService.GetAvailableDonorsAsync(date, forEmergency);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/nearby
        [HttpGet("nearby")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? tìm ki?m ng??i hi?n máu g?n ?ó
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNearbyDonors(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double radiusKm = 10.0, 
            [FromQuery] Guid? bloodGroupId = null)
        {
            if (radiusKm <= 0 || radiusKm > 500)
            {
                return HandleResponse(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    "Radius must be between 0.1 and 500 kilometers"));
            }

            var response = await _donorProfileService.GetDonorsByDistanceAsync(latitude, longitude, radiusKm, bloodGroupId);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/nearby/available
        [HttpGet("nearby/available")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? tìm ki?m ng??i hi?n máu kh? d?ng g?n ?ó
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNearbyAvailableDonors([FromQuery] NearbyDonorSearchDto searchDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<IEnumerable<DonorProfileDto>>(ModelState));
            }

            var response = await _donorProfileService.GetAvailableDonorsByDistanceAsync(searchDto);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/nearby/paged
        [HttpGet("nearby/paged")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có th? xem t?t c? h? s? ng??i hi?n máu g?n ?ó (phân trang)
        [ProducesResponseType(typeof(PagedApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNearbyDonors(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double radiusKm = 10.0, 
            [FromQuery] DonorProfileParameters parameters = null)
        {
            if (radiusKm <= 0 || radiusKm > 500)
            {
                return HandleResponse(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    "Radius must be between 0.1 and 500 kilometers"));
            }

            parameters ??= new DonorProfileParameters();
            var response = await _donorProfileService.GetPagedDonorsByDistanceAsync(latitude, longitude, radiusKm, parameters);
            return HandleResponse(response);
        }

        // POST: api/DonorProfiles
        [HttpPost]
        [Authorize(Roles = "Member")] // Ch? Member m?i ???c t?o h? s? hi?n máu (cho chính h?)
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostDonorProfile([FromBody] CreateDonorProfileDto donorProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonorProfileDto>(ModelState));
            }

            var response = await _donorProfileService.CreateDonorProfileAsync(donorProfileDto);
            return HandleResponse(response);
        }

        // PUT: api/DonorProfiles/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Member,Staff,Admin")] // Ng??i dùng có th? c?p nh?t h? s? c?a h?, Staff và Admin có th? c?p nh?t b?t k? h? s? nào
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutDonorProfile(Guid id, [FromBody] UpdateDonorProfileDto donorProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonorProfileDto>(ModelState));
            }

            var response = await _donorProfileService.UpdateDonorProfileAsync(id, donorProfileDto);
            return HandleResponse(response);
        }

        // PUT: api/DonorProfiles/availability
        [HttpPut("availability")]
        [Authorize(Roles = "Member")] // Ch? Member có th? c?p nh?t thông tin s?n sàng hi?n máu c?a h?
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateDonationAvailability([FromBody] UpdateDonationAvailabilityDto availabilityDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonorProfileDto>(ModelState));
            }

            var response = await _donorProfileService.UpdateDonationAvailabilityAsync(availabilityDto);
            return HandleResponse(response);
        }

        // DELETE: api/DonorProfiles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa h? s?
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteDonorProfile(Guid id)
        {
            var response = await _donorProfileService.DeleteDonorProfileAsync(id);
            return HandleResponse(response);
        }
    }
}