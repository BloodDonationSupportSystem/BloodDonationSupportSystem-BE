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
    public class RolesController : BaseApiController
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/Roles
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRoles()
        {
            var response = await _roleService.GetAllRolesAsync();
            return HandleResponse(response);
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRole(Guid id)
        {
            var response = await _roleService.GetRoleByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/Roles
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostRole([FromBody] CreateRoleDto roleDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<RoleDto>(ModelState));
            }

            var response = await _roleService.CreateRoleAsync(roleDto);
            return HandleResponse(response);
        }

        // PUT: api/Roles/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutRole(Guid id, [FromBody] UpdateRoleDto roleDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<RoleDto>(ModelState));
            }

            var response = await _roleService.UpdateRoleAsync(id, roleDto);
            return HandleResponse(response);
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var response = await _roleService.DeleteRoleAsync(id);
            return HandleResponse(response);
        }
    }
}