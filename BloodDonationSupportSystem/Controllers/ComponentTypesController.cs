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
    public class ComponentTypesController : BaseApiController
    {
        private readonly IComponentTypeService _componentTypeService;

        public ComponentTypesController(IComponentTypeService componentTypeService)
        {
            _componentTypeService = componentTypeService;
        }

        // GET: api/ComponentTypes
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComponentTypeDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetComponentTypes()
        {
            var response = await _componentTypeService.GetAllComponentTypesAsync();
            return HandleResponse(response);
        }

        // GET: api/ComponentTypes/5
        [HttpGet("{id}")]
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
        [ProducesResponseType(typeof(ApiResponse<ComponentTypeDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
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
        [ProducesResponseType(typeof(ApiResponse<ComponentTypeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
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