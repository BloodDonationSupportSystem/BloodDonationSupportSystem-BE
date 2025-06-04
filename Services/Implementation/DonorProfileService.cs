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
    public class DonorProfileService : IDonorProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DonorProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetAllDonorProfilesAsync()
        {
            try
            {
                var donorProfiles = await _unitOfWork.DonorProfiles.FindAsync(dp => dp.DeletedTime == null);
                var donorProfileDtos = donorProfiles.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<DonorProfileDto>>(donorProfileDtos)
                {
                    Message = $"Retrieved {donorProfileDtos.Count} donor profiles successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<DonorProfileDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DonorProfileDto>> GetDonorProfileByIdAsync(Guid id)
        {
            try
            {
                var donorProfile = await _unitOfWork.DonorProfiles.GetByIdWithDetailsAsync(id);
                
                if (donorProfile == null || donorProfile.DeletedTime != null)
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.NotFound, $"Donor profile with ID {id} not found");

                return new ApiResponse<DonorProfileDto>(MapToDto(donorProfile));
            }
            catch (Exception ex)
            {
                return new ApiResponse<DonorProfileDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DonorProfileDto>> GetDonorProfileByUserIdAsync(Guid userId)
        {
            try
            {
                // Verify that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                var donorProfile = await _unitOfWork.DonorProfiles.GetByUserIdAsync(userId);
                
                if (donorProfile == null || donorProfile.DeletedTime != null)
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.NotFound, $"Donor profile for user with ID {userId} not found");

                return new ApiResponse<DonorProfileDto>(MapToDto(donorProfile));
            }
            catch (Exception ex)
            {
                return new ApiResponse<DonorProfileDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetDonorProfilesByBloodGroupIdAsync(Guid bloodGroupId)
        {
            try
            {
                // Verify that the blood group exists
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodGroupId);
                if (bloodGroup == null)
                {
                    return new ApiResponse<IEnumerable<DonorProfileDto>>(HttpStatusCode.BadRequest, $"Blood group with ID {bloodGroupId} not found");
                }

                var donorProfiles = await _unitOfWork.DonorProfiles.GetByBloodGroupIdAsync(bloodGroupId);
                var activeDonorProfiles = donorProfiles.Where(dp => dp.DeletedTime == null);
                var donorProfileDtos = activeDonorProfiles.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<DonorProfileDto>>(donorProfileDtos)
                {
                    Message = $"Retrieved {donorProfileDtos.Count} donor profiles for blood group {bloodGroup.GroupName} successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<DonorProfileDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DonorProfileDto>> CreateDonorProfileAsync(CreateDonorProfileDto donorProfileDto)
        {
            try
            {
                // Verify that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(donorProfileDto.UserId);
                if (user == null)
                {
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.BadRequest, $"User with ID {donorProfileDto.UserId} not found");
                }

                // Verify that the blood group exists
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(donorProfileDto.BloodGroupId);
                if (bloodGroup == null)
                {
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.BadRequest, $"Blood group with ID {donorProfileDto.BloodGroupId} not found");
                }

                // Verify that the user doesn't already have a donor profile
                bool isAlreadyDonor = await _unitOfWork.DonorProfiles.IsUserAlreadyDonorAsync(donorProfileDto.UserId);
                if (isAlreadyDonor)
                {
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.BadRequest, $"User with ID {donorProfileDto.UserId} already has a donor profile");
                }

                var donorProfile = new DonorProfile
                {
                    DateOfBirth = donorProfileDto.DateOfBirth,
                    Gender = donorProfileDto.Gender,
                    LastDonationDate = donorProfileDto.LastDonationDate,
                    HealthStatus = donorProfileDto.HealthStatus,
                    LastHealthCheckDate = donorProfileDto.LastHealthCheckDate,
                    TotalDonations = donorProfileDto.TotalDonations,
                    Address = donorProfileDto.Address,
                    Latitude = donorProfileDto.Latitude,
                    Longitude = donorProfileDto.Longitude,
                    UserId = donorProfileDto.UserId,
                    BloodGroupId = donorProfileDto.BloodGroupId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.DonorProfiles.AddAsync(donorProfile);
                await _unitOfWork.CompleteAsync();

                // Fetch the donor profile with related details
                var createdDonorProfile = await _unitOfWork.DonorProfiles.GetByIdWithDetailsAsync(donorProfile.Id);

                return new ApiResponse<DonorProfileDto>(MapToDto(createdDonorProfile), "Donor profile created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DonorProfileDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DonorProfileDto>> UpdateDonorProfileAsync(Guid id, UpdateDonorProfileDto donorProfileDto)
        {
            try
            {
                var donorProfile = await _unitOfWork.DonorProfiles.GetByIdAsync(id);
                
                if (donorProfile == null || donorProfile.DeletedTime != null)
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.NotFound, $"Donor profile with ID {id} not found");

                // Verify that the blood group exists
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(donorProfileDto.BloodGroupId);
                if (bloodGroup == null)
                {
                    return new ApiResponse<DonorProfileDto>(HttpStatusCode.BadRequest, $"Blood group with ID {donorProfileDto.BloodGroupId} not found");
                }

                donorProfile.DateOfBirth = donorProfileDto.DateOfBirth;
                donorProfile.Gender = donorProfileDto.Gender;
                donorProfile.LastDonationDate = donorProfileDto.LastDonationDate;
                donorProfile.HealthStatus = donorProfileDto.HealthStatus;
                donorProfile.LastHealthCheckDate = donorProfileDto.LastHealthCheckDate;
                donorProfile.TotalDonations = donorProfileDto.TotalDonations;
                donorProfile.Address = donorProfileDto.Address;
                donorProfile.Latitude = donorProfileDto.Latitude;
                donorProfile.Longitude = donorProfileDto.Longitude;
                donorProfile.BloodGroupId = donorProfileDto.BloodGroupId;
                donorProfile.LastUpdatedTime = DateTimeOffset.Now;

                _unitOfWork.DonorProfiles.Update(donorProfile);
                await _unitOfWork.CompleteAsync();

                // Fetch the updated donor profile with related details
                var updatedDonorProfile = await _unitOfWork.DonorProfiles.GetByIdWithDetailsAsync(id);

                return new ApiResponse<DonorProfileDto>(MapToDto(updatedDonorProfile), "Donor profile updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<DonorProfileDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteDonorProfileAsync(Guid id)
        {
            try
            {
                var donorProfile = await _unitOfWork.DonorProfiles.GetByIdAsync(id);
                
                if (donorProfile == null || donorProfile.DeletedTime != null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Donor profile with ID {id} not found");

                // Soft delete - update DeletedTime
                donorProfile.DeletedTime = DateTimeOffset.Now;
                _unitOfWork.DonorProfiles.Update(donorProfile);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PagedApiResponse<DonorProfileDto>> GetPagedDonorProfilesAsync(DonorProfileParameters parameters)
        {
            try
            {
                var (donorProfiles, totalCount) = await _unitOfWork.DonorProfiles.GetPagedDonorProfilesAsync(parameters);
                var donorProfileDtos = donorProfiles.Select(MapToDto).ToList();

                return new PagedApiResponse<DonorProfileDto>(
                    donorProfileDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<DonorProfileDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        private DonorProfileDto MapToDto(DonorProfile donorProfile)
        {
            return new DonorProfileDto
            {
                Id = donorProfile.Id,
                DateOfBirth = donorProfile.DateOfBirth,
                Gender = donorProfile.Gender,
                LastDonationDate = donorProfile.LastDonationDate,
                HealthStatus = donorProfile.HealthStatus,
                LastHealthCheckDate = donorProfile.LastHealthCheckDate,
                TotalDonations = donorProfile.TotalDonations,
                Address = donorProfile.Address,
                Latitude = donorProfile.Latitude,
                Longitude = donorProfile.Longitude,
                UserId = donorProfile.UserId,
                UserName = donorProfile.User != null ? $"{donorProfile.User.FirstName} {donorProfile.User.LastName}" : string.Empty,
                BloodGroupId = donorProfile.BloodGroupId,
                BloodGroupName = donorProfile.BloodGroup != null ? donorProfile.BloodGroup.GroupName : string.Empty,
                CreatedTime = donorProfile.CreatedTime,
                LastUpdatedTime = donorProfile.LastUpdatedTime
            };
        }
    }
}