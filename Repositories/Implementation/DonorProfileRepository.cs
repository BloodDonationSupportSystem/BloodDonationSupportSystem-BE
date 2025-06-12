using BusinessObjects.Data;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class DonorProfileRepository : GenericRepository<DonorProfile>, IDonorProfileRepository
    {
        public DonorProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DonorProfile> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .FirstOrDefaultAsync(dp => dp.Id == id);
        }

        public async Task<DonorProfile> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .FirstOrDefaultAsync(dp => dp.UserId == userId);
        }

        public async Task<IEnumerable<DonorProfile>> GetByBloodGroupIdAsync(Guid bloodGroupId)
        {
            return await _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .Where(dp => dp.BloodGroupId == bloodGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonorProfile>> GetDonorsByBloodGroupAsync(Guid bloodGroupId, bool onlyAvailable = true)
        {
            var query = _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .Where(dp => dp.BloodGroupId == bloodGroupId && dp.DeletedTime == null);

            if (onlyAvailable)
            {
                // Only get donors who are available for donation
                query = query.Where(dp =>
                    dp.IsAvailableForEmergency &&
                    (dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= DateTimeOffset.UtcNow));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> IsUserAlreadyDonorAsync(Guid userId)
        {
            return await _dbSet.AnyAsync(dp => dp.UserId == userId);
        }

        public async Task<(IEnumerable<DonorProfile> donorProfiles, int totalCount)> GetPagedDonorProfilesAsync(DonorProfileParameters parameters)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.BloodGroup))
            {
                query = query.Where(dp => dp.BloodGroup.GroupName.Contains(parameters.BloodGroup));
            }

            if (!string.IsNullOrEmpty(parameters.HealthStatus))
            {
                query = query.Where(dp => dp.HealthStatus.Contains(parameters.HealthStatus));
            }

            if (parameters.MinimumDonations.HasValue)
            {
                query = query.Where(dp => dp.TotalDonations >= parameters.MinimumDonations.Value);
            }

            // L?c theo tình tr?ng s?n sàng hi?n máu
            if (parameters.IsAvailableNow.HasValue && parameters.IsAvailableNow.Value)
            {
                var now = DateTimeOffset.UtcNow;
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= now);
            }

            if (parameters.IsAvailableForEmergency.HasValue)
            {
                query = query.Where(dp => dp.IsAvailableForEmergency == parameters.IsAvailableForEmergency.Value);
            }

            if (parameters.AvailableAfter.HasValue)
            {
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate >= parameters.AvailableAfter.Value);
            }

            if (parameters.AvailableBefore.HasValue)
            {
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= parameters.AvailableBefore.Value);
            }

            if (!string.IsNullOrEmpty(parameters.PreferredDonationTime))
            {
                query = query.Where(dp => dp.PreferredDonationTime.Contains(parameters.PreferredDonationTime));
            }

            // Filter soft deleted
            query = query.Where(dp => dp.DeletedTime == null);

            // Get total count
            var totalCount = await query.CountAsync();

            // Include related entities
            query = query.Include(dp => dp.User).Include(dp => dp.BloodGroup);

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "name" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.User.FirstName).ThenBy(dp => dp.User.LastName) 
                    : query.OrderByDescending(dp => dp.User.FirstName).ThenByDescending(dp => dp.User.LastName),
                "bloodgroup" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.BloodGroup.GroupName) 
                    : query.OrderByDescending(dp => dp.BloodGroup.GroupName),
                "donations" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.TotalDonations) 
                    : query.OrderByDescending(dp => dp.TotalDonations),
                "lastdonation" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.LastDonationDate) 
                    : query.OrderByDescending(dp => dp.LastDonationDate),
                "nextavailable" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.NextAvailableDonationDate) 
                    : query.OrderByDescending(dp => dp.NextAvailableDonationDate),
                _ => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.CreatedTime) 
                    : query.OrderByDescending(dp => dp.CreatedTime)
            };

            // Apply pagination
            var donorProfiles = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (donorProfiles, totalCount);
        }

        // Tri?n khai ph??ng th?c tìm ki?m ng??i hi?n máu ?ang s?n sàng
        public async Task<IEnumerable<DonorProfile>> GetAvailableDonorsAsync(DateTimeOffset? date = null, bool? forEmergency = null)
        {
            var query = _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .Where(dp => dp.DeletedTime == null);

            // N?u có ngày c? th?, ki?m tra ng??i hi?n máu s?n sàng vào ngày ?ó
            if (date.HasValue)
            {
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= date.Value);
            }
            else
            {
                // N?u không có ngày c? th?, ki?m tra ng??i hi?n máu s?n sàng hi?n t?i
                var now = DateTimeOffset.UtcNow;
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= now);
            }

            // N?u c?n l?c theo tình tr?ng s?n sàng kh?n c?p
            if (forEmergency.HasValue)
            {
                query = query.Where(dp => dp.IsAvailableForEmergency == forEmergency.Value);
            }

            return await query.ToListAsync();
        }

        // Tri?n khai ph??ng th?c c?p nh?t thông tin s?n sàng hi?n máu
        public async Task<bool> UpdateDonationAvailabilityAsync(Guid id, DateTimeOffset? nextAvailableDate, bool isAvailableForEmergency, string preferredTime)
        {
            var donorProfile = await _dbSet.FindAsync(id);
            if (donorProfile == null || donorProfile.DeletedTime != null)
            {
                return false;
            }

            donorProfile.NextAvailableDonationDate = nextAvailableDate;
            donorProfile.IsAvailableForEmergency = isAvailableForEmergency;
            donorProfile.PreferredDonationTime = preferredTime ?? string.Empty;
            donorProfile.LastUpdatedTime = DateTimeOffset.UtcNow;

            _context.Entry(donorProfile).State = EntityState.Modified;
            
            return true;
        }

        // Implement distance-based search methods
        public async Task<IEnumerable<DonorProfile>> GetDonorsByDistanceAsync(double latitude, double longitude, double radiusKm, Guid? bloodGroupId = null)
        {
            // Get all donor profiles
            var query = _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .Where(dp => dp.DeletedTime == null);

            // Filter by blood group if specified
            if (bloodGroupId.HasValue)
            {
                query = query.Where(dp => dp.BloodGroupId == bloodGroupId.Value);
            }

            // Get all donors
            var allDonors = await query.ToListAsync();

            // Filter donors by distance in memory
            var nearbyDonors = allDonors
                .Where(dp => !string.IsNullOrEmpty(dp.Latitude) && !string.IsNullOrEmpty(dp.Longitude))
                .Select(dp => new
                {
                    Donor = dp,
                    Distance = CalculateDistance(latitude, longitude, dp.Latitude, dp.Longitude)
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Donor)
                .ToList();

            return nearbyDonors;
        }

        public async Task<IEnumerable<DonorProfile>> GetAvailableDonorsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            DateTimeOffset? date = null, 
            bool? forEmergency = null, 
            Guid? bloodGroupId = null)
        {
            // Get all donor profiles
            var query = _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .Where(dp => dp.DeletedTime == null);

            // Filter by availability
            if (date.HasValue)
            {
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= date.Value);
            }
            else
            {
                var now = DateTimeOffset.UtcNow;
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= now);
            }

            // Filter by emergency availability
            if (forEmergency.HasValue)
            {
                query = query.Where(dp => dp.IsAvailableForEmergency == forEmergency.Value);
            }

            // Filter by blood group if specified
            if (bloodGroupId.HasValue)
            {
                query = query.Where(dp => dp.BloodGroupId == bloodGroupId.Value);
            }

            // Get all filtered donors
            var filteredDonors = await query.ToListAsync();

            // Filter donors by distance in memory
            var nearbyDonors = filteredDonors
                .Where(dp => !string.IsNullOrEmpty(dp.Latitude) && !string.IsNullOrEmpty(dp.Longitude))
                .Select(dp => new
                {
                    Donor = dp,
                    Distance = CalculateDistance(latitude, longitude, dp.Latitude, dp.Longitude)
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Donor)
                .ToList();

            return nearbyDonors;
        }

        public async Task<(IEnumerable<DonorProfile> donorProfiles, int totalCount)> GetPagedDonorsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            DonorProfileParameters parameters)
        {
            // Get all donor profiles with filters applied
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.BloodGroup))
            {
                query = query.Where(dp => dp.BloodGroup.GroupName.Contains(parameters.BloodGroup));
            }

            if (!string.IsNullOrEmpty(parameters.HealthStatus))
            {
                query = query.Where(dp => dp.HealthStatus.Contains(parameters.HealthStatus));
            }

            if (parameters.MinimumDonations.HasValue)
            {
                query = query.Where(dp => dp.TotalDonations >= parameters.MinimumDonations.Value);
            }

            if (parameters.IsAvailableNow.HasValue && parameters.IsAvailableNow.Value)
            {
                var now = DateTimeOffset.UtcNow;
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= now);
            }

            if (parameters.IsAvailableForEmergency.HasValue)
            {
                query = query.Where(dp => dp.IsAvailableForEmergency == parameters.IsAvailableForEmergency.Value);
            }

            if (parameters.AvailableAfter.HasValue)
            {
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate >= parameters.AvailableAfter.Value);
            }

            if (parameters.AvailableBefore.HasValue)
            {
                query = query.Where(dp => dp.NextAvailableDonationDate == null || dp.NextAvailableDonationDate <= parameters.AvailableBefore.Value);
            }

            if (!string.IsNullOrEmpty(parameters.PreferredDonationTime))
            {
                query = query.Where(dp => dp.PreferredDonationTime.Contains(parameters.PreferredDonationTime));
            }

            // Filter soft deleted
            query = query.Where(dp => dp.DeletedTime == null);

            // Include related entities
            query = query.Include(dp => dp.User).Include(dp => dp.BloodGroup);

            // Get all filtered donors
            var filteredDonors = await query.ToListAsync();

            // Filter and calculate distance in memory
            var nearbyDonors = filteredDonors
                .Where(dp => !string.IsNullOrEmpty(dp.Latitude) && !string.IsNullOrEmpty(dp.Longitude))
                .Select(dp => new
                {
                    Donor = dp,
                    Distance = CalculateDistance(latitude, longitude, dp.Latitude, dp.Longitude)
                })
                .Where(x => x.Distance <= radiusKm)
                .ToList();

            // Get total count for pagination
            var totalCount = nearbyDonors.Count;

            // Apply sorting based on parameters
            var sortedDonors = parameters.SortBy?.ToLower() switch
            {
                "name" => parameters.SortAscending 
                    ? nearbyDonors.OrderBy(x => x.Donor.User.FirstName).ThenBy(x => x.Donor.User.LastName) 
                    : nearbyDonors.OrderByDescending(x => x.Donor.User.FirstName).ThenByDescending(x => x.Donor.User.LastName),
                "bloodgroup" => parameters.SortAscending 
                    ? nearbyDonors.OrderBy(x => x.Donor.BloodGroup.GroupName) 
                    : nearbyDonors.OrderByDescending(x => x.Donor.BloodGroup.GroupName),
                "donations" => parameters.SortAscending 
                    ? nearbyDonors.OrderBy(x => x.Donor.TotalDonations) 
                    : nearbyDonors.OrderByDescending(x => x.Donor.TotalDonations),
                "lastdonation" => parameters.SortAscending 
                    ? nearbyDonors.OrderBy(x => x.Donor.LastDonationDate) 
                    : nearbyDonors.OrderByDescending(x => x.Donor.LastDonationDate),
                "nextavailable" => parameters.SortAscending 
                    ? nearbyDonors.OrderBy(x => x.Donor.NextAvailableDonationDate) 
                    : nearbyDonors.OrderByDescending(x => x.Donor.NextAvailableDonationDate),
                "distance" => parameters.SortAscending 
                    ? nearbyDonors.OrderBy(x => x.Distance) 
                    : nearbyDonors.OrderByDescending(x => x.Distance),
                _ => nearbyDonors.OrderBy(x => x.Distance) // Default sort by distance
            };

            // Apply pagination
            var pagedDonors = sortedDonors
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(x => x.Donor)
                .ToList();

            return (pagedDonors, totalCount);
        }

        // Helper method to calculate distance between two coordinates
        private double CalculateDistance(double lat1, double lon1, string lat2Str, string lon2Str)
        {
            if (double.TryParse(lat2Str, out double lat2) && double.TryParse(lon2Str, out double lon2))
            {
                return GeoCalculator.CalculateDistance(lat1, lon1, lat2, lon2);
            }
            return double.MaxValue; // Return maximum value for invalid coordinates
        }
    }
}