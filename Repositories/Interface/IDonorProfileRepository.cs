using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IDonorProfileRepository : IGenericRepository<DonorProfile>
    {
        Task<DonorProfile> GetByIdWithDetailsAsync(Guid id);
        Task<DonorProfile> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<DonorProfile>> GetByBloodGroupIdAsync(Guid bloodGroupId);
        Task<bool> IsUserAlreadyDonorAsync(Guid userId);
        Task<(IEnumerable<DonorProfile> donorProfiles, int totalCount)> GetPagedDonorProfilesAsync(DonorProfileParameters parameters);
        Task<IEnumerable<DonorProfile>> GetAvailableDonorsAsync(DateTimeOffset? date = null, bool? forEmergency = null);
        Task<bool> UpdateDonationAvailabilityAsync(Guid id, DateTimeOffset? nextAvailableDate, bool isAvailableForEmergency, string preferredTime);
        
        // New methods for distance-based searches
        Task<IEnumerable<DonorProfile>> GetDonorsByDistanceAsync(double latitude, double longitude, double radiusKm, Guid? bloodGroupId = null);
        Task<IEnumerable<DonorProfile>> GetAvailableDonorsByDistanceAsync(double latitude, double longitude, double radiusKm, DateTimeOffset? date = null, bool? forEmergency = null, Guid? bloodGroupId = null);
        Task<(IEnumerable<DonorProfile> donorProfiles, int totalCount)> GetPagedDonorsByDistanceAsync(double latitude, double longitude, double radiusKm, DonorProfileParameters parameters);
        
        // Method for emergency requests
        Task<IEnumerable<DonorProfile>> GetDonorsByBloodGroupAsync(Guid bloodGroupId, bool onlyAvailable = true);
    }
}