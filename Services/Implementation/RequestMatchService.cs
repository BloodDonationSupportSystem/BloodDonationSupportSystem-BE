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
    public class RequestMatchService : IRequestMatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RequestMatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetAllRequestMatchesAsync()
        {
            try
            {
                var requestMatches = await _unitOfWork.RequestMatches.FindAsync(rm => rm.DeletedTime == null);
                var requestMatchDtos = requestMatches.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<RequestMatchDto>>(requestMatchDtos)
                {
                    Message = $"Retrieved {requestMatchDtos.Count} request matches successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<RequestMatchDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<RequestMatchDto>> GetRequestMatchByIdAsync(Guid id)
        {
            try
            {
                var requestMatch = await _unitOfWork.RequestMatches.GetByIdWithDetailsAsync(id);
                
                if (requestMatch == null || requestMatch.DeletedTime != null)
                    return new ApiResponse<RequestMatchDto>(HttpStatusCode.NotFound, $"Request match with ID {id} not found");

                return new ApiResponse<RequestMatchDto>(MapToDto(requestMatch));
            }
            catch (Exception ex)
            {
                return new ApiResponse<RequestMatchDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetRequestMatchesByRequestIdAsync(Guid requestId)
        {
            try
            {
                // Verify that the request exists
                var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse<IEnumerable<RequestMatchDto>>(HttpStatusCode.BadRequest, $"Blood request with ID {requestId} not found");
                }

                var requestMatches = await _unitOfWork.RequestMatches.GetByRequestIdAsync(requestId);
                var requestMatchDtos = requestMatches.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<RequestMatchDto>>(requestMatchDtos)
                {
                    Message = $"Retrieved {requestMatchDtos.Count} request matches for blood request {requestId} successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<RequestMatchDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetRequestMatchesByEmergencyRequestIdAsync(Guid emergencyRequestId)
        {
            try
            {
                // Verify that the emergency request exists
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(emergencyRequestId);
                if (emergencyRequest == null)
                {
                    return new ApiResponse<IEnumerable<RequestMatchDto>>(HttpStatusCode.BadRequest, $"Emergency request with ID {emergencyRequestId} not found");
                }

                var requestMatches = await _unitOfWork.RequestMatches.GetByEmergencyRequestIdAsync(emergencyRequestId);
                var requestMatchDtos = requestMatches.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<RequestMatchDto>>(requestMatchDtos)
                {
                    Message = $"Retrieved {requestMatchDtos.Count} request matches for emergency request {emergencyRequestId} successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<RequestMatchDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetRequestMatchesByDonationEventIdAsync(Guid donationEventId)
        {
            try
            {
                // Verify that the donation event exists
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdAsync(donationEventId);
                if (donationEvent == null)
                {
                    return new ApiResponse<IEnumerable<RequestMatchDto>>(HttpStatusCode.BadRequest, $"Donation event with ID {donationEventId} not found");
                }

                var requestMatches = await _unitOfWork.RequestMatches.GetByDonationEventIdAsync(donationEventId);
                var requestMatchDtos = requestMatches.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<RequestMatchDto>>(requestMatchDtos)
                {
                    Message = $"Retrieved {requestMatchDtos.Count} request matches for donation event {donationEventId} successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<RequestMatchDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<RequestMatchDto>> CreateRequestMatchAsync(CreateRequestMatchDto requestMatchDto)
        {
            try
            {
                // Verify that the request exists
                var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestMatchDto.RequestId);
                if (request == null)
                {
                    return new ApiResponse<RequestMatchDto>(HttpStatusCode.BadRequest, $"Blood request with ID {requestMatchDto.RequestId} not found");
                }

                // Verify that the emergency request exists
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(requestMatchDto.EmergencyRequestId);
                if (emergencyRequest == null)
                {
                    return new ApiResponse<RequestMatchDto>(HttpStatusCode.BadRequest, $"Emergency request with ID {requestMatchDto.EmergencyRequestId} not found");
                }

                // Verify that the donation event exists
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdAsync(requestMatchDto.DonationEventId);
                if (donationEvent == null)
                {
                    return new ApiResponse<RequestMatchDto>(HttpStatusCode.BadRequest, $"Donation event with ID {requestMatchDto.DonationEventId} not found");
                }

                // Verify that the units assigned is valid
                if (requestMatchDto.UnitsAssigned <= 0)
                {
                    return new ApiResponse<RequestMatchDto>(HttpStatusCode.BadRequest, "Units assigned must be greater than zero");
                }

                var requestMatch = new RequestMatch
                {
                    RequestId = requestMatchDto.RequestId,
                    EmergencyRequestId = requestMatchDto.EmergencyRequestId,
                    DonationEventId = requestMatchDto.DonationEventId,
                    UnitsAssigned = requestMatchDto.UnitsAssigned,
                    MatchDate = DateTimeOffset.Now,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.RequestMatches.AddAsync(requestMatch);
                await _unitOfWork.CompleteAsync();

                // Fetch the request match with details
                var createdRequestMatch = await _unitOfWork.RequestMatches.GetByIdWithDetailsAsync(requestMatch.Id);

                return new ApiResponse<RequestMatchDto>(MapToDto(createdRequestMatch), "Request match created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<RequestMatchDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<RequestMatchDto>> UpdateRequestMatchAsync(Guid id, UpdateRequestMatchDto requestMatchDto)
        {
            try
            {
                var requestMatch = await _unitOfWork.RequestMatches.GetByIdAsync(id);
                
                if (requestMatch == null || requestMatch.DeletedTime != null)
                    return new ApiResponse<RequestMatchDto>(HttpStatusCode.NotFound, $"Request match with ID {id} not found");

                // Verify that the units assigned is valid
                if (requestMatchDto.UnitsAssigned <= 0)
                {
                    return new ApiResponse<RequestMatchDto>(HttpStatusCode.BadRequest, "Units assigned must be greater than zero");
                }

                requestMatch.UnitsAssigned = requestMatchDto.UnitsAssigned;
                requestMatch.LastUpdatedTime = DateTimeOffset.Now;

                _unitOfWork.RequestMatches.Update(requestMatch);
                await _unitOfWork.CompleteAsync();

                // Fetch the updated request match with details
                var updatedRequestMatch = await _unitOfWork.RequestMatches.GetByIdWithDetailsAsync(id);

                return new ApiResponse<RequestMatchDto>(MapToDto(updatedRequestMatch), "Request match updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<RequestMatchDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteRequestMatchAsync(Guid id)
        {
            try
            {
                var requestMatch = await _unitOfWork.RequestMatches.GetByIdAsync(id);
                
                if (requestMatch == null || requestMatch.DeletedTime != null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Request match with ID {id} not found");

                // Soft delete - update DeletedTime
                requestMatch.DeletedTime = DateTimeOffset.Now;
                _unitOfWork.RequestMatches.Update(requestMatch);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PagedApiResponse<RequestMatchDto>> GetPagedRequestMatchesAsync(RequestMatchParameters parameters)
        {
            try
            {
                var (requestMatches, totalCount) = await _unitOfWork.RequestMatches.GetPagedRequestMatchesAsync(parameters);
                var requestMatchDtos = requestMatches.Select(MapToDto).ToList();

                return new PagedApiResponse<RequestMatchDto>(
                    requestMatchDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<RequestMatchDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        private RequestMatchDto MapToDto(RequestMatch requestMatch)
        {
            return new RequestMatchDto
            {
                Id = requestMatch.Id,
                MatchDate = requestMatch.MatchDate,
                UnitsAssigned = requestMatch.UnitsAssigned,
                RequestId = requestMatch.RequestId,
                RequestInfo = requestMatch.BloodRequest != null 
                    ? $"{requestMatch.BloodRequest.BloodGroup?.GroupName ?? "Unknown"} - {requestMatch.BloodRequest.ComponentType?.Name ?? "Unknown"}" 
                    : "Unknown",
                EmergencyRequestId = requestMatch.EmergencyRequestId,
                EmergencyRequestInfo = requestMatch.EmergencyRequest != null 
                    ? $"{requestMatch.EmergencyRequest.PatientName} - {requestMatch.EmergencyRequest.BloodGroup?.GroupName ?? "Unknown"}" 
                    : "Unknown",
                DonationEventId = requestMatch.DonationEventId,
                DonationEventInfo = requestMatch.DonationEvent != null 
                    ? $"{requestMatch.DonationEvent.BloodGroup?.GroupName ?? "Unknown"} - {requestMatch.DonationEvent.ComponentType?.Name ?? "Unknown"}" 
                    : "Unknown",
                CreatedTime = requestMatch.CreatedTime,
                LastUpdatedTime = requestMatch.LastUpdatedTime
            };
        }
    }
}