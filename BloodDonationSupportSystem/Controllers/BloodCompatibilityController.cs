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
    public class BloodCompatibilityController : BaseApiController
    {
        private readonly IBloodCompatibilityService _bloodCompatibilityService;

        public BloodCompatibilityController(IBloodCompatibilityService bloodCompatibilityService)
        {
            _bloodCompatibilityService = bloodCompatibilityService;
        }

        /// <summary>
        /// Tra c?u các nhóm máu t??ng thích cho truy?n máu toàn ph?n
        /// </summary>
        /// <param name="recipientBloodGroupId">ID nhóm máu c?a ng??i nh?n</param>
        /// <returns>Danh sách các nhóm máu t??ng thích</returns>
        [HttpGet("whole-blood/{recipientBloodGroupId}")]
        [AllowAnonymous] // Cho phép truy c?p công khai
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodGroupDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetCompatibleWholeBloodGroups(Guid recipientBloodGroupId)
        {
            var response = await _bloodCompatibilityService.GetCompatibleWholeBloodGroupsAsync(recipientBloodGroupId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Tra c?u các nhóm máu t??ng thích cho truy?n thành ph?n máu c? th?
        /// </summary>
        /// <param name="recipientBloodGroupId">ID nhóm máu c?a ng??i nh?n</param>
        /// <param name="componentTypeId">ID lo?i thành ph?n máu</param>
        /// <returns>Danh sách các nhóm máu t??ng thích cho thành ph?n máu</returns>
        [HttpGet("component/{recipientBloodGroupId}/{componentTypeId}")]
        [AllowAnonymous] // Cho phép truy c?p công khai
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodGroupDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetCompatibleComponentBloodGroups(Guid recipientBloodGroupId, Guid componentTypeId)
        {
            var response = await _bloodCompatibilityService.GetCompatibleComponentBloodGroupsAsync(recipientBloodGroupId, componentTypeId);
            return HandleResponse(response);
        }

        /// <summary>
        /// L?y ma tr?n t??ng thích ??y ?? c?a t?t c? nhóm máu
        /// </summary>
        /// <returns>Ma tr?n t??ng thích nhóm máu ??y ??</returns>
        [HttpGet("matrix")]
        [AllowAnonymous] // Cho phép truy c?p công khai
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodGroupCompatibilityDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodGroupCompatibilityMatrix()
        {
            var response = await _bloodCompatibilityService.GetBloodGroupCompatibilityMatrixAsync();
            return HandleResponse(response);
        }

        /// <summary>
        /// Tra c?u t??ng thích nhóm máu v?i tùy ch?n tìm ki?m
        /// </summary>
        /// <param name="searchDto">Thông tin tìm ki?m</param>
        /// <returns>Danh sách nhóm máu t??ng thích</returns>
        [HttpPost("search")]
        [AllowAnonymous] // Cho phép truy c?p công khai
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodGroupDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> SearchCompatibleBloodGroups([FromBody] BloodCompatibilitySearchDto searchDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<IEnumerable<BloodGroupDto>>(ModelState));
            }

            if (searchDto.IsWholeBloodSearch || !searchDto.ComponentTypeId.HasValue)
            {
                var response = await _bloodCompatibilityService.GetCompatibleWholeBloodGroupsAsync(searchDto.RecipientBloodGroupId);
                return HandleResponse(response);
            }
            else
            {
                var response = await _bloodCompatibilityService.GetCompatibleComponentBloodGroupsAsync(
                    searchDto.RecipientBloodGroupId, 
                    searchDto.ComponentTypeId.Value);
                return HandleResponse(response);
            }
        }
    }
}