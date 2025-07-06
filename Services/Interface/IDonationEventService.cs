using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDonationEventService
    {
        // Get all donation events with paging
        Task<PagedApiResponse<DonationEventDto>> GetPagedDonationEventsAsync(DonationEventParameters parameters);
        
        // Get a specific donation event by ID
        Task<ApiResponse<DonationEventDto>> GetDonationEventByIdAsync(Guid id);
        
        // Get donation events related to a specific blood request
        Task<ApiResponse<IEnumerable<DonationEventDto>>> GetDonationEventsByRequestAsync(Guid requestId, string requestType);
        
        // Get donation events assigned to a specific donor
        Task<ApiResponse<IEnumerable<DonationEventDto>>> GetDonationEventsByDonorAsync(Guid donorId);
        
        // Get donation events related to a specific appointment
        Task<ApiResponse<DonationEventDto>> GetDonationEventByAppointmentIdAsync(Guid appointmentId);
        
        // Create a new donation event
        Task<ApiResponse<DonationEventDto>> CreateDonationEventAsync(CreateDonationEventDto donationEventDto);
        
        // Create a new walk-in donation event with donor profile creation
        Task<ApiResponse<DonationEventDto>> CreateWalkInDonationEventAsync(CreateWalkInDonationEventDto walkInDto);
        
        // Update an existing donation event
        Task<ApiResponse<DonationEventDto>> UpdateDonationEventAsync(Guid id, UpdateDonationEventDto donationEventDto);
        
        // Soft delete a donation event
        Task<ApiResponse> DeleteDonationEventAsync(Guid id);

        // Check-in a donor from an appointment
        Task<ApiResponse<DonationEventDto>> CheckInAppointmentAsync(CheckInAppointmentDto checkInDto);
        
        // Perform health check on a donor
        Task<ApiResponse<DonationEventDto>> PerformHealthCheckAsync(DonorHealthCheckDto healthCheckDto);
        
        // Start the donation process
        Task<ApiResponse<DonationEventDto>> StartDonationProcessAsync(StartDonationDto startDto);
        
        // Record complications during donation
        Task<ApiResponse<DonationEventDto>> RecordDonationComplicationAsync(DonationComplicationDto complicationDto);
        
        // Complete a donation
        Task<ApiResponse<DonationEventDto>> CompleteDonationAsync(CompleteDonationDto completionDto);
        
        // Get auto-fulfill status report for monitoring and debugging
        Task<ApiResponse<object>> GetAutoFulfillStatusReportAsync(Guid? bloodGroupId = null, Guid? componentTypeId = null);
    }
}