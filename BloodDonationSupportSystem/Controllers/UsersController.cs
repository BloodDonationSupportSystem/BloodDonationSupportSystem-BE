using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BloodDonationSupportSystem.Extensions;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUsers()
        {
            var response = await _userService.GetAllUsersAsync();
            return HandleResponse(response);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            // Check if the user is requesting their own data or is an admin
            var currentUserId = HttpContext.GetUserId();
            var isAdmin = HttpContext.IsInRole("Admin");

            if (id != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            var response = await _userService.GetUserByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/Users/username/{username}
        [HttpGet("username/{username}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var response = await _userService.GetUserByUsernameAsync(username);
            return HandleResponse(response);
        }

        // POST: api/Users
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<UserDto>(ModelState));
            }

            var response = await _userService.CreateUserAsync(userDto);
            return HandleResponse(response);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutUser(Guid id, [FromBody] UpdateUserDto userDto)
        {
            // Check if the user is updating their own data or is an admin
            var currentUserId = HttpContext.GetUserId();
            var isAdmin = HttpContext.IsInRole("Admin");

            if (id != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<UserDto>(ModelState));
            }

            var response = await _userService.UpdateUserAsync(id, userDto);
            return HandleResponse(response);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var response = await _userService.DeleteUserAsync(id);
            return HandleResponse(response);
        }
    }
}