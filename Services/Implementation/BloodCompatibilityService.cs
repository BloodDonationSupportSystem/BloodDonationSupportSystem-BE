using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
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

        public BloodCompatibilityService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BloodCompatibilityService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleWholeBloodGroupsAsync(Guid recipientBloodGroupId)
        {
            try
            {
                // Ki?m tra nhóm máu ng??i nh?n có t?n t?i không
                var recipientBloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(recipientBloodGroupId);
                if (recipientBloodGroup == null)
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        HttpStatusCode.NotFound,
                        "Recipient blood group not found");
                }

                // L?y t?t c? nhóm máu
                var allBloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                
                // Xác ??nh nhóm máu t??ng thích d?a trên quy t?c t??ng thích máu toàn ph?n
                List<BloodGroup> compatibleBloodGroups = GetCompatibleWholeBlood(recipientBloodGroup.GroupName, allBloodGroups);
                
                var bloodGroupDtos = compatibleBloodGroups.Select(bg => _mapper.Map<BloodGroupDto>(bg)).ToList();
                
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    bloodGroupDtos,
                    $"Found {bloodGroupDtos.Count} compatible blood groups for whole blood transfusion to recipient with blood group {recipientBloodGroup.GroupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible whole blood groups for recipient blood group ID: {RecipientBloodGroupId}", recipientBloodGroupId);
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible whole blood groups");
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleComponentBloodGroupsAsync(Guid recipientBloodGroupId, Guid componentTypeId)
        {
            try
            {
                // Ki?m tra nhóm máu ng??i nh?n và lo?i thành ph?n máu có t?n t?i không
                var recipientBloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(recipientBloodGroupId);
                if (recipientBloodGroup == null)
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        HttpStatusCode.NotFound,
                        "Recipient blood group not found");
                }

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId);
                if (componentType == null)
                {
                    return new ApiResponse<IEnumerable<BloodGroupDto>>(
                        HttpStatusCode.NotFound,
                        "Component type not found");
                }

                // L?y t?t c? nhóm máu
                var allBloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                
                // Xác ??nh nhóm máu t??ng thích d?a trên lo?i thành ph?n máu
                List<BloodGroup> compatibleBloodGroups = new List<BloodGroup>();
                
                // Quy t?c t??ng thích cho t?ng lo?i thành ph?n:
                switch (componentType.Name.ToLower())
                {
                    case "red blood cells":
                    case "red cells":
                    case "rbc":
                    case "packed red blood cells":
                    case "prbc":
                        // H?ng c?u tuân theo quy t?c t??ng t? máu toàn ph?n
                        compatibleBloodGroups = GetCompatibleRedBloodCells(recipientBloodGroup.GroupName, allBloodGroups);
                        break;
                        
                    case "plasma":
                    case "fresh frozen plasma":
                    case "ffp":
                        // Huy?t t??ng tuân theo quy t?c ng??c v?i máu toàn ph?n
                        compatibleBloodGroups = GetCompatiblePlasma(recipientBloodGroup.GroupName, allBloodGroups);
                        break;
                        
                    case "platelets":
                    case "platelet concentrate":
                        // Ti?u c?u th??ng tuân theo quy t?c v? kháng nguyên ABO, nh?ng ít nghiêm ng?t h?n
                        compatibleBloodGroups = GetCompatiblePlatelets(recipientBloodGroup.GroupName, allBloodGroups);
                        break;
                        
                    case "cryoprecipitate":
                    case "cryo":
                        // Cryoprecipitate ít quan tr?ng v? ABO, nh?ng th??ng theo quy t?c t??ng t? huy?t t??ng
                        compatibleBloodGroups = GetCompatibleCryoprecipitate(recipientBloodGroup.GroupName, allBloodGroups);
                        break;
                        
                    case "whole blood":
                    default:
                        // Máu toàn ph?n ho?c tr??ng h?p m?c ??nh
                        compatibleBloodGroups = GetCompatibleWholeBlood(recipientBloodGroup.GroupName, allBloodGroups);
                        break;
                }
                
                var bloodGroupDtos = compatibleBloodGroups.Select(bg => _mapper.Map<BloodGroupDto>(bg)).ToList();
                
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    bloodGroupDtos,
                    $"Found {bloodGroupDtos.Count} compatible blood groups for {componentType.Name} transfusion to recipient with blood group {recipientBloodGroup.GroupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible component blood groups. RecipientBloodGroupId: {RecipientBloodGroupId}, ComponentTypeId: {ComponentTypeId}", 
                    recipientBloodGroupId, componentTypeId);
                return new ApiResponse<IEnumerable<BloodGroupDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible component blood groups");
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodGroupCompatibilityDto>>> GetBloodGroupCompatibilityMatrixAsync()
        {
            try
            {
                var bloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                var componentTypes = await _unitOfWork.ComponentTypes.GetAllAsync();
                
                List<BloodGroupCompatibilityDto> compatibilityMatrix = new List<BloodGroupCompatibilityDto>();
                
                foreach (var bloodGroup in bloodGroups)
                {
                    var compatibilityDto = new BloodGroupCompatibilityDto
                    {
                        BloodGroupId = bloodGroup.Id,
                        BloodGroupName = bloodGroup.GroupName,
                        CanDonateTo = new List<BloodGroupInfoDto>(),
                        CanReceiveFrom = new List<BloodGroupInfoDto>()
                    };
                    
                    // Xác ??nh nhóm máu mà nhóm máu này có th? hi?n cho (máu toàn ph?n)
                    var canDonateTo = GetCanDonateTo(bloodGroup.GroupName, bloodGroups);
                    compatibilityDto.CanDonateTo = canDonateTo.Select(bg => new BloodGroupInfoDto
                    {
                        BloodGroupId = bg.Id,
                        BloodGroupName = bg.GroupName
                    }).ToList();
                    
                    // Xác ??nh nhóm máu mà nhóm máu này có th? nh?n t? (máu toàn ph?n)
                    var canReceiveFrom = GetCompatibleWholeBlood(bloodGroup.GroupName, bloodGroups);
                    compatibilityDto.CanReceiveFrom = canReceiveFrom.Select(bg => new BloodGroupInfoDto
                    {
                        BloodGroupId = bg.Id,
                        BloodGroupName = bg.GroupName
                    }).ToList();
                    
                    // Thêm thông tin t??ng thích theo t?ng lo?i thành ph?n máu
                    compatibilityDto.ComponentCompatibility = new List<ComponentCompatibilityDto>();
                    foreach (var component in componentTypes)
                    {
                        List<BloodGroup> compatibleDonors = new List<BloodGroup>();
                        
                        switch (component.Name.ToLower())
                        {
                            case "red blood cells":
                            case "red cells":
                            case "rbc":
                            case "packed red blood cells":
                            case "prbc":
                                compatibleDonors = GetCompatibleRedBloodCells(bloodGroup.GroupName, bloodGroups);
                                break;
                                
                            case "plasma":
                            case "fresh frozen plasma":
                            case "ffp":
                                compatibleDonors = GetCompatiblePlasma(bloodGroup.GroupName, bloodGroups);
                                break;
                                
                            case "platelets":
                            case "platelet concentrate":
                                compatibleDonors = GetCompatiblePlatelets(bloodGroup.GroupName, bloodGroups);
                                break;
                                
                            case "cryoprecipitate":
                            case "cryo":
                                compatibleDonors = GetCompatibleCryoprecipitate(bloodGroup.GroupName, bloodGroups);
                                break;
                                
                            case "whole blood":
                            default:
                                compatibleDonors = GetCompatibleWholeBlood(bloodGroup.GroupName, bloodGroups);
                                break;
                        }
                        
                        compatibilityDto.ComponentCompatibility.Add(new ComponentCompatibilityDto
                        {
                            ComponentTypeId = component.Id,
                            ComponentTypeName = component.Name,
                            CompatibleDonors = compatibleDonors.Select(bg => new BloodGroupInfoDto
                            {
                                BloodGroupId = bg.Id,
                                BloodGroupName = bg.GroupName
                            }).ToList()
                        });
                    }
                    
                    compatibilityMatrix.Add(compatibilityDto);
                }
                
                return new ApiResponse<IEnumerable<BloodGroupCompatibilityDto>>(
                    compatibilityMatrix,
                    $"Retrieved blood group compatibility matrix with {compatibilityMatrix.Count} blood groups");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting blood group compatibility matrix");
                return new ApiResponse<IEnumerable<BloodGroupCompatibilityDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting blood group compatibility matrix");
            }
        }

        public async Task<ApiResponse<IEnumerable<Guid>>> GetCompatibleDonorBloodGroupsAsync(Guid recipientBloodGroupId)
        {
            try
            {
                var recipientBloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(recipientBloodGroupId);
                if (recipientBloodGroup == null)
                {
                    return new ApiResponse<IEnumerable<Guid>>(
                        HttpStatusCode.NotFound,
                        "Recipient blood group not found");
                }

                var allBloodGroups = await _unitOfWork.BloodGroups.GetAllAsync();
                var compatibleBloodGroups = GetCompatibleWholeBlood(recipientBloodGroup.GroupName, allBloodGroups);
                var compatibleIds = compatibleBloodGroups.Select(bg => bg.Id).ToList();

                return new ApiResponse<IEnumerable<Guid>>(
                    compatibleIds,
                    $"Found {compatibleIds.Count} compatible donor blood group IDs for recipient with blood group {recipientBloodGroup.GroupName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting compatible donor blood group IDs for recipient blood group ID: {RecipientBloodGroupId}", recipientBloodGroupId);
                return new ApiResponse<IEnumerable<Guid>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting compatible donor blood group IDs");
            }
        }

        #region Helper Methods

        private List<BloodGroup> GetCompatibleWholeBlood(string recipientBloodGroup, IEnumerable<BloodGroup> allBloodGroups)
        {
            switch (recipientBloodGroup.ToUpper())
            {
                case "O+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "O+" || bg.GroupName == "O-").ToList();
                case "O-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "O-").ToList();
                case "A+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A+" || bg.GroupName == "A-" || 
                        bg.GroupName == "O+" || bg.GroupName == "O-").ToList();
                case "A-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A-" || bg.GroupName == "O-").ToList();
                case "B+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B+" || bg.GroupName == "B-" || 
                        bg.GroupName == "O+" || bg.GroupName == "O-").ToList();
                case "B-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B-" || bg.GroupName == "O-").ToList();
                case "AB+":
                    return allBloodGroups.ToList(); // AB+ là ng??i nh?n toàn n?ng
                case "AB-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A-" || bg.GroupName == "B-" || 
                        bg.GroupName == "AB-" || bg.GroupName == "O-").ToList();
                default:
                    return new List<BloodGroup>();
            }
        }

        private List<BloodGroup> GetCompatibleRedBloodCells(string recipientBloodGroup, IEnumerable<BloodGroup> allBloodGroups)
        {
            // H?ng c?u tuân theo quy t?c t??ng t? máu toàn ph?n
            return GetCompatibleWholeBlood(recipientBloodGroup, allBloodGroups);
        }

        private List<BloodGroup> GetCompatiblePlasma(string recipientBloodGroup, IEnumerable<BloodGroup> allBloodGroups)
        {
            // Huy?t t??ng tuân theo quy t?c ng??c v?i máu toàn ph?n
            switch (recipientBloodGroup.ToUpper())
            {
                case "O+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "O+" || bg.GroupName == "O-" || 
                        bg.GroupName == "A+" || bg.GroupName == "A-" || 
                        bg.GroupName == "B+" || bg.GroupName == "B-" || 
                        bg.GroupName == "AB+" || bg.GroupName == "AB-").ToList();
                case "O-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "O-" || bg.GroupName == "A-" || 
                        bg.GroupName == "B-" || bg.GroupName == "AB-").ToList();
                case "A+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A+" || bg.GroupName == "A-" || 
                        bg.GroupName == "AB+" || bg.GroupName == "AB-").ToList();
                case "A-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A-" || bg.GroupName == "AB-").ToList();
                case "B+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B+" || bg.GroupName == "B-" || 
                        bg.GroupName == "AB+" || bg.GroupName == "AB-").ToList();
                case "B-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B-" || bg.GroupName == "AB-").ToList();
                case "AB+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "AB+" || bg.GroupName == "AB-").ToList();
                case "AB-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "AB-").ToList();
                default:
                    return new List<BloodGroup>();
            }
        }

        private List<BloodGroup> GetCompatiblePlatelets(string recipientBloodGroup, IEnumerable<BloodGroup> allBloodGroups)
        {
            // Ti?u c?u th??ng thích h?p nh?t khi phù h?p ABO, nh?ng có th? truy?n không ??ng nhóm n?u c?n
            // Thông th??ng, O có th? cho t?t c?, nh?ng t?t nh?t là cùng nhóm
            switch (recipientBloodGroup.ToUpper())
            {
                case "O+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "O+" || bg.GroupName == "O-").ToList();
                case "O-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "O-").ToList();
                case "A+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A+" || bg.GroupName == "A-" || 
                        bg.GroupName == "O+" || bg.GroupName == "O-").ToList();
                case "A-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A-" || bg.GroupName == "O-").ToList();
                case "B+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B+" || bg.GroupName == "B-" || 
                        bg.GroupName == "O+" || bg.GroupName == "O-").ToList();
                case "B-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B-" || bg.GroupName == "O-").ToList();
                case "AB+":
                    return allBloodGroups.ToList(); // AB+ là ng??i nh?n toàn n?ng
                case "AB-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A-" || bg.GroupName == "B-" || 
                        bg.GroupName == "AB-" || bg.GroupName == "O-").ToList();
                default:
                    return new List<BloodGroup>();
            }
        }

        private List<BloodGroup> GetCompatibleCryoprecipitate(string recipientBloodGroup, IEnumerable<BloodGroup> allBloodGroups)
        {
            // Cryoprecipitate th??ng ???c coi là không quan tr?ng ABO, nh?ng t?t nh?t là tuân theo t??ng thích ABO
            // Nó th??ng theo quy t?c t??ng t? huy?t t??ng
            return GetCompatiblePlasma(recipientBloodGroup, allBloodGroups);
        }

        private List<BloodGroup> GetCanDonateTo(string donorBloodGroup, IEnumerable<BloodGroup> allBloodGroups)
        {
            switch (donorBloodGroup.ToUpper())
            {
                case "O+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "O+" || bg.GroupName == "A+" || 
                        bg.GroupName == "B+" || bg.GroupName == "AB+").ToList();
                case "O-":
                    return allBloodGroups.ToList(); // O- là ng??i cho toàn n?ng
                case "A+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A+" || bg.GroupName == "AB+").ToList();
                case "A-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "A+" || bg.GroupName == "A-" || 
                        bg.GroupName == "AB+" || bg.GroupName == "AB-").ToList();
                case "B+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B+" || bg.GroupName == "AB+").ToList();
                case "B-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "B+" || bg.GroupName == "B-" || 
                        bg.GroupName == "AB+" || bg.GroupName == "AB-").ToList();
                case "AB+":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "AB+").ToList();
                case "AB-":
                    return allBloodGroups.Where(bg => 
                        bg.GroupName == "AB+" || bg.GroupName == "AB-").ToList();
                default:
                    return new List<BloodGroup>();
            }
        }

        #endregion
    }
}