using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Repositories.Interface;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class BloodDonationWorkflowService : IBloodDonationWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BloodDonationWorkflowService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IBloodRequestService _bloodRequestService;
        private readonly IEmergencyRequestService _emergencyRequestService;
        private readonly IBloodInventoryService _bloodInventoryService;

        public BloodDonationWorkflowService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BloodDonationWorkflowService> logger,
            INotificationService notificationService,
            IBloodRequestService bloodRequestService,
            IEmergencyRequestService emergencyRequestService,
            IBloodInventoryService bloodInventoryService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _bloodRequestService = bloodRequestService;
            _emergencyRequestService = emergencyRequestService;
            _bloodInventoryService = bloodInventoryService;
        }

        public async Task<PagedApiResponse<DonationWorkflowDto>> GetPagedDonationWorkflowsAsync(DonationWorkflowParameters parameters)
        {
            try
            {
                var (workflows, totalCount) = await _unitOfWork.BloodDonationWorkflows.GetPagedWorkflowsAsync(parameters);
                var workflowDtos = _mapper.Map<IEnumerable<DonationWorkflowDto>>(workflows);
                
                return new PagedApiResponse<DonationWorkflowDto>(
                    workflowDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged donation workflows");
                var errorResponse = new PagedApiResponse<DonationWorkflowDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Error occurred while getting donation workflows"
                };
                return errorResponse;
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> GetDonationWorkflowByIdAsync(Guid id)
        {
            try
            {
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(id);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }

                var workflowDto = _mapper.Map<DonationWorkflowDto>(workflow);
                return new ApiResponse<DonationWorkflowDto>(workflowDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation workflow with ID: {Id}", id);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation workflow");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetDonationWorkflowsByRequestAsync(Guid requestId, string requestType)
        {
            try
            {
                var workflows = await _unitOfWork.BloodDonationWorkflows.GetByRequestAsync(requestId, requestType);
                var workflowDtos = _mapper.Map<IEnumerable<DonationWorkflowDto>>(workflows);
                
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    workflowDtos,
                    $"Found {workflowDtos.Count()} workflows for the {requestType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation workflows for request. RequestId: {RequestId}, RequestType: {RequestType}",
                    requestId, requestType);
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation workflows for request");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetDonationWorkflowsByDonorAsync(Guid donorId)
        {
            try
            {
                var workflows = await _unitOfWork.BloodDonationWorkflows.GetByDonorIdAsync(donorId);
                var workflowDtos = _mapper.Map<IEnumerable<DonationWorkflowDto>>(workflows);
                
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    workflowDtos,
                    $"Found {workflowDtos.Count()} workflows for the donor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation workflows for donor. DonorId: {DonorId}", donorId);
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation workflows for donor");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> CreateDonationWorkflowAsync(CreateDonationWorkflowDto workflowDto)
        {
            try
            {
                // Fetch required details based on request type
                Guid bloodGroupId;
                Guid componentTypeId;
                double requiredQuantity;
                
                if (workflowDto.RequestType == "BloodRequest")
                {
                    var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(workflowDto.RequestId);
                    if (bloodRequest == null)
                    {
                        return new ApiResponse<DonationWorkflowDto>(
                            HttpStatusCode.NotFound,
                            "Blood request not found");
                    }
                    
                    bloodGroupId = bloodRequest.BloodGroupId;
                    componentTypeId = bloodRequest.ComponentTypeId;
                    requiredQuantity = bloodRequest.QuantityUnits;
                }
                else if (workflowDto.RequestType == "EmergencyRequest")
                {
                    var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(workflowDto.RequestId);
                    if (emergencyRequest == null)
                    {
                        return new ApiResponse<DonationWorkflowDto>(
                            HttpStatusCode.NotFound,
                            "Emergency request not found");
                    }
                    
                    bloodGroupId = emergencyRequest.BloodGroupId;
                    componentTypeId = emergencyRequest.ComponentTypeId;
                    requiredQuantity = emergencyRequest.QuantityUnits;
                }
                else
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Invalid request type. Must be 'BloodRequest' or 'EmergencyRequest'");
                }
                
                // Create a new workflow
                var workflow = new BloodDonationWorkflow
                {
                    Id = Guid.NewGuid(),
                    RequestId = workflowDto.RequestId,
                    RequestType = workflowDto.RequestType,
                    BloodGroupId = bloodGroupId,
                    ComponentTypeId = componentTypeId,
                    Status = "Created",
                    Notes = workflowDto.Notes,
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsActive = true
                };
                
                // Check if the request can be fulfilled from inventory
                if (workflowDto.CheckInventoryFirst)
                {
                    var hasInventory = await _unitOfWork.BloodDonationWorkflows
                        .CheckCompatibleBloodInventoryAsync(bloodGroupId, componentTypeId, requiredQuantity);
                    
                    if (hasInventory)
                    {
                        // Get the appropriate inventory item
                        var inventory = await _unitOfWork.BloodDonationWorkflows
                            .GetCompatibleBloodInventoryAsync(bloodGroupId, componentTypeId, requiredQuantity);
                        
                        if (inventory != null)
                        {
                            workflow.InventoryId = inventory.Id;
                            workflow.Status = "CompletedFromInventory";
                            workflow.CompletedTime = DateTimeOffset.UtcNow;
                            
                            // Update inventory status
                            inventory.Status = "Reserved";
                            _unitOfWork.BloodInventories.Update(inventory);
                        }
                    }
                }
                
                // Save the workflow
                await _unitOfWork.BloodDonationWorkflows.AddAsync(workflow);
                await _unitOfWork.CompleteAsync();
                
                // Send notification if needed based on workflow status
                if (workflow.Status == "Created")
                {
                    // Notify administrators or staff that a new workflow needs donor assignment
                    await SendWorkflowNotificationAsync(workflow, "New donation workflow created and awaiting donor assignment");
                }
                else if (workflow.Status == "CompletedFromInventory")
                {
                    // Notify about fulfillment from inventory
                    await SendWorkflowNotificationAsync(workflow, "Request fulfilled from existing blood inventory");
                    
                    // Update the original request status
                    await UpdateRequestStatusAsync(workflowDto.RequestId, workflowDto.RequestType, "Fulfilled");
                }
                
                var result = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(workflow.Id);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(result);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    "Donation workflow created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating donation workflow for request ID: {RequestId}", 
                    workflowDto.RequestId);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while creating donation workflow");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> UpdateDonationWorkflowAsync(Guid id, UpdateDonationWorkflowDto workflowDto)
        {
            try
            {
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(id);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                // Update workflow properties
                if (!string.IsNullOrEmpty(workflowDto.Status))
                {
                    workflow.Status = workflowDto.Status;
                }
                
                if (workflowDto.DonorId.HasValue)
                {
                    // Verify donor exists
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(workflowDto.DonorId.Value);
                    if (donor == null)
                    {
                        return new ApiResponse<DonationWorkflowDto>(
                            HttpStatusCode.BadRequest,
                            "Specified donor profile does not exist");
                    }
                    
                    workflow.DonorId = workflowDto.DonorId;
                    
                    // If donor is assigned, update status if it's still in "Created" state
                    if (workflow.Status == "Created")
                    {
                        workflow.Status = "DonorAssigned";
                    }
                }
                
                if (workflowDto.InventoryId.HasValue)
                {
                    // Verify inventory exists
                    var inventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(workflowDto.InventoryId.Value);
                    if (inventory == null)
                    {
                        return new ApiResponse<DonationWorkflowDto>(
                            HttpStatusCode.BadRequest,
                            "Specified inventory item does not exist");
                    }
                    
                    workflow.InventoryId = workflowDto.InventoryId;
                    
                    // Update status if fulfilling from inventory
                    workflow.Status = "CompletedFromInventory";
                    workflow.CompletedTime = DateTimeOffset.UtcNow;
                    
                    // Update inventory status
                    inventory.Status = "Reserved";
                    _unitOfWork.BloodInventories.Update(inventory);
                    
                    // Update the original request status
                    await UpdateRequestStatusAsync(workflow.RequestId, workflow.RequestType, "Fulfilled");
                }
                
                if (workflowDto.AppointmentDate.HasValue)
                {
                    workflow.AppointmentDate = workflowDto.AppointmentDate;
                    workflow.AppointmentLocation = workflowDto.AppointmentLocation;
                    
                    // If appointment is set, update status
                    if (workflow.Status == "DonorAssigned")
                    {
                        workflow.Status = "Scheduled";
                    }
                }
                
                if (!string.IsNullOrEmpty(workflowDto.Notes))
                {
                    workflow.Notes = workflowDto.Notes;
                }
                
                // Update tracking info
                workflow.LastUpdatedTime = DateTimeOffset.UtcNow;
                
                // Update workflow
                _unitOfWork.BloodDonationWorkflows.Update(workflow);
                await _unitOfWork.CompleteAsync();
                
                // Send notification based on workflow status change
                await SendStatusChangeNotificationAsync(workflow);
                
                var updatedWorkflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(id);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(updatedWorkflow);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    "Donation workflow updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating donation workflow with ID: {Id}", id);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while updating donation workflow");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> UpdateWorkflowStatusAsync(WorkflowStatusUpdateDto statusUpdateDto)
        {
            try
            {
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(statusUpdateDto.WorkflowId);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                // Update workflow status
                string previousStatus = workflow.Status;
                workflow.Status = statusUpdateDto.NewStatus;
                workflow.StatusDescription = statusUpdateDto.Notes;
                workflow.LastUpdatedTime = DateTimeOffset.UtcNow;
                
                // Set completion time if the status is "Completed" or "CompletedFromInventory"
                if (statusUpdateDto.NewStatus == "Completed" || statusUpdateDto.NewStatus == "CompletedFromInventory")
                {
                    workflow.CompletedTime = DateTimeOffset.UtcNow;
                    
                    // Update the original request status
                    await UpdateRequestStatusAsync(workflow.RequestId, workflow.RequestType, "Fulfilled");
                }
                else if (statusUpdateDto.NewStatus == "Cancelled")
                {
                    // Handle cancellation logic
                    // If there was an inventory assigned, release it
                    if (workflow.InventoryId.HasValue)
                    {
                        var inventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(workflow.InventoryId.Value);
                        if (inventory != null && inventory.Status == "Reserved")
                        {
                            inventory.Status = "Available";
                            _unitOfWork.BloodInventories.Update(inventory);
                        }
                    }
                }
                
                _unitOfWork.BloodDonationWorkflows.Update(workflow);
                await _unitOfWork.CompleteAsync();
                
                // Send notification based on status change
                await SendStatusChangeNotificationAsync(workflow, previousStatus);
                
                var updatedWorkflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(statusUpdateDto.WorkflowId);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(updatedWorkflow);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    $"Workflow status updated to {statusUpdateDto.NewStatus}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating workflow status. WorkflowId: {WorkflowId}, NewStatus: {NewStatus}",
                    statusUpdateDto.WorkflowId, statusUpdateDto.NewStatus);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while updating workflow status");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> AssignDonorAsync(AssignDonorDto assignDonorDto)
        {
            try
            {
                // Verify workflow exists
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(assignDonorDto.WorkflowId);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                // Verify donor exists
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(assignDonorDto.DonorId);
                if (donor == null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Specified donor profile does not exist");
                }
                
                // Verify blood compatibility
                if (donor.BloodGroupId != workflow.BloodGroupId)
                {
                    // For future enhancement: Check compatible blood groups, not just exact match
                    // This would require integration with blood compatibility service
                }
                
                // Assign donor to workflow
                var success = await _unitOfWork.BloodDonationWorkflows.AssignDonorAsync(
                    assignDonorDto.WorkflowId,
                    assignDonorDto.DonorId,
                    assignDonorDto.AppointmentDate,
                    assignDonorDto.AppointmentLocation);
                
                if (!success)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.InternalServerError,
                        "Failed to assign donor to workflow");
                }
                
                // Send notification to donor
                if (donor.UserId != Guid.Empty)
                {
                    string message = "You have been assigned to a blood donation request.";
                    if (assignDonorDto.AppointmentDate.HasValue)
                    {
                        message += $" An appointment has been scheduled for {assignDonorDto.AppointmentDate.Value.ToString("g")}";
                        if (!string.IsNullOrEmpty(assignDonorDto.AppointmentLocation))
                        {
                            message += $" at {assignDonorDto.AppointmentLocation}.";
                        }
                        else
                        {
                            message += ".";
                        }
                    }
                    
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = donor.UserId,
                        Type = "DonorAssignment",
                        Message = message
                    });
                }
                
                var updatedWorkflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(assignDonorDto.WorkflowId);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(updatedWorkflow);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    "Donor assigned to workflow successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning donor to workflow. WorkflowId: {WorkflowId}, DonorId: {DonorId}",
                    assignDonorDto.WorkflowId, assignDonorDto.DonorId);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while assigning donor to workflow");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> FulfillFromInventoryAsync(FulfillFromInventoryDto fulfillDto)
        {
            try
            {
                // Verify workflow exists
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(fulfillDto.WorkflowId);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                // Verify inventory exists and is available
                var inventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(fulfillDto.InventoryId);
                if (inventory == null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Specified inventory item does not exist");
                }
                
                if (inventory.Status != "Available")
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        $"Inventory item is not available (current status: {inventory.Status})");
                }
                
                if (inventory.ExpirationDate <= DateTimeOffset.UtcNow)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Inventory item has expired");
                }
                
                // Verify blood compatibility
                if (inventory.BloodGroupId != workflow.BloodGroupId || inventory.ComponentTypeId != workflow.ComponentTypeId)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Inventory item blood group or component type does not match the request");
                }
                
                // Fulfill from inventory
                var success = await _unitOfWork.BloodDonationWorkflows.FulfillFromInventoryAsync(
                    fulfillDto.WorkflowId,
                    fulfillDto.InventoryId);
                
                if (!success)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.InternalServerError,
                        "Failed to fulfill request from inventory");
                }
                
                // Update the original request status
                await UpdateRequestStatusAsync(workflow.RequestId, workflow.RequestType, "Fulfilled");
                
                // Send notification
                await SendWorkflowNotificationAsync(workflow, "Request fulfilled from blood inventory");
                
                var updatedWorkflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(fulfillDto.WorkflowId);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(updatedWorkflow);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    "Request fulfilled from inventory successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fulfilling request from inventory. WorkflowId: {WorkflowId}, InventoryId: {InventoryId}",
                    fulfillDto.WorkflowId, fulfillDto.InventoryId);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while fulfilling request from inventory");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> CompleteDonationAsync(CompleteDonationDto completeDonationDto)
        {
            try
            {
                // Verify workflow exists
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(completeDonationDto.WorkflowId);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                // Verify donor is assigned
                if (!workflow.DonorId.HasValue)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Cannot complete donation: No donor assigned to this workflow");
                }
                
                // Complete the donation
                var success = await _unitOfWork.BloodDonationWorkflows.CompleteDonationAsync(
                    completeDonationDto.WorkflowId,
                    completeDonationDto.DonationDate,
                    completeDonationDto.DonationLocation,
                    completeDonationDto.QuantityDonated);
                
                if (!success)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.InternalServerError,
                        "Failed to complete donation");
                }
                
                // Update donor's last donation date
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(workflow.DonorId.Value);
                if (donor != null)
                {
                    donor.LastDonationDate = completeDonationDto.DonationDate;
                    donor.TotalDonations += 1;
                    
                    // Calculate next available donation date (typically 3 months from donation)
                    donor.NextAvailableDonationDate = completeDonationDto.DonationDate.AddDays(90);
                    
                    _unitOfWork.DonorProfiles.Update(donor);
                    
                    // Ki?m tra cài ??t nh?c nh? c?a ng??i hi?n máu
                    var reminderSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(donor.Id);
                    
                    // N?u ch?a có cài ??t nh?c nh?, t?o cài ??t m?c ??nh
                    if (reminderSettings == null && donor.UserId != Guid.Empty)
                    {
                        reminderSettings = new DonorReminderSettings
                        {
                            DonorProfileId = donor.Id,
                            EnableReminders = true,
                            DaysBeforeEligible = 7,
                            EmailNotifications = true,
                            InAppNotifications = true
                        };
                        
                        await _unitOfWork.DonorReminderSettings.AddAsync(reminderSettings);
                    }
                }
                
                // Update the original request status
                await UpdateRequestStatusAsync(workflow.RequestId, workflow.RequestType, "Fulfilled");
                
                // Add to blood inventory if applicable
                if (completeDonationDto.QuantityDonated > 0)
                {
                    // Get component type for shelf life information
                    var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(workflow.ComponentTypeId);
                    int shelfLifeDays = componentType?.ShelfLifeDays ?? 35; // Default to 35 days if not specified
                    
                    // Create inventory entry
                    await _bloodInventoryService.CreateBloodInventoryAsync(new CreateBloodInventoryDto
                    {
                        BloodGroupId = workflow.BloodGroupId,
                        ComponentTypeId = workflow.ComponentTypeId,
                        QuantityUnits = (int)completeDonationDto.QuantityDonated,
                        ExpirationDate = completeDonationDto.DonationDate.AddDays(shelfLifeDays),
                        Status = "Available",
                        InventorySource = $"Donation from workflow ID: {workflow.Id}",
                        DonationEventId = Guid.Empty // Use a default value since it's required but may not be applicable
                    });
                }
                
                // Send notification
                if (workflow.DonorId.HasValue)
                {
                    var donorProfile = await _unitOfWork.DonorProfiles.GetByIdAsync(workflow.DonorId.Value);
                    if (donorProfile != null && donorProfile.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donorProfile.UserId,
                            Type = "DonationCompleted",
                            Message = $"Thank you for your blood donation of {completeDonationDto.QuantityDonated} units on {completeDonationDto.DonationDate.ToString("d")}. Your contribution is saving lives!"
                        });
                        
                        // Thêm thông báo v? ngày có th? hi?n máu ti?p theo
                        if (donorProfile.NextAvailableDonationDate.HasValue)
                        {
                            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                            {
                                UserId = donorProfile.UserId,
                                Type = "NextDonationDate",
                                Message = $"B?n s? ?? ?i?u ki?n hi?n máu ti?p theo vào ngày {donorProfile.NextAvailableDonationDate.Value.ToString("dd/MM/yyyy")}. Chúng tôi s? g?i nh?c nh? khi ??n th?i ?i?m ?ó."
                            });
                        }
                    }
                }
                
                // Get updated workflow with details
                var updatedWorkflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(completeDonationDto.WorkflowId);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(updatedWorkflow);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    "Donation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while completing donation. WorkflowId: {WorkflowId}",
                    completeDonationDto.WorkflowId);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while completing donation");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> CancelDonationWorkflowAsync(Guid id, string reason)
        {
            try
            {
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(id);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                // Can't cancel completed workflows
                if (workflow.Status == "Completed" || workflow.Status == "CompletedFromInventory")
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Cannot cancel a completed workflow");
                }
                
                // Update workflow status
                workflow.Status = "Cancelled";
                workflow.StatusDescription = reason;
                workflow.LastUpdatedTime = DateTimeOffset.UtcNow;
                workflow.IsActive = false;
                
                // If there was an inventory assigned, release it
                if (workflow.InventoryId.HasValue)
                {
                    var inventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(workflow.InventoryId.Value);
                    if (inventory != null && inventory.Status == "Reserved")
                    {
                        inventory.Status = "Available";
                        _unitOfWork.BloodInventories.Update(inventory);
                    }
                }
                
                _unitOfWork.BloodDonationWorkflows.Update(workflow);
                await _unitOfWork.CompleteAsync();
                
                // Send notification to assigned donor if any
                if (workflow.DonorId.HasValue)
                {
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(workflow.DonorId.Value);
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Type = "DonationCancelled",
                            Message = $"Your blood donation appointment has been cancelled. Reason: {reason}"
                        });
                    }
                }
                
                var updatedWorkflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(id);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(updatedWorkflow);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    "Donation workflow cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling donation workflow with ID: {Id}", id);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while cancelling donation workflow");
            }
        }

        public async Task<ApiResponse> DeleteDonationWorkflowAsync(Guid id)
        {
            try
            {
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdAsync(id);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                // Soft delete
                workflow.DeletedTime = DateTimeOffset.UtcNow;
                workflow.IsActive = false;
                
                _unitOfWork.BloodDonationWorkflows.Update(workflow);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse("Donation workflow deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting donation workflow with ID: {Id}", id);
                return new ApiResponse(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while deleting donation workflow");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetWorkflowsByStatusAsync(string status)
        {
            try
            {
                var workflows = await _unitOfWork.BloodDonationWorkflows.GetByStatusAsync(status);
                var workflowDtos = _mapper.Map<IEnumerable<DonationWorkflowDto>>(workflows);
                
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    workflowDtos,
                    $"Found {workflowDtos.Count()} workflows with status '{status}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting workflows by status: {Status}", status);
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting workflows by status");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetPendingAppointmentsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            try
            {
                var appointments = await _unitOfWork.BloodDonationWorkflows.GetPendingAppointmentsAsync(startDate, endDate);
                var appointmentDtos = _mapper.Map<IEnumerable<DonationWorkflowDto>>(appointments);
                
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    appointmentDtos,
                    $"Found {appointmentDtos.Count()} pending appointments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting pending appointments");
                return new ApiResponse<IEnumerable<DonationWorkflowDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting pending appointments");
            }
        }

        public async Task<ApiResponse<DonationWorkflowDto>> ConfirmAppointmentAsync(Guid workflowId)
        {
            try
            {
                var workflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(workflowId);
                if (workflow == null || workflow.DeletedTime != null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.NotFound,
                        "Donation workflow not found");
                }
                
                if (workflow.Status != "Scheduled" || workflow.AppointmentDate == null)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.BadRequest,
                        "Cannot confirm appointment: Workflow is not in scheduled state or has no appointment date");
                }
                
                var success = await _unitOfWork.BloodDonationWorkflows.ConfirmAppointmentAsync(workflowId);
                if (!success)
                {
                    return new ApiResponse<DonationWorkflowDto>(
                        HttpStatusCode.InternalServerError,
                        "Failed to confirm appointment");
                }
                
                // Send notification
                if (workflow.DonorId.HasValue)
                {
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(workflow.DonorId.Value);
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Type = "AppointmentConfirmed",
                            Message = $"Your blood donation appointment on {workflow.AppointmentDate.Value.ToString("g")} has been confirmed."
                        });
                    }
                }
                
                var updatedWorkflow = await _unitOfWork.BloodDonationWorkflows.GetByIdWithDetailsAsync(workflowId);
                var workflowResultDto = _mapper.Map<DonationWorkflowDto>(updatedWorkflow);
                
                return new ApiResponse<DonationWorkflowDto>(
                    workflowResultDto,
                    "Appointment confirmed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while confirming appointment for workflow ID: {WorkflowId}", workflowId);
                return new ApiResponse<DonationWorkflowDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while confirming appointment");
            }
        }

        #region Helper Methods

        private async Task SendWorkflowNotificationAsync(BloodDonationWorkflow workflow, string message)
        {
            try
            {
                // Notify assigned donor if any
                if (workflow.DonorId.HasValue)
                {
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(workflow.DonorId.Value);
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Type = "WorkflowUpdate",
                            Message = message
                        });
                    }
                }
                
                // Notify staff and admins
                // In a real implementation, you would have a way to get staff/admin user IDs
                // For this example, we'll just log the message
                _logger.LogInformation("Workflow notification (staff/admin): {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending workflow notification");
            }
        }

        private async Task SendStatusChangeNotificationAsync(BloodDonationWorkflow workflow, string previousStatus = null)
        {
            try
            {
                string message = $"Blood donation workflow status updated to {workflow.Status}";
                if (!string.IsNullOrEmpty(workflow.StatusDescription))
                {
                    message += $": {workflow.StatusDescription}";
                }
                
                await SendWorkflowNotificationAsync(workflow, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status change notification");
            }
        }

        private async Task UpdateRequestStatusAsync(Guid requestId, string requestType, string newStatus)
        {
            try
            {
                if (requestType == "BloodRequest")
                {
                    var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
                    if (request != null)
                    {
                        request.Status = newStatus;
                        
                        _unitOfWork.BloodRequests.Update(request);
                        await _unitOfWork.CompleteAsync();
                    }
                }
                else if (requestType == "EmergencyRequest")
                {
                    var request = await _unitOfWork.EmergencyRequests.GetByIdAsync(requestId);
                    if (request != null)
                    {
                        request.Status = newStatus;
                        
                        _unitOfWork.EmergencyRequests.Update(request);
                        await _unitOfWork.CompleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request status. RequestId: {RequestId}, RequestType: {RequestType}, NewStatus: {NewStatus}",
                    requestId, requestType, newStatus);
            }
        }

        #endregion
    }
}