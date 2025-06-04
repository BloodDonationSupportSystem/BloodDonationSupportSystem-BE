using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using Repositories.Interface;
using Services.Interface;
using Shared.Models;

namespace Services.Implementation
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BloodRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedApiResponse<BloodRequestDto>> GetPagedBloodRequestsAsync(BloodRequestParameters parameters)
        {
            try
            {
                var bloodRequestRepo = _unitOfWork.BloodRequests;
                var (requests, totalCount) = await bloodRequestRepo.GetPagedBloodRequestsAsync(parameters);

                var bloodRequestDtos = requests.Select(MapToDto).ToList();

                return new PagedApiResponse<BloodRequestDto>(bloodRequestDtos, totalCount, parameters.PageNumber, parameters.PageSize)
                {
                    Message = $"Retrieved {bloodRequestDtos.Count} blood requests out of {totalCount} total"
                };
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<BloodRequestDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> GetBloodRequestByIdAsync(Guid id)
        {
            try
            {
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");

                return new ApiResponse<BloodRequestDto>(MapToDto(bloodRequest));
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> CreateBloodRequestAsync(CreateBloodRequestDto bloodRequestDto)
        {
            try
            {
                // Validate foreign keys exist
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {bloodRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(bloodRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {bloodRequestDto.ComponentTypeId} does not exist");

                var location = await _unitOfWork.Locations.GetByIdAsync(bloodRequestDto.LocationId);
                if (location == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Location with ID {bloodRequestDto.LocationId} does not exist");

                var user = await _unitOfWork.Users.GetByIdAsync(bloodRequestDto.RequestedBy);
                if (user == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"User with ID {bloodRequestDto.RequestedBy} does not exist");

                // Create blood request
                var bloodRequest = new BloodRequest
                {
                    QuantityUnits = bloodRequestDto.QuantityUnits,
                    RequestDate = DateTimeOffset.UtcNow,
                    Status = bloodRequestDto.Status,
                    NeededByDate = bloodRequestDto.NeededByDate,
                    RequestedBy = bloodRequestDto.RequestedBy,
                    BloodGroupId = bloodRequestDto.BloodGroupId,
                    ComponentTypeId = bloodRequestDto.ComponentTypeId,
                    LocationId = bloodRequestDto.LocationId
                };

                await _unitOfWork.BloodRequests.AddAsync(bloodRequest);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);

                return new ApiResponse<BloodRequestDto>(MapToDto(bloodRequest), "Blood request created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> UpdateBloodRequestAsync(Guid id, UpdateBloodRequestDto bloodRequestDto)
        {
            try
            {
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");

                // Validate foreign keys
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {bloodRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(bloodRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {bloodRequestDto.ComponentTypeId} does not exist");

                var location = await _unitOfWork.Locations.GetByIdAsync(bloodRequestDto.LocationId);
                if (location == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Location with ID {bloodRequestDto.LocationId} does not exist");

                // Update blood request
                bloodRequest.QuantityUnits = bloodRequestDto.QuantityUnits;
                bloodRequest.Status = bloodRequestDto.Status;
                bloodRequest.NeededByDate = bloodRequestDto.NeededByDate;
                bloodRequest.BloodGroupId = bloodRequestDto.BloodGroupId;
                bloodRequest.ComponentTypeId = bloodRequestDto.ComponentTypeId;
                bloodRequest.LocationId = bloodRequestDto.LocationId;

                _unitOfWork.BloodRequests.Update(bloodRequest);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);

                return new ApiResponse<BloodRequestDto>(MapToDto(bloodRequest), "Blood request updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteBloodRequestAsync(Guid id)
        {
            try
            {
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");

                _unitOfWork.BloodRequests.Delete(bloodRequest);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private BloodRequestDto MapToDto(BloodRequest bloodRequest)
        {
            return new BloodRequestDto
            {
                Id = bloodRequest.Id,
                QuantityUnits = bloodRequest.QuantityUnits,
                RequestDate = bloodRequest.RequestDate,
                Status = bloodRequest.Status,
                NeededByDate = bloodRequest.NeededByDate,
                RequestedBy = bloodRequest.RequestedBy,
                RequesterName = bloodRequest.User != null ? $"{bloodRequest.User.FirstName} {bloodRequest.User.LastName}" : "",
                BloodGroupId = bloodRequest.BloodGroupId,
                BloodGroupName = bloodRequest.BloodGroup?.GroupName ?? "",
                ComponentTypeId = bloodRequest.ComponentTypeId,
                ComponentTypeName = bloodRequest.ComponentType?.Name ?? "",
                LocationId = bloodRequest.LocationId,
                LocationName = bloodRequest.Location?.Name ?? ""
            };
        }
    }
}