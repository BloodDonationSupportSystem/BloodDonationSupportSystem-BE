using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<LocationDto>>> GetAllLocationsAsync()
        {
            try
            {
                var locations = await _unitOfWork.Locations.GetAllAsync();
                var locationDtos = locations.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<LocationDto>>(locationDtos)
                {
                    Message = $"Retrieved {locationDtos.Count} locations successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<LocationDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<LocationDto>> GetLocationByIdAsync(Guid id)
        {
            try
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(id);
                
                if (location == null)
                    return new ApiResponse<LocationDto>(HttpStatusCode.NotFound, $"Location with ID {id} not found");

                return new ApiResponse<LocationDto>(MapToDto(location));
            }
            catch (Exception ex)
            {
                return new ApiResponse<LocationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<LocationDto>>> GetLocationsByUserIdAsync(Guid userId)
        {
            try
            {
                // Get the staff assignments for the given user
                var assignments = await _unitOfWork.LocationStaffAssignments.GetByUserIdAsync(userId);
                
                if (assignments == null || !assignments.Any())
                {
                    return new ApiResponse<IEnumerable<LocationDto>>(
                        HttpStatusCode.NotFound, 
                        $"No locations found for user with ID {userId}");
                }
                
                // Extract the location IDs and get the location details
                var locationIds = assignments.Select(a => a.LocationId).ToList();
                var locations = new List<Location>();
                
                foreach (var locationId in locationIds)
                {
                    var location = await _unitOfWork.Locations.GetByIdAsync(locationId);
                    if (location != null)
                    {
                        locations.Add(location);
                    }
                }
                
                var locationDtos = locations.Select(MapToDto).ToList();
                
                return new ApiResponse<IEnumerable<LocationDto>>(locationDtos)
                {
                    Message = $"Retrieved {locationDtos.Count} locations for user ID {userId} successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<LocationDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<LocationDto>> CreateLocationAsync(CreateLocationDto locationDto)
        {
            try
            {
                // Check for existing location with the same name
                var existingLocation = await _unitOfWork.Locations.GetByNameAsync(locationDto.Name);
                if (existingLocation != null)
                {
                    return new ApiResponse<LocationDto>(HttpStatusCode.Conflict, $"Location with name '{locationDto.Name}' already exists");
                }

                var location = new Location
                {
                    Name = locationDto.Name,
                    Address = locationDto.Address,
                    Latitude = locationDto.Latitude,
                    Longitude = locationDto.Longitude
                };

                await _unitOfWork.Locations.AddAsync(location);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<LocationDto>(MapToDto(location), "Location created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<LocationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<LocationDto>> UpdateLocationAsync(Guid id, UpdateLocationDto locationDto)
        {
            try
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(id);
                
                if (location == null)
                    return new ApiResponse<LocationDto>(HttpStatusCode.NotFound, $"Location with ID {id} not found");

                // Check if updating to a name that already exists (but not this location's name)
                if (location.Name != locationDto.Name)
                {
                    var existingLocation = await _unitOfWork.Locations.GetByNameAsync(locationDto.Name);
                    if (existingLocation != null)
                    {
                        return new ApiResponse<LocationDto>(HttpStatusCode.Conflict, $"Location with name '{locationDto.Name}' already exists");
                    }
                }

                location.Name = locationDto.Name;
                location.Address = locationDto.Address;
                location.Latitude = locationDto.Latitude;
                location.Longitude = locationDto.Longitude;

                _unitOfWork.Locations.Update(location);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<LocationDto>(MapToDto(location), "Location updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<LocationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteLocationAsync(Guid id)
        {
            try
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(id);
                
                if (location == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Location with ID {id} not found");

                _unitOfWork.Locations.Delete(location);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private LocationDto MapToDto(Location location)
        {
            return new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };
        }
    }
}