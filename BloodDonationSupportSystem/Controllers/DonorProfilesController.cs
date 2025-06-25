using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Security.Claims;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Mặc định yêu cầu đăng nhập cho tất cả các endpoints
    public class DonorProfilesController : BaseApiController
    {
        private readonly IDonorProfileService _donorProfileService;

        public DonorProfilesController(IDonorProfileService donorProfileService)
        {
            _donorProfileService = donorProfileService;
        }

        // GET: api/DonorProfiles/current/exists
        [HttpGet("current/exists")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> HasDonorProfile()
        {
            try
            {
                // Get the current user's ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return HandleResponse(new ApiResponse<bool>(
                        HttpStatusCode.Unauthorized,
                        "User is not properly authenticated."));
                }

                var userId = Guid.Parse(userIdClaim.Value);

                // Check if the user has a donor profile
                var response = await _donorProfileService.GetDonorProfileByUserIdAsync(userId);
                
                // Return true if the profile exists, false if not found
                bool hasProfile = response.Success && response.Data != null;
                
                return HandleResponse(new ApiResponse<bool>(hasProfile));
            }
            catch (Exception ex)
            {
                return HandleResponse(new ApiResponse<bool>(
                    HttpStatusCode.InternalServerError,
                    $"Error checking donor profile existence: {ex.Message}"));
            }
        }

        // GET: api/DonorProfiles
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff có thể xem tất cả hồ sơ người hiến máu (phân trang)
        [ProducesResponseType(typeof(PagedApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfiles([FromQuery] DonorProfileParameters parameters)
        {
            var response = await _donorProfileService.GetPagedDonorProfilesAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff có thể xem tất cả hồ sơ người hiến máu
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAllDonorProfiles()
        {
            var response = await _donorProfileService.GetAllDonorProfilesAsync();
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Member")] // Tất cả người dùng đã đăng nhập đều có thể xem thông tin chi tiết một hồ sơ
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
        [Authorize(Roles = "Admin,Staff,Member")] // Tất cả người dùng đã đăng nhập đều có thể xem hồ sơ theo userId
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
        [Authorize(Roles = "Admin,Staff,Member")] // Tất cả người dùng đã đăng nhập đều có thể tìm kiếm theo nhóm máu
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
        [Authorize(Roles = "Admin,Staff,Member")] // Tất cả người dùng đã đăng nhập đều có thể tìm kiếm người hiến máu khả dụng
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAvailableDonors([FromQuery] DateTimeOffset? date = null, [FromQuery] bool? forEmergency = null)
        {
            var response = await _donorProfileService.GetAvailableDonorsAsync(date, forEmergency);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/nearby
        [HttpGet("nearby")]
        [Authorize(Roles = "Admin,Staff,Member")] // Tất cả người dùng đã đăng nhập đều có thể tìm kiếm người hiến máu gần đó
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
        [Authorize(Roles = "Admin,Staff,Member")] // Tất cả người dùng đã đăng nhập đều có thể tìm kiếm người hiến máu khả dụng gần đó
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
        [Authorize(Roles = "Admin,Staff")] // Chỉ Admin và Staff có thể xem tất cả hồ sơ người hiến máu gần đó (phân trang)
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
        [Authorize(Roles = "Member")] // Chỉ Member mới được tạo hồ sơ hiến máu (cho chính họ)
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
        [Authorize(Roles = "Member,Staff,Admin")] // Người dùng có thể cập nhật hồ sơ của họ, Staff và Admin có thể cập nhật bất kỳ hồ sơ nào
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
        [Authorize(Roles = "Member")] // Chỉ Member có thể cập nhật thông tin sẵn sàng hiến máu của họ
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
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền xóa hồ sơ
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