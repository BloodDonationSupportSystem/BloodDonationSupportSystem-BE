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
    public class BloodRequestRepository : GenericRepository<BloodRequest>, IBloodRequestRepository
    {
        public BloodRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<BloodRequest> items, int totalCount)> GetPagedBloodRequestsAsync(BloodRequestParameters parameters)
        {
            IQueryable<BloodRequest> query = _dbSet
                .Include(br => br.User)
                .Include(br => br.BloodGroup)
                .Include(br => br.ComponentType)
                .Include(br => br.Location);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(br => br.Status.ToLower() == parameters.Status.ToLower());
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(br => br.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(br => br.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.LocationId.HasValue)
            {
                query = query.Where(br => br.LocationId == parameters.LocationId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(br => br.RequestDate >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(br => br.RequestDate <= parameters.EndDate);
            }

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<BloodRequest>> GetBloodRequestsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            Guid? bloodGroupId = null, 
            string status = null)
        {
            // Create a base query with all necessary includes
            var query = _dbSet
                .Include(br => br.User)
                .Include(br => br.BloodGroup)
                .Include(br => br.ComponentType)
                .Include(br => br.Location)
                .AsQueryable();

            // Apply filters if provided
            if (bloodGroupId.HasValue)
            {
                query = query.Where(br => br.BloodGroupId == bloodGroupId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(br => br.Status.ToLower() == status.ToLower());
            }

            // Get all filtered requests with locations
            var requests = await query.ToListAsync();

            // Filter by distance in memory
            var nearbyRequests = requests
                .Where(br => br.Location != null && 
                            !string.IsNullOrEmpty(br.Location.Latitude) && 
                            !string.IsNullOrEmpty(br.Location.Longitude))
                .Select(br => new
                {
                    Request = br,
                    Distance = CalculateDistance(latitude, longitude, br.Location.Latitude, br.Location.Longitude)
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Select(x => x.Request)
                .ToList();

            return nearbyRequests;
        }

        public async Task<(IEnumerable<BloodRequest> requests, int totalCount)> GetPagedBloodRequestsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            BloodRequestParameters parameters)
        {
            // Create a base query with all necessary includes
            var query = _dbSet
                .Include(br => br.User)
                .Include(br => br.BloodGroup)
                .Include(br => br.ComponentType)
                .Include(br => br.Location)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(br => br.Status.ToLower() == parameters.Status.ToLower());
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(br => br.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(br => br.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.LocationId.HasValue)
            {
                query = query.Where(br => br.LocationId == parameters.LocationId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(br => br.RequestDate >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(br => br.RequestDate <= parameters.EndDate);
            }

            // Get all filtered requests
            var filteredRequests = await query.ToListAsync();

            // Filter by distance in memory
            var requestsWithDistance = filteredRequests
                .Where(br => br.Location != null && 
                            !string.IsNullOrEmpty(br.Location.Latitude) && 
                            !string.IsNullOrEmpty(br.Location.Longitude))
                .Select(br => new
                {
                    Request = br,
                    Distance = CalculateDistance(latitude, longitude, br.Location.Latitude, br.Location.Longitude)
                })
                .Where(x => x.Distance <= radiusKm)
                .ToList();

            // Get total count for pagination
            var totalCount = requestsWithDistance.Count;

            // Apply sorting
            var sortedRequests = parameters.SortBy?.ToLower() switch
            {
                "date" => parameters.SortAscending
                    ? requestsWithDistance.OrderBy(x => x.Request.RequestDate)
                    : requestsWithDistance.OrderByDescending(x => x.Request.RequestDate),
                
                "neededby" => parameters.SortAscending
                    ? requestsWithDistance.OrderBy(x => x.Request.NeededByDate)
                    : requestsWithDistance.OrderByDescending(x => x.Request.NeededByDate),
                
                "status" => parameters.SortAscending
                    ? requestsWithDistance.OrderBy(x => x.Request.Status)
                    : requestsWithDistance.OrderByDescending(x => x.Request.Status),
                
                "quantity" => parameters.SortAscending
                    ? requestsWithDistance.OrderBy(x => x.Request.QuantityUnits)
                    : requestsWithDistance.OrderByDescending(x => x.Request.QuantityUnits),
                
                "bloodgroup" => parameters.SortAscending
                    ? requestsWithDistance.OrderBy(x => x.Request.BloodGroup.GroupName)
                    : requestsWithDistance.OrderByDescending(x => x.Request.BloodGroup.GroupName),
                
                "location" => parameters.SortAscending
                    ? requestsWithDistance.OrderBy(x => x.Request.Location.Name)
                    : requestsWithDistance.OrderByDescending(x => x.Request.Location.Name),
                
                "distance" => parameters.SortAscending
                    ? requestsWithDistance.OrderBy(x => x.Distance)
                    : requestsWithDistance.OrderByDescending(x => x.Distance),
                
                _ => requestsWithDistance.OrderBy(x => x.Distance) // Default sort by distance
            };

            // Apply pagination
            var pagedRequests = sortedRequests
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(x => x.Request)
                .ToList();

            return (pagedRequests, totalCount);
        }

        private IQueryable<BloodRequest> ApplySorting(IQueryable<BloodRequest> query, BloodRequestParameters parameters)
        {
            // Default sort is by request date, newest first
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return parameters.SortAscending
                    ? query.OrderBy(br => br.RequestDate)
                    : query.OrderByDescending(br => br.RequestDate);
            }

            // Apply sorting based on the provided field
            return parameters.SortBy.ToLower() switch
            {
                "date" => parameters.SortAscending
                    ? query.OrderBy(br => br.RequestDate)
                    : query.OrderByDescending(br => br.RequestDate),
                
                "neededby" => parameters.SortAscending
                    ? query.OrderBy(br => br.NeededByDate)
                    : query.OrderByDescending(br => br.NeededByDate),
                
                "status" => parameters.SortAscending
                    ? query.OrderBy(br => br.Status)
                    : query.OrderByDescending(br => br.Status),
                
                "quantity" => parameters.SortAscending
                    ? query.OrderBy(br => br.QuantityUnits)
                    : query.OrderByDescending(br => br.QuantityUnits),
                
                "bloodgroup" => parameters.SortAscending
                    ? query.OrderBy(br => br.BloodGroup.GroupName)
                    : query.OrderByDescending(br => br.BloodGroup.GroupName),
                
                "location" => parameters.SortAscending
                    ? query.OrderBy(br => br.Location.Name)
                    : query.OrderByDescending(br => br.Location.Name),
                
                _ => parameters.SortAscending
                    ? query.OrderBy(br => br.RequestDate)
                    : query.OrderByDescending(br => br.RequestDate),
            };
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