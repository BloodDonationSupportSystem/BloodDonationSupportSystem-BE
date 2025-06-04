using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<UserDto>(ModelState));
            }

            // Register as Member (default role) - using exact case from database
            var response = await _userService.RegisterUserAsync(registerDto, "Member");
            return HandleResponse(response);
        }

        // POST: api/Auth/register-staff
        [HttpPost("register-staff")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterUserDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<UserDto>(ModelState));
            }

            // Register as Staff - using exact case from database
            var response = await _userService.RegisterUserAsync(registerDto, "Staff");
            return HandleResponse(response);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<TokenResponseDto>(ModelState));
            }

            var response = await _userService.AuthenticateAsync(loginDto);
            return HandleResponse(response);
        }

        // POST: api/Auth/refresh-token
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new ApiResponse(System.Net.HttpStatusCode.BadRequest, "Access token and refresh token are required"));
            }

            var response = await _userService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
            return HandleResponse(response);
        }

        // POST: api/Auth/revoke-token
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new ApiResponse(System.Net.HttpStatusCode.BadRequest, "Refresh token is required"));
            }

            var response = await _userService.RevokeTokenAsync(request.RefreshToken);
            return HandleResponse(response);
        }

        // POST: api/Auth/reset-password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<object>(ModelState));
            }

            var response = await _userService.ResetPasswordAsync(resetPasswordDto);
            return HandleResponse(response);
        }
    }
}