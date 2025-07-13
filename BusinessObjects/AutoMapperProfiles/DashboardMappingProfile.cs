using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using System;
using System.Linq;

namespace BusinessObjects.AutoMapperProfiles
{
    public class DashboardMappingProfile : Profile
    {
        public DashboardMappingProfile()
        {
            // Map BloodRequest to EmergencyBloodRequestDto
            CreateMap<BloodRequest, EmergencyBloodRequestDto>()
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null));

            // Map BloodInventory to BloodInventorySummaryDto (used in GetInventorySummaryAsync)
            CreateMap<BloodInventory, BloodInventorySummaryDto>()
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.Ignore()) // This is calculated in the service
                .ForMember(dest => dest.MinimumRecommended, opt => opt.Ignore()) // This is set in the service
                .ForMember(dest => dest.OptimalQuantity, opt => opt.Ignore()) // This is set in the service
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // This is calculated in the service
                .ForMember(dest => dest.AvailabilityPercentage, opt => opt.Ignore()); // This is calculated in the service

            // Map BloodInventory to CriticalInventoryDto (used in GetCriticalInventoryAsync)
            CreateMap<BloodInventory, CriticalInventoryDto>()
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null))
                .ForMember(dest => dest.AvailableQuantity, opt => opt.Ignore()) // This is calculated in the service
                .ForMember(dest => dest.MinimumRecommended, opt => opt.Ignore()) // This is set in the service
                .ForMember(dest => dest.CriticalityLevel, opt => opt.Ignore()) // This is set in the service
                .ForMember(dest => dest.EstimatedDaysUntilDepletion, opt => opt.Ignore()); // This is set in the service
                                                                                           // Map DonationEvent to DonationEventInfoDto
            CreateMap<DonationEvent, DonationEventInfoDto>();
        }
    }
}