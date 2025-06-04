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
    public class EmergencyRequestService : IEmergencyRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmergencyRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedApiResponse<EmergencyRequestDto>> GetPagedEmergencyRequestsAsync(EmergencyRequestParameters parameters)
        {
            try
            {
                var emergencyRequestRepo = _unitOfWork.EmergencyRequests;
                var (requests, totalCount) = await emergencyRequestRepo.GetPagedEmergencyRequestsAsync(parameters);

                var emergencyRequestDtos = requests.Select(MapToDto).ToList();

                return new PagedApiResponse<EmergencyRequestDto>(emergencyRequestDtos, totalCount, parameters.PageNumber, parameters.PageSize)
                {
                    Message = $"Retrieved {emergencyRequestDtos.Count} emergency requests out of {totalCount} total"
                };
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<EmergencyRequestDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<EmergencyRequestDto>> GetEmergencyRequestByIdAsync(Guid id)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");

                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest));
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<EmergencyRequestDto>> CreateEmergencyRequestAsync(CreateEmergencyRequestDto emergencyRequestDto)
        {
            try
            {
                // Validate foreign keys exist
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(emergencyRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {emergencyRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(emergencyRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {emergencyRequestDto.ComponentTypeId} does not exist");

                // Create emergency request
                var emergencyRequest = new EmergencyRequest
                {
                    PatientName = emergencyRequestDto.PatientName,
                    ContactInfo = emergencyRequestDto.ContactInfo,
                    QuantityUnits = emergencyRequestDto.QuantityUnits,
                    Status = emergencyRequestDto.Status,
                    UrgencyLevel = emergencyRequestDto.UrgencyLevel,
                    RequestDate = DateTimeOffset.UtcNow,
                    BloodGroupId = emergencyRequestDto.BloodGroupId,
                    ComponentTypeId = emergencyRequestDto.ComponentTypeId,
                    CreatedTime = DateTimeOffset.UtcNow,
                    CreatedBy = "System" // In a real application, this would be the user's name
                };

                await _unitOfWork.EmergencyRequests.AddAsync(emergencyRequest);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(emergencyRequest.Id);

                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest), "Emergency request created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<EmergencyRequestDto>> UpdateEmergencyRequestAsync(Guid id, UpdateEmergencyRequestDto emergencyRequestDto)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");

                // Validate foreign keys
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(emergencyRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {emergencyRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(emergencyRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {emergencyRequestDto.ComponentTypeId} does not exist");

                // Update emergency request
                emergencyRequest.PatientName = emergencyRequestDto.PatientName;
                emergencyRequest.ContactInfo = emergencyRequestDto.ContactInfo;
                emergencyRequest.QuantityUnits = emergencyRequestDto.QuantityUnits;
                emergencyRequest.Status = emergencyRequestDto.Status;
                emergencyRequest.UrgencyLevel = emergencyRequestDto.UrgencyLevel;
                emergencyRequest.BloodGroupId = emergencyRequestDto.BloodGroupId;
                emergencyRequest.ComponentTypeId = emergencyRequestDto.ComponentTypeId;
                emergencyRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                emergencyRequest.LastUpdatedBy = "System"; // In a real application, this would be the user's name

                _unitOfWork.EmergencyRequests.Update(emergencyRequest);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(emergencyRequest.Id);

                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest), "Emergency request updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteEmergencyRequestAsync(Guid id)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");

                // Set deleted time instead of actually deleting
                emergencyRequest.DeletedTime = DateTimeOffset.UtcNow;
                _unitOfWork.EmergencyRequests.Update(emergencyRequest);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private EmergencyRequestDto MapToDto(EmergencyRequest emergencyRequest)
        {
            return new EmergencyRequestDto
            {
                Id = emergencyRequest.Id,
                PatientName = emergencyRequest.PatientName,
                ContactInfo = emergencyRequest.ContactInfo,
                QuantityUnits = emergencyRequest.QuantityUnits,
                Status = emergencyRequest.Status,
                UrgencyLevel = emergencyRequest.UrgencyLevel,
                RequestDate = emergencyRequest.RequestDate,
                CreatedTime = emergencyRequest.CreatedTime,
                LastUpdatedTime = emergencyRequest.LastUpdatedTime,
                BloodGroupId = emergencyRequest.BloodGroupId,
                BloodGroupName = emergencyRequest.BloodGroup?.GroupName ?? "",
                ComponentTypeId = emergencyRequest.ComponentTypeId,
                ComponentTypeName = emergencyRequest.ComponentType?.Name ?? ""
            };
        }
    }
}