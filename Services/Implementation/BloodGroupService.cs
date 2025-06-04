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
    public class BloodGroupService : IBloodGroupService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BloodGroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetAllBloodGroupsAsync()
        {
            try
            {
                var bloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                var bloodGroupDtos = bloodGroups.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<BloodGroupDto>>(bloodGroupDtos)
                {
                    Message = $"Retrieved {bloodGroupDtos.Count} blood groups successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<BloodGroupDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodGroupDto>> GetBloodGroupByIdAsync(Guid id)
        {
            try
            {
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(id);
                
                if (bloodGroup == null)
                    return new ApiResponse<BloodGroupDto>(HttpStatusCode.NotFound, $"Blood group with ID {id} not found");

                return new ApiResponse<BloodGroupDto>(MapToDto(bloodGroup));
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodGroupDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodGroupDto>> CreateBloodGroupAsync(CreateBloodGroupDto bloodGroupDto)
        {
            try
            {
                // Check for existing blood group with the same name
                var existingBloodGroup = await _unitOfWork.BloodGroups.GetByNameAsync(bloodGroupDto.GroupName);
                if (existingBloodGroup != null)
                {
                    return new ApiResponse<BloodGroupDto>(HttpStatusCode.Conflict, $"Blood group with name '{bloodGroupDto.GroupName}' already exists");
                }

                var bloodGroup = new BloodGroup
                {
                    GroupName = bloodGroupDto.GroupName,
                    Description = bloodGroupDto.Description
                };

                await _unitOfWork.BloodGroups.AddAsync(bloodGroup);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<BloodGroupDto>(MapToDto(bloodGroup), "Blood group created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodGroupDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodGroupDto>> UpdateBloodGroupAsync(Guid id, UpdateBloodGroupDto bloodGroupDto)
        {
            try
            {
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(id);
                
                if (bloodGroup == null)
                    return new ApiResponse<BloodGroupDto>(HttpStatusCode.NotFound, $"Blood group with ID {id} not found");

                // Check if updating to a name that already exists (but not this blood group's name)
                if (bloodGroup.GroupName != bloodGroupDto.GroupName)
                {
                    var existingBloodGroup = await _unitOfWork.BloodGroups.GetByNameAsync(bloodGroupDto.GroupName);
                    if (existingBloodGroup != null)
                    {
                        return new ApiResponse<BloodGroupDto>(HttpStatusCode.Conflict, $"Blood group with name '{bloodGroupDto.GroupName}' already exists");
                    }
                }

                bloodGroup.GroupName = bloodGroupDto.GroupName;
                bloodGroup.Description = bloodGroupDto.Description;

                _unitOfWork.BloodGroups.Update(bloodGroup);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<BloodGroupDto>(MapToDto(bloodGroup), "Blood group updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodGroupDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteBloodGroupAsync(Guid id)
        {
            try
            {
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(id);
                
                if (bloodGroup == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Blood group with ID {id} not found");

                _unitOfWork.BloodGroups.Delete(bloodGroup);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private BloodGroupDto MapToDto(BloodGroup bloodGroup)
        {
            return new BloodGroupDto
            {
                Id = bloodGroup.Id,
                GroupName = bloodGroup.GroupName,
                Description = bloodGroup.Description
            };
        }
    }
}