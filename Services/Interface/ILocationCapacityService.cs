using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILocationCapacityService
{
    Task<ApiResponse<IEnumerable<LocationCapacityDto>>> GetByLocationIdAsync(Guid locationId);
    Task<ApiResponse<LocationCapacityDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<LocationCapacityDto>> CreateAsync(CreateLocationCapacityDto dto);
    Task<ApiResponse<IEnumerable<LocationCapacityDto>>> CreateMultipleAsync(CreateMultipleLocationCapacityDto dto);
    Task<ApiResponse<LocationCapacityDto>> UpdateAsync(Guid id, UpdateLocationCapacityDto dto);
    Task<ApiResponse> DeleteAsync(Guid id);
    Task<ApiResponse<IEnumerable<LocationCapacityDto>>> GetByLocationAndDateAsync(Guid locationId, DateTimeOffset date);
}