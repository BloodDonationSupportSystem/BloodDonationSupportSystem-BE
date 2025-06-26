using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDonationAppointmentRequestService
    {
        // Get appointment requests
        Task<PagedApiResponse<DonationAppointmentRequestDto>> GetPagedAppointmentRequestsAsync(AppointmentRequestParameters parameters);
        Task<ApiResponse<DonationAppointmentRequestDto>> GetAppointmentRequestByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetAppointmentRequestsByDonorIdAsync(Guid donorId);
        Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetPendingStaffReviewsAsync();
        Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetPendingDonorResponsesAsync();
        Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetUrgentRequestsAsync();

        // Donor-initiated requests
        Task<ApiResponse<DonationAppointmentRequestDto>> CreateDonorAppointmentRequestAsync(CreateDonorAppointmentRequestDto requestDto, Guid donorUserId);
        Task<ApiResponse<DonationAppointmentRequestDto>> UpdateDonorAppointmentRequestAsync(Guid requestId, UpdateAppointmentRequestDto updateDto, Guid donorUserId);
        Task<ApiResponse> CancelDonorAppointmentRequestAsync(Guid requestId, Guid donorUserId);

        // Staff-initiated requests (assignments)
        Task<ApiResponse<DonationAppointmentRequestDto>> CreateStaffAppointmentRequestAsync(CreateStaffAppointmentRequestDto requestDto, Guid staffUserId);
        Task<ApiResponse<DonationAppointmentRequestDto>> UpdateStaffAppointmentRequestAsync(Guid requestId, UpdateAppointmentRequestDto updateDto, Guid staffUserId);

        // Staff responses to donor requests
        Task<ApiResponse<DonationAppointmentRequestDto>> ApproveAppointmentRequestAsync(Guid requestId, StaffAppointmentResponseDto responseDto, Guid staffUserId);
        Task<ApiResponse<DonationAppointmentRequestDto>> RejectAppointmentRequestAsync(Guid requestId, StaffAppointmentResponseDto responseDto, Guid staffUserId);
        Task<ApiResponse<DonationAppointmentRequestDto>> ModifyAppointmentRequestAsync(Guid requestId, StaffAppointmentResponseDto responseDto, Guid staffUserId);

        // Donor responses to staff assignments
        Task<ApiResponse<DonationAppointmentRequestDto>> AcceptStaffAssignmentAsync(Guid requestId, DonorAppointmentResponseDto responseDto, Guid donorUserId);
        Task<ApiResponse<DonationAppointmentRequestDto>> RejectStaffAssignmentAsync(Guid requestId, DonorAppointmentResponseDto responseDto, Guid donorUserId);

        // Workflow integration
        Task<ApiResponse<DonationAppointmentRequestDto>> ConvertToWorkflowAsync(Guid requestId, Guid staffUserId);
        Task<ApiResponse> LinkToWorkflowAsync(Guid requestId, Guid workflowId);

        // Time slot and availability
        Task<ApiResponse<IEnumerable<AvailableTimeSlotsDto>>> GetAvailableTimeSlotsAsync(Guid locationId, DateTimeOffset startDate, int days = 7);
        Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetAppointmentsByLocationAndDateAsync(Guid locationId, DateTimeOffset date);

        // Maintenance and cleanup
        Task<ApiResponse<int>> MarkExpiredRequestsAsync();
        Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetRequestsExpiringInHoursAsync(int hours);

        // Notifications and reminders
        Task<ApiResponse> SendAppointmentReminderAsync(Guid requestId);
        Task<ApiResponse> SendExpiryWarningAsync(Guid requestId);
    }
}