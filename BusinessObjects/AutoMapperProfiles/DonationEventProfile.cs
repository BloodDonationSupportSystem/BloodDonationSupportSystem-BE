using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;

namespace BusinessObjects.AutoMapperProfiles
{
    public class DonationEventProfile : Profile
    {
        public DonationEventProfile()
        {
            CreateMap<DonationEvent, DonationEventDto>()
                .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src => 
                    src.DonorProfile != null ? 
                        $"{src.DonorProfile.User.FirstName} {src.DonorProfile.User.LastName}" : null))
                .ForMember(dest => dest.DonorEmail, opt => opt.MapFrom(src => 
                    src.DonorProfile != null && src.DonorProfile.User != null ? 
                        src.DonorProfile.User.Email : null))
                .ForMember(dest => dest.DonorPhone, opt => opt.MapFrom(src => 
                    src.DonorProfile != null && src.DonorProfile.User != null ? 
                        src.DonorProfile.User.PhoneNumber : null))
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => 
                    src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => 
                    src.ComponentType != null ? src.ComponentType.Name : null))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => 
                    src.Location != null ? src.Location.Name : null))
                .ForMember(dest => dest.LocationAddress, opt => opt.MapFrom(src => 
                    src.Location != null ? src.Location.Address : null))
                .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => 
                    src.Staff != null ? $"{src.Staff.FirstName} {src.Staff.LastName}" : null));

            // Mapping for creating a donation event
            CreateMap<CreateDonationEventDto, DonationEvent>();
            
            // Mapping for creating a walk-in donation event
            CreateMap<CreateWalkInDonationEventDto, DonationEvent>();
            
            // Mapping for updating a donation event
            CreateMap<UpdateDonationEventDto, DonationEvent>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}