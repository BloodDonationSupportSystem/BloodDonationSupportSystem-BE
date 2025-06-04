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
    public class DonationEventService : IDonationEventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DonationEventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedApiResponse<DonationEventDto>> GetPagedDonationEventsAsync(DonationEventParameters parameters)
        {
            try
            {
                var donationEventRepo = _unitOfWork.DonationEvents;
                var (events, totalCount) = await donationEventRepo.GetPagedDonationEventsAsync(parameters);

                var donationEventDtos = events.Select(MapToDto).ToList();

                return new PagedApiResponse<DonationEventDto>(donationEventDtos, totalCount, parameters.PageNumber, parameters.PageSize)
                {
                    Message = $"Retrieved {donationEventDtos.Count} donation events out of {totalCount} total"
                };
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<DonationEventDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<DonationEventDto>> GetDonationEventByIdAsync(Guid id)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(id);
                
                if (donationEvent == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.NotFound, $"Donation event with ID {id} not found");

                return new ApiResponse<DonationEventDto>(MapToDto(donationEvent));
            }
            catch (Exception ex)
            {
                return new ApiResponse<DonationEventDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DonationEventDto>> CreateDonationEventAsync(CreateDonationEventDto donationEventDto)
        {
            try
            {
                // Validate foreign keys exist
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(donationEventDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.BadRequest, $"Blood group with ID {donationEventDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(donationEventDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.BadRequest, $"Component type with ID {donationEventDto.ComponentTypeId} does not exist");

                var location = await _unitOfWork.Locations.GetByIdAsync(donationEventDto.LocationId);
                if (location == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.BadRequest, $"Location with ID {donationEventDto.LocationId} does not exist");

                // Validate donor exists
                // You should add a method to check if a donor exists, for now we'll assume it exists
                // var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donationEventDto.DonorId);
                // if (donor == null)
                //     return new ApiResponse<DonationEventDto>(HttpStatusCode.BadRequest, $"Donor with ID {donationEventDto.DonorId} does not exist");

                // Create donation event
                var donationEvent = new DonationEvent
                {
                    QuantityUnits = donationEventDto.QuantityUnits,
                    Status = donationEventDto.Status,
                    CollectedAt = donationEventDto.CollectedAt,
                    DonorId = donationEventDto.DonorId,
                    BloodGroupId = donationEventDto.BloodGroupId,
                    ComponentTypeId = donationEventDto.ComponentTypeId,
                    LocationId = donationEventDto.LocationId,
                    CreatedTime = DateTimeOffset.UtcNow,
                    CreatedBy = "System" // In a real application, this would be the user's name
                };

                await _unitOfWork.DonationEvents.AddAsync(donationEvent);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);

                return new ApiResponse<DonationEventDto>(MapToDto(donationEvent), "Donation event created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DonationEventDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DonationEventDto>> UpdateDonationEventAsync(Guid id, UpdateDonationEventDto donationEventDto)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdAsync(id);
                
                if (donationEvent == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.NotFound, $"Donation event with ID {id} not found");

                // Validate foreign keys
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(donationEventDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.BadRequest, $"Blood group with ID {donationEventDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(donationEventDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.BadRequest, $"Component type with ID {donationEventDto.ComponentTypeId} does not exist");

                var location = await _unitOfWork.Locations.GetByIdAsync(donationEventDto.LocationId);
                if (location == null)
                    return new ApiResponse<DonationEventDto>(HttpStatusCode.BadRequest, $"Location with ID {donationEventDto.LocationId} does not exist");

                // Update donation event
                donationEvent.QuantityUnits = donationEventDto.QuantityUnits;
                donationEvent.Status = donationEventDto.Status;
                donationEvent.CollectedAt = donationEventDto.CollectedAt;
                donationEvent.BloodGroupId = donationEventDto.BloodGroupId;
                donationEvent.ComponentTypeId = donationEventDto.ComponentTypeId;
                donationEvent.LocationId = donationEventDto.LocationId;
                donationEvent.LastUpdatedTime = DateTimeOffset.UtcNow;
                donationEvent.LastUpdatedBy = "System"; // In a real application, this would be the user's name

                _unitOfWork.DonationEvents.Update(donationEvent);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);

                return new ApiResponse<DonationEventDto>(MapToDto(donationEvent), "Donation event updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<DonationEventDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteDonationEventAsync(Guid id)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdAsync(id);
                
                if (donationEvent == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Donation event with ID {id} not found");

                // Set deleted time instead of actually deleting
                donationEvent.DeletedTime = DateTimeOffset.UtcNow;
                _unitOfWork.DonationEvents.Update(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private DonationEventDto MapToDto(DonationEvent donationEvent)
        {
            return new DonationEventDto
            {
                Id = donationEvent.Id,
                QuantityUnits = donationEvent.QuantityUnits,
                Status = donationEvent.Status,
                CollectedAt = donationEvent.CollectedAt,
                CreatedTime = donationEvent.CreatedTime,
                LastUpdatedTime = donationEvent.LastUpdatedTime,
                DonorId = donationEvent.DonorId,
                DonorName = donationEvent.DonorProfile?.User != null ? 
                    $"{donationEvent.DonorProfile.User.FirstName} {donationEvent.DonorProfile.User.LastName}" : "",
                BloodGroupId = donationEvent.BloodGroupId,
                BloodGroupName = donationEvent.BloodGroup?.GroupName ?? "",
                ComponentTypeId = donationEvent.ComponentTypeId,
                ComponentTypeName = donationEvent.ComponentType?.Name ?? "",
                LocationId = donationEvent.LocationId,
                LocationName = donationEvent.Location?.Name ?? ""
            };
        }
    }
}