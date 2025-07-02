using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface ILocationService
    {
        Task<ApiResponse<IEnumerable<LocationDto>>> GetAllLocationsAsync();
        Task<ApiResponse<LocationDto>> GetLocationByIdAsync(Guid id);
        Task<ApiResponse<LocationDto>> CreateLocationAsync(CreateLocationDto locationDto);
        Task<ApiResponse<LocationDto>> UpdateLocationAsync(Guid id, UpdateLocationDto locationDto);
        Task<ApiResponse> DeleteLocationAsync(Guid id);
        Task<ApiResponse<IEnumerable<LocationDto>>> GetLocationsByUserIdAsync(Guid userId);
    }
}