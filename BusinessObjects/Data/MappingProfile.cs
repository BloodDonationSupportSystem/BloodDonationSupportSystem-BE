using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using System;

namespace BusinessObjects.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ========== DonorProfile mappings ==========
            CreateMap<DonorProfile, DonorProfileDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null));

            CreateMap<CreateDonorProfileDto, DonorProfile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore());

            CreateMap<UpdateDonorProfileDto, DonorProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore());

            CreateMap<UpdateDonationAvailabilityDto, DonorProfile>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ========== BloodGroup mappings ==========
            CreateMap<BloodGroup, BloodGroupDto>();
            CreateMap<CreateBloodGroupDto, BloodGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));

            CreateMap<UpdateBloodGroupDto, BloodGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // ========== User mappings ==========
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null));

            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore());

            // ========== Role mappings ==========
            CreateMap<Role, RoleDto>();
            CreateMap<CreateRoleDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));

            CreateMap<UpdateRoleDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // ========== DonationEvent mappings ==========
            CreateMap<DonationEvent, DonationEventDto>()
                .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src => src.DonorProfile != null ?
                    (src.DonorProfile.User != null ? $"{src.DonorProfile.User.FirstName} {src.DonorProfile.User.LastName}" : "Unknown")  : "Unknown"))
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));

            CreateMap<CreateDonationEventDto, DonationEvent>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DonorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore());

            CreateMap<UpdateDonationEventDto, DonationEvent>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DonorProfile, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore());

            // ========== BloodInventory mappings ==========
            CreateMap<BloodInventory, BloodInventoryDto>()
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null))
                .ForMember(dest => dest.DonationEventId, opt => opt.MapFrom(src => src.DonationEvent != null ? src.DonationEvent.Id : (Guid?)null));

            CreateMap<CreateBloodInventoryDto, BloodInventory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.DonationEvent, opt => opt.Ignore());

            CreateMap<UpdateBloodInventoryDto, BloodInventory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.DonationEvent, opt => opt.Ignore());

            // ========== BloodRequest mappings ==========
            CreateMap<BloodRequest, BloodRequestDto>()
                .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.User != null ? src.User.LastName : null))
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));

            CreateMap<CreateBloodRequestDto, BloodRequest>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore());

            CreateMap<UpdateBloodRequestDto, BloodRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore());

            // ========== EmergencyRequest mappings ==========
            CreateMap<EmergencyRequest, EmergencyRequestDto>()
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null));

            CreateMap<CreateEmergencyRequestDto, EmergencyRequest>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore());

            CreateMap<UpdateEmergencyRequestDto, EmergencyRequest>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore());

            // ========== BlogPost mappings ==========
            CreateMap<BlogPost, BlogPostDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.User != null ? src.User.LastName : null));

            CreateMap<CreateBlogPostDto, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateBlogPostDto, BlogPost>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // ========== Document mappings ==========
            CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null));

            CreateMap<CreateDocumentDto, Document>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateDocumentDto, Document>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // ========== ComponentType mappings ==========
            CreateMap<ComponentType, ComponentTypeDto>();
            CreateMap<CreateComponentTypeDto, ComponentType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.BloodInventories, opt => opt.Ignore())
                .ForMember(dest => dest.DonationEvents, opt => opt.Ignore());

            CreateMap<UpdateComponentTypeDto, ComponentType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BloodInventories, opt => opt.Ignore())
                .ForMember(dest => dest.DonationEvents, opt => opt.Ignore());

            // ========== Location mappings ==========
            CreateMap<Location, LocationDto>();
            CreateMap<CreateLocationDto, Location>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));

            CreateMap<UpdateLocationDto, Location>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // ========== Notification mappings ==========
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null));

            CreateMap<CreateNotificationDto, Notification>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateNotificationDto, Notification>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // ========== BloodDonationWorkflow mappings ==========
            CreateMap<BloodDonationWorkflow, DonationWorkflowDto>()
                .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src => src.Donor != null ? $"{src.Donor.User.FirstName} {src.Donor.User.LastName}" : null))
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => src.ComponentType != null ? src.ComponentType.Name : null));

            CreateMap<CreateDonationWorkflowDto, BloodDonationWorkflow>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.Donor, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore());

            CreateMap<UpdateDonationWorkflowDto, BloodDonationWorkflow>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.Donor, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore());

            // ========== DonorReminderSettings mappings ==========
            CreateMap<DonorReminderSettings, DonorReminderSettingsDto>()
                .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src =>
                    src.DonorProfile != null && src.DonorProfile.User != null ? $"{src.DonorProfile.User.FirstName} {src.DonorProfile.User.LastName}" : null));

            CreateMap<CreateDonorReminderSettingsDto, DonorReminderSettings>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastReminderSentTime, opt => opt.Ignore())
                .ForMember(dest => dest.DonorProfile, opt => opt.Ignore());

            CreateMap<UpdateDonorReminderSettingsDto, DonorReminderSettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastReminderSentTime, opt => opt.Ignore())
                .ForMember(dest => dest.DonorProfile, opt => opt.Ignore());

            // ========== DonationAppointmentRequest mappings ==========
            CreateMap<DonationAppointmentRequest, DonationAppointmentRequestDto>()
                .ForMember(dest => dest.DonorName, opt => opt.MapFrom(src => 
                    src.Donor != null && src.Donor.User != null ? $"{src.Donor.User.FirstName} {src.Donor.User.LastName}" : "Unknown"))
                .ForMember(dest => dest.DonorEmail, opt => opt.MapFrom(src => 
                    src.Donor != null && src.Donor.User != null ? src.Donor.User.Email : string.Empty))
                .ForMember(dest => dest.DonorPhone, opt => opt.MapFrom(src => 
                    src.Donor != null && src.Donor.User != null ? src.Donor.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => 
                    src.Location != null ? src.Location.Name : string.Empty))
                .ForMember(dest => dest.LocationAddress, opt => opt.MapFrom(src => 
                    src.Location != null ? src.Location.Address : string.Empty))
                .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(src => 
                    src.BloodGroup != null ? src.BloodGroup.GroupName : null))
                .ForMember(dest => dest.ComponentTypeName, opt => opt.MapFrom(src => 
                    src.ComponentType != null ? src.ComponentType.Name : null))
                .ForMember(dest => dest.InitiatedByUserName, opt => opt.MapFrom(src => 
                    src.InitiatedByUser != null ? $"{src.InitiatedByUser.FirstName} {src.InitiatedByUser.LastName}" : null))
                .ForMember(dest => dest.ReviewedByUserName, opt => opt.MapFrom(src => 
                    src.ReviewedByUser != null ? $"{src.ReviewedByUser.FirstName} {src.ReviewedByUser.LastName}" : null))
                .ForMember(dest => dest.ConfirmedLocationName, opt => opt.MapFrom(src => 
                    src.ConfirmedLocation != null ? src.ConfirmedLocation.Name : null));

            CreateMap<CreateDonorAppointmentRequestDto, DonationAppointmentRequest>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(src => "DonorInitiated"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.IsUrgent ? 2 : 1))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.Donor, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.InitiatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmedLocation, opt => opt.Ignore())
                .ForMember(dest => dest.Workflow, opt => opt.Ignore());

            CreateMap<CreateStaffAppointmentRequestDto, DonationAppointmentRequest>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.RequestType, opt => opt.MapFrom(src => "StaffInitiated"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"))
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedTime, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.MapFrom(src => 
                    src.AutoExpireHours.HasValue ? DateTimeOffset.UtcNow.AddHours(src.AutoExpireHours.Value) : (DateTimeOffset?)null))
                .ForMember(dest => dest.Donor, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForMember(dest => dest.BloodGroup, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentType, opt => opt.Ignore())
                .ForMember(dest => dest.InitiatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ReviewedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmedLocation, opt => opt.Ignore())
                .ForMember(dest => dest.Workflow, opt => opt.Ignore());

            CreateMap<UpdateAppointmentRequestDto, DonationAppointmentRequest>()
                .ForMember(dest => dest.LastUpdatedTime, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}