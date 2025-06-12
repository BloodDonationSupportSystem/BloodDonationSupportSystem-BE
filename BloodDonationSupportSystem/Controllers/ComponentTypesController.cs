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
    public class ComponentTypesController : BaseApiController
    {
        private readonly IComponentTypeService _componentTypeService;

        public ComponentTypesController(IComponentTypeService componentTypeService)
        {
            _componentTypeService = componentTypeService;
        }

        // GET: api/ComponentTypes
        [HttpGet]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem thông tin các thành ph?n máu
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComponentTypeDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetComponentTypes()
        {
            var response = await _componentTypeService.GetAllComponentTypesAsync();
            return HandleResponse(response);
        }

        // GET: api/ComponentTypes/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem thông tin chi ti?t thành ph?n máu
        [ProducesResponseType(typeof(ApiResponse<ComponentTypeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetComponentType(Guid id)
        {
            var response = await _componentTypeService.GetComponentTypeByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/ComponentTypes
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n thêm m?i thành ph?n máu
        [ProducesResponseType(typeof(ApiResponse<ComponentTypeDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostComponentType([FromBody] CreateComponentTypeDto componentTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<ComponentTypeDto>(ModelState));
            }

            var response = await _componentTypeService.CreateComponentTypeAsync(componentTypeDto);
            return HandleResponse(response);
        }

        // PUT: api/ComponentTypes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n c?p nh?t thành ph?n máu
        [ProducesResponseType(typeof(ApiResponse<ComponentTypeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutComponentType(Guid id, [FromBody] UpdateComponentTypeDto componentTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<ComponentTypeDto>(ModelState));
            }

            var response = await _componentTypeService.UpdateComponentTypeAsync(id, componentTypeDto);
            return HandleResponse(response);
        }

        // DELETE: api/ComponentTypes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa thành ph?n máu
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteComponentType(Guid id)
        {
            var response = await _componentTypeService.DeleteComponentTypeAsync(id);
            return HandleResponse(response);
        }
    }
}