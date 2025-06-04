using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IComponentTypeService
    {
        Task<ApiResponse<IEnumerable<ComponentTypeDto>>> GetAllComponentTypesAsync();
        Task<ApiResponse<ComponentTypeDto>> GetComponentTypeByIdAsync(Guid id);
        Task<ApiResponse<ComponentTypeDto>> CreateComponentTypeAsync(CreateComponentTypeDto componentTypeDto);
        Task<ApiResponse<ComponentTypeDto>> UpdateComponentTypeAsync(Guid id, UpdateComponentTypeDto componentTypeDto);
        Task<ApiResponse> DeleteComponentTypeAsync(Guid id);
    }
}