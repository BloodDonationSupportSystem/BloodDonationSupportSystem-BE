using AutoMapper;
using BusinessObjects.Dtos;
using Microsoft.Extensions.Logging;
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
    public class BloodCompatibilityService : IBloodCompatibilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BloodCompatibilityService> _logger;

        // Blood compatibility rules (based on standard medical guidelines)
        private static readonly Dictionary<string, List<string>> WholeBloodCompatibility = new Dictionary<string, List<string>>
        {
            // Recipients can receive from these donor blood groups
            { "O-", new List<string> { "O-" } },
            { "O+", new List<string> { "O-", "O+" } },
            { "A-", new List<string> { "O-", "A-" } },
            { "A+", new List<string> { "O-", "O+", "A-", "A+" } },
            { "B-", new List<string> { "O-", "B-" } },
            { "B+", new List<string> { "O-", "O+", "B-", "B+" } },
            { "AB-", new List<string> { "O-", "A-", "B-", "AB-" } },
            { "AB+", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } }
        };

        // Red blood cells compatibility
        private static readonly Dictionary<string, List<string>> RedCellsCompatibility = new Dictionary<string, List<string>>
        {
            // Same as whole blood
            { "O-", new List<string> { "O-" } },
            { "O+", new List<string> { "O-", "O+" } },
            { "A-", new List<string> { "O-", "A-" } },
            { "A+", new List<string> { "O-", "O+", "A-", "A+" } },
            { "B-", new List<string> { "O-", "B-" } },
            { "B+", new List<string> { "O-", "O+", "B-", "B+" } },
            { "AB-", new List<string> { "O-", "A-", "B-", "AB-" } },
            { "AB+", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } }
        };

        // Plasma compatibility (opposite of red cells)
        private static readonly Dictionary<string, List<string>> PlasmaCompatibility = new Dictionary<string, List<string>>
        {
            { "O-", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } },
            { "O+", new List<string> { "O+", "A+", "B+", "AB+" } },
            { "A-", new List<string> { "A-", "A+", "AB-", "AB+" } },
            { "A+", new List<string> { "A+", "AB+" } },
            { "B-", new List<string> { "B-", "B+", "AB-", "AB+" } },
            { "B+", new List<string> { "B+", "AB+" } },
            { "AB-", new List<string> { "AB-", "AB+" } },
            { "AB+", new List<string> { "AB+" } }
        };

        // Platelets compatibility
        private static readonly Dictionary<string, List<string>> PlateletsCompatibility = new Dictionary<string, List<string>>
        {
            // Generally compatible with ABO, but Rh is important
            { "O-", new List<string> { "O-", "A-", "B-", "AB-" } },
            { "O+", new List<string> { "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" } },
            { "A-", new List<string> { "A-", "AB-" } },
            { "A+", new List<string> { "A-", "A+", "AB-", "AB+" } },
            { "B-", new List<string> { "B-", "AB-" } },
            { "B+", new List<string> { "B-", "B+", "AB-", "AB+" } },
            { "AB-", new List<string> { "AB-" } },
            { "AB+", new List<string> { "AB-", "AB+" } }
        };

        private static readonly string RedCellsComponentName = "Red Blood Cells";
        private static readonly string PlasmaComponentName = "Plasma";
        private static readonly string PlateletsComponentName = "Platelets";

        public BloodCompatibilityService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BloodCompatibilityService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleBloodGroupsForWholeBloodAsync(Guid bloodGroupId)
        {
            try
            {
                // Get the recipient blood group
                var recipientBloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodGroupId);
                if (recipientBloodGroup == null)
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        HttpStatusCode.NotFound,
                        "Recipient blood group not found");
                }

                // Get all blood groups
                var allBloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                
                // Find compatible donor blood groups for whole blood
                if (!WholeBloodCompatibility.TryGetValue(recipientBloodGroup.GroupName, out var compatibleGroups))
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        new List<BloodGroupDto>(),
                        "No compatibility information found for this blood group");
                }

                var compatibleBloodGroups = allBloodGroups
                    .Where(bg => compatibleGroups.Contains(bg.GroupName))
                    .ToList();

                var result = _mapper.Map<IEnumerable<BloodGroupDto>>(compatibleBloodGroups);
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    result,
                    $"Found {result.Count()} compatible blood groups for whole blood transfusion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible blood groups for whole blood. Blood Group ID: {BloodGroupId}", bloodGroupId);
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible blood groups");
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleBloodGroupsForComponentAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            try
            {
                // Get the recipient blood group
                var recipientBloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodGroupId);
                if (recipientBloodGroup == null)
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        HttpStatusCode.NotFound,
                        "Recipient blood group not found");
                }

                // Get the component type
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId);
                if (componentType == null)
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        HttpStatusCode.NotFound,
                        "Component type not found");
                }

                // Get all blood groups
                var allBloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                
                // Select the appropriate compatibility dictionary based on component type
                Dictionary<string, List<string>> compatibilityDict;
                
                if (componentType.Name.Contains(RedCellsComponentName, StringComparison.OrdinalIgnoreCase))
                {
                    compatibilityDict = RedCellsCompatibility;
                }
                else if (componentType.Name.Contains(PlasmaComponentName, StringComparison.OrdinalIgnoreCase))
                {
                    compatibilityDict = PlasmaCompatibility;
                }
                else if (componentType.Name.Contains(PlateletsComponentName, StringComparison.OrdinalIgnoreCase))
                {
                    compatibilityDict = PlateletsCompatibility;
                }
                else
                {
                    // Default to whole blood compatibility if component type is not recognized
                    compatibilityDict = WholeBloodCompatibility;
                }

                // Find compatible donor blood groups for the specified component
                if (!compatibilityDict.TryGetValue(recipientBloodGroup.GroupName, out var compatibleGroups))
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        new List<BloodGroupDto>(),
                        "No compatibility information found for this blood group and component");
                }

                var compatibleBloodGroups = allBloodGroups
                    .Where(bg => compatibleGroups.Contains(bg.GroupName))
                    .ToList();

                var result = _mapper.Map<IEnumerable<BloodGroupDto>>(compatibleBloodGroups);
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    result,
                    $"Found {result.Count()} compatible blood groups for {componentType.Name} transfusion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible blood groups for component. Blood Group ID: {BloodGroupId}, Component Type ID: {ComponentTypeId}", 
                    bloodGroupId, componentTypeId);
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible blood groups for component");
            }
        }

        public async Task<ApiResponse<IEnumerable<Guid>>> GetCompatibleDonorBloodGroupsAsync(Guid recipientBloodGroupId)
        {
            try
            {
                // Get the recipient blood group
                var recipientBloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(recipientBloodGroupId);
                if (recipientBloodGroup == null)
                {
                    return new ApiResponse<IEnumerable<Guid>>(
                        HttpStatusCode.NotFound,
                        "Recipient blood group not found");
                }

                // Get all blood groups
                var allBloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                
                // Find compatible donor blood groups for whole blood
                if (!WholeBloodCompatibility.TryGetValue(recipientBloodGroup.GroupName, out var compatibleGroups))
                {
                    return new ApiResponse<IEnumerable<Guid>>(
                        new List<Guid>(),
                        "No compatibility information found for this blood group");
                }

                var compatibleBloodGroups = allBloodGroups
                    .Where(bg => compatibleGroups.Contains(bg.GroupName))
                    .Select(bg => bg.Id)
                    .ToList();

                return new ApiResponse<IEnumerable<Guid>>(
                    compatibleBloodGroups,
                    $"Found {compatibleBloodGroups.Count} compatible blood group IDs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible donor blood group IDs. Blood Group ID: {BloodGroupId}", recipientBloodGroupId);
                return new ApiResponse<IEnumerable<Guid>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible blood group IDs");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetCompatibleDonorsForWholeBloodAsync(Guid bloodGroupId, bool? emergencyOnly = false)
        {
            try
            {
                // First get compatible blood groups
                var compatibleGroupsResponse = await GetCompatibleBloodGroupsForWholeBloodAsync(bloodGroupId);
                if (!compatibleGroupsResponse.Success || compatibleGroupsResponse.Data == null)
                {
                    return new ApiResponse<IEnumerable<DonorProfileDto>>(
                        compatibleGroupsResponse.StatusCode,
                        compatibleGroupsResponse.Message);
                }

                // Get compatible donor profiles
                var compatibleBloodGroupIds = compatibleGroupsResponse.Data.Select(bg => bg.Id).ToList();
                var availableDonors = await _unitOfWork.DonorProfiles.GetAvailableDonorsAsync(null, emergencyOnly);
                
                // Filter by compatible blood groups
                var compatibleDonors = availableDonors
                    .Where(donor => compatibleBloodGroupIds.Contains(donor.BloodGroupId))
                    .ToList();

                var result = _mapper.Map<IEnumerable<DonorProfileDto>>(compatibleDonors);
                return new ApiResponse<IEnumerable<DonorProfileDto>>(
                    result,
                    $"Found {result.Count()} compatible donors for whole blood transfusion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible donors for whole blood. Blood Group ID: {BloodGroupId}", bloodGroupId);
                return new ApiResponse<IEnumerable<DonorProfileDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible donors");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetCompatibleDonorsForComponentAsync(Guid bloodGroupId, Guid componentTypeId, bool? emergencyOnly = false)
        {
            try
            {
                // First get compatible blood groups for the component
                var compatibleGroupsResponse = await GetCompatibleBloodGroupsForComponentAsync(bloodGroupId, componentTypeId);
                if (!compatibleGroupsResponse.Success || compatibleGroupsResponse.Data == null)
                {
                    return new ApiResponse<IEnumerable<DonorProfileDto>>(
                        compatibleGroupsResponse.StatusCode,
                        compatibleGroupsResponse.Message);
                }

                // Get compatible donor profiles
                var compatibleBloodGroupIds = compatibleGroupsResponse.Data.Select(bg => bg.Id).ToList();
                var availableDonors = await _unitOfWork.DonorProfiles.GetAvailableDonorsAsync(null, emergencyOnly);
                
                // Filter by compatible blood groups
                var compatibleDonors = availableDonors
                    .Where(donor => compatibleBloodGroupIds.Contains(donor.BloodGroupId))
                    .ToList();

                var result = _mapper.Map<IEnumerable<DonorProfileDto>>(compatibleDonors);

                // Get component name for the message
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId);
                string componentName = componentType?.Name ?? "specified component";

                return new ApiResponse<IEnumerable<DonorProfileDto>>(
                    result,
                    $"Found {result.Count()} compatible donors for {componentName} transfusion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible donors for component. Blood Group ID: {BloodGroupId}, Component Type ID: {ComponentTypeId}", 
                    bloodGroupId, componentTypeId);
                return new ApiResponse<IEnumerable<DonorProfileDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible donors for component");
            }
        }
    }
}