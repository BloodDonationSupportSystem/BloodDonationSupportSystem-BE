using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Data;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using Shared.Utilities;

namespace Repositories.Implementation
{
    public class EmergencyRequestRepository : GenericRepository<EmergencyRequest>, IEmergencyRequestRepository
    {
        public EmergencyRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<EmergencyRequest> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .Include(er => er.Location)
                .FirstOrDefaultAsync(er => er.Id == id);
        }

        public async Task<EmergencyRequest> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .Include(er => er.Location)
                .FirstOrDefaultAsync(er => er.Id == id);
        }

        public async Task<(IEnumerable<EmergencyRequest> items, int totalCount)> GetPagedEmergencyRequestsAsync(EmergencyRequestParameters parameters)
        {
            IQueryable<EmergencyRequest> query = _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .Include(er => er.Location);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(er => er.Status.ToLower() == parameters.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(parameters.UrgencyLevel))
            {
                query = query.Where(er => er.UrgencyLevel.ToLower() == parameters.UrgencyLevel.ToLower());
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(er => er.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(er => er.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(er => er.RequestDate >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(er => er.RequestDate <= parameters.EndDate);
            }

            if (parameters.IsActive.HasValue)
            {
                query = query.Where(er => er.IsActive == parameters.IsActive.Value);
            }

            // Apply location-based filtering if provided
            if (parameters.Latitude.HasValue && parameters.Longitude.HasValue && parameters.RadiusKm.HasValue)
            {
                // We can't filter by distance directly in the query, so we'll fetch all matching records
                // and filter them in memory
                var allRequests = await query.ToListAsync();
                var requestsWithDistance = allRequests
                    .Where(er => HasValidCoordinates(er))
                    .Select(er => new RequestWithDistance
                    {
                        Request = er,
                        Distance = CalculateDistance(
                            parameters.Latitude.Value,
                            parameters.Longitude.Value,
                            GetLatitude(er),
                            GetLongitude(er))
                    })
                    .Where(x => x.Distance <= parameters.RadiusKm.Value)
                    .OrderBy(x => x.Distance)
                    .ToList();

                // Get total count
                var totalCount = requestsWithDistance.Count;

                // Apply sorting to the in-memory collection
                var sortedRequests = ApplySortingInMemory(requestsWithDistance, parameters);

                // Apply pagination to the in-memory collection
                var pagedRequests = sortedRequests
                    .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .Select(x => x.Request)
                    .ToList();

                return (pagedRequests, totalCount);
            }

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Get total count
            var queryTotalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (items, queryTotalCount);
        }

        public async Task<IEnumerable<EmergencyRequest>> GetEmergencyRequestsByDistanceAsync(
            double latitude,
            double longitude,
            double radiusKm,
            Guid? bloodGroupId = null,
            string urgencyLevel = null,
            bool? isActive = null)
        {
            // Create base query with all necessary includes
            var query = _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .Include(er => er.Location)
                .AsQueryable();

            // Apply filters if provided
            if (bloodGroupId.HasValue)
            {
                query = query.Where(er => er.BloodGroupId == bloodGroupId.Value);
            }

            if (!string.IsNullOrEmpty(urgencyLevel))
            {
                query = query.Where(er => er.UrgencyLevel.ToLower() == urgencyLevel.ToLower());
            }

            if (isActive.HasValue)
            {
                query = query.Where(er => er.IsActive == isActive.Value);
            }

            // Get all filtered requests
            var requests = await query.ToListAsync();

            // Filter by distance in memory and sort by distance
            var nearbyRequests = requests
                .Where(er => HasValidCoordinates(er))
                .Select(er => new RequestWithDistance
                {
                    Request = er,
                    Distance = CalculateDistance(latitude, longitude, GetLatitude(er), GetLongitude(er))
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Request)
                .ToList();

            return nearbyRequests;
        }

        public async Task<(IEnumerable<EmergencyRequest> requests, int totalCount)> GetPagedEmergencyRequestsByDistanceAsync(
            double latitude,
            double longitude,
            double radiusKm,
            EmergencyRequestParameters parameters)
        {
            // Create base query with all necessary includes
            var query = _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .Include(er => er.Location)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(er => er.Status.ToLower() == parameters.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(parameters.UrgencyLevel))
            {
                query = query.Where(er => er.UrgencyLevel.ToLower() == parameters.UrgencyLevel.ToLower());
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(er => er.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(er => er.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(er => er.RequestDate >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(er => er.RequestDate <= parameters.EndDate);
            }

            if (parameters.IsActive.HasValue)
            {
                query = query.Where(er => er.IsActive == parameters.IsActive.Value);
            }

            // Get all filtered requests
            var filteredRequests = await query.ToListAsync();

            // Filter by distance in memory
            var requestsWithDistance = filteredRequests
                .Where(er => HasValidCoordinates(er))
                .Select(er => new RequestWithDistance
                {
                    Request = er,
                    Distance = CalculateDistance(latitude, longitude, GetLatitude(er), GetLongitude(er))
                })
                .Where(x => x.Distance <= radiusKm)
                .ToList();

            // Get total count for pagination
            var totalCount = requestsWithDistance.Count;

            // Apply sorting
            var sortedRequests = ApplySortingInMemory(requestsWithDistance, parameters);

            // Apply pagination
            var pagedRequests = sortedRequests
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(x => x.Request)
                .ToList();

            return (pagedRequests, totalCount);
        }

        public async Task<IEnumerable<EmergencyRequest>> GetActiveEmergencyRequestsAsync(Guid? bloodGroupId = null)
        {
            var query = _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .Include(er => er.Location)
                .Where(er => er.IsActive && er.DeletedTime == null);

            if (bloodGroupId.HasValue)
            {
                query = query.Where(er => er.BloodGroupId == bloodGroupId.Value);
            }

            return await query
                .OrderByDescending(er => er.UrgencyLevel)
                .ThenByDescending(er => er.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyRequest>> GetEmergencyRequestsByBloodGroupAsync(Guid bloodGroupId, bool onlyActive = true)
        {
            var query = _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .Include(er => er.Location)
                .Where(er => er.BloodGroupId == bloodGroupId && er.DeletedTime == null);

            if (onlyActive)
            {
                query = query.Where(er => er.IsActive);
            }

            return await query
                .OrderByDescending(er => er.UrgencyLevel)
                .ThenByDescending(er => er.RequestDate)
                .ToListAsync();
        }

        private IQueryable<EmergencyRequest> ApplySorting(IQueryable<EmergencyRequest> query, EmergencyRequestParameters parameters)
        {
            // Default sort is by urgency and date, newest first
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return query.OrderByDescending(er => er.UrgencyLevel)
                            .ThenByDescending(er => er.RequestDate);
            }

            // Apply sorting based on the provided field
            return parameters.SortBy.ToLower() switch
            {
                "date" => parameters.SortAscending
                    ? query.OrderBy(er => er.RequestDate)
                    : query.OrderByDescending(er => er.RequestDate),

                "urgency" => parameters.SortAscending
                    ? query.OrderBy(er => er.UrgencyLevel)
                    : query.OrderByDescending(er => er.UrgencyLevel),

                "status" => parameters.SortAscending
                    ? query.OrderBy(er => er.Status)
                    : query.OrderByDescending(er => er.Status),

                "quantity" => parameters.SortAscending
                    ? query.OrderBy(er => er.QuantityUnits)
                    : query.OrderByDescending(er => er.QuantityUnits),

                "bloodgroup" => parameters.SortAscending
                    ? query.OrderBy(er => er.BloodGroup.GroupName)
                    : query.OrderByDescending(er => er.BloodGroup.GroupName),

                "componenttype" => parameters.SortAscending
                    ? query.OrderBy(er => er.ComponentType.Name)
                    : query.OrderByDescending(er => er.ComponentType.Name),

                "patient" => parameters.SortAscending
                    ? query.OrderBy(er => er.PatientName)
                    : query.OrderByDescending(er => er.PatientName),

                _ => query.OrderByDescending(er => er.UrgencyLevel)
                        .ThenByDescending(er => er.RequestDate),
            };
        }

        private IEnumerable<RequestWithDistance> ApplySortingInMemory(IEnumerable<RequestWithDistance> items, EmergencyRequestParameters parameters)
        {
            // Default sort is by distance
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return items.OrderBy(x => x.Distance);
            }

            // Apply sorting based on the provided field
            return parameters.SortBy.ToLower() switch
            {
                "date" => parameters.SortAscending
                    ? items.OrderBy(x => x.Request.RequestDate)
                    : items.OrderByDescending(x => x.Request.RequestDate),

                "urgency" => parameters.SortAscending
                    ? items.OrderBy(x => x.Request.UrgencyLevel)
                    : items.OrderByDescending(x => x.Request.UrgencyLevel),

                "status" => parameters.SortAscending
                    ? items.OrderBy(x => x.Request.Status)
                    : items.OrderByDescending(x => x.Request.Status),

                "quantity" => parameters.SortAscending
                    ? items.OrderBy(x => x.Request.QuantityUnits)
                    : items.OrderByDescending(x => x.Request.QuantityUnits),

                "bloodgroup" => parameters.SortAscending
                    ? items.OrderBy(x => x.Request.BloodGroup.GroupName)
                    : items.OrderByDescending(x => x.Request.BloodGroup.GroupName),

                "componenttype" => parameters.SortAscending
                    ? items.OrderBy(x => x.Request.ComponentType.Name)
                    : items.OrderByDescending(x => x.Request.ComponentType.Name),

                "patient" => parameters.SortAscending
                    ? items.OrderBy(x => x.Request.PatientName)
                    : items.OrderByDescending(x => x.Request.PatientName),

                "distance" => parameters.SortAscending
                    ? items.OrderBy(x => x.Distance)
                    : items.OrderByDescending(x => x.Distance),

                _ => items.OrderBy(x => x.Distance),
            };
        }

        // Helper methods for location calculations
        private bool HasValidCoordinates(EmergencyRequest request)
        {
            // Check if the request has valid coordinates either directly or through location
            return (!string.IsNullOrEmpty(request.Latitude) && !string.IsNullOrEmpty(request.Longitude)) ||
                   (request.Location != null && !string.IsNullOrEmpty(request.Location.Latitude) && !string.IsNullOrEmpty(request.Location.Longitude));
        }

        private double GetLatitude(EmergencyRequest request)
        {
            // Get latitude from either direct property or location
            if (!string.IsNullOrEmpty(request.Latitude) && double.TryParse(request.Latitude, out double lat))
            {
                return lat;
            }
            else if (request.Location != null && !string.IsNullOrEmpty(request.Location.Latitude) &&
                     double.TryParse(request.Location.Latitude, out double locLat))
            {
                return locLat;
            }
            return 0;
        }

        private double GetLongitude(EmergencyRequest request)
        {
            // Get longitude from either direct property or location
            if (!string.IsNullOrEmpty(request.Longitude) && double.TryParse(request.Longitude, out double lon))
            {
                return lon;
            }
            else if (request.Location != null && !string.IsNullOrEmpty(request.Location.Longitude) &&
                     double.TryParse(request.Location.Longitude, out double locLon))
            {
                return locLon;
            }
            return 0;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            return GeoCalculator.CalculateDistance(lat1, lon1, lat2, lon2);
        }
        
        // Helper class to store emergency request with its distance
        private class RequestWithDistance
        {
            public EmergencyRequest Request { get; set; }
            public double Distance { get; set; }
        }
    }
}