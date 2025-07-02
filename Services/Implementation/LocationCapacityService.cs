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
    public class LocationCapacityService : ILocationCapacityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LocationCapacityService> _logger;

        public LocationCapacityService(IUnitOfWork unitOfWork, ILogger<LocationCapacityService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<LocationCapacityDto>>> GetByLocationIdAsync(Guid locationId)
        {
            var capacities = await _unitOfWork.LocationCapacities.GetByLocationIdAsync(locationId);
            var dtos = capacities.Select(c => MapToDto(c));
            return new ApiResponse<IEnumerable<LocationCapacityDto>>(dtos);
        }

        public async Task<ApiResponse<IEnumerable<LocationCapacityDto>>> GetByLocationAndDateAsync(Guid locationId, DateTimeOffset date)
        {
            // Lấy tất cả capacity active của location cho ngày cụ thể
            var capacities = await _unitOfWork.LocationCapacities.GetActiveCapacitiesAsync(locationId, date.UtcDateTime);
            var dtos = capacities.Select(MapToDto);
            return new ApiResponse<IEnumerable<LocationCapacityDto>>(dtos);
        }

        public async Task<ApiResponse<LocationCapacityDto>> GetByIdAsync(Guid id)
        {
            var capacity = await _unitOfWork.LocationCapacities.GetByIdAsync(id);
            if (capacity == null)
                return new ApiResponse<LocationCapacityDto>(HttpStatusCode.NotFound, "Capacity not found");
            return new ApiResponse<LocationCapacityDto>(MapToDto(capacity));
        }

        public async Task<ApiResponse<LocationCapacityDto>> CreateAsync(CreateLocationCapacityDto dto)
        {
            var entity = new LocationCapacity
            {
                LocationId = dto.LocationId,
                TimeSlot = dto.TimeSlot,
                TotalCapacity = dto.TotalCapacity,
                DayOfWeek = dto.DayOfWeek,
                EffectiveDate = dto.EffectiveDate,
                ExpiryDate = dto.ExpiryDate,
                Notes = dto.Notes,
                IsActive = dto.IsActive
            };
            await _unitOfWork.LocationCapacities.AddAsync(entity);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<LocationCapacityDto>(MapToDto(entity), "Created successfully");
        }

        public async Task<ApiResponse<LocationCapacityDto>> UpdateAsync(Guid id, UpdateLocationCapacityDto dto)
        {
            var entity = await _unitOfWork.LocationCapacities.GetByIdAsync(id);
            if (entity == null)
                return new ApiResponse<LocationCapacityDto>(HttpStatusCode.NotFound, "Capacity not found");

            entity.TimeSlot = dto.TimeSlot;
            entity.TotalCapacity = dto.TotalCapacity;
            entity.DayOfWeek = dto.DayOfWeek;
            entity.EffectiveDate = dto.EffectiveDate;
            entity.ExpiryDate = dto.ExpiryDate;
            entity.Notes = dto.Notes;
            entity.IsActive = dto.IsActive;

            _unitOfWork.LocationCapacities.Update(entity);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<LocationCapacityDto>(MapToDto(entity), "Updated successfully");
        }

        public async Task<ApiResponse> DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.LocationCapacities.GetByIdAsync(id);
            if (entity == null)
                return new ApiResponse(HttpStatusCode.NotFound, "Capacity not found");
            _unitOfWork.LocationCapacities.Delete(entity);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse("Deleted successfully");
        }

        public async Task<ApiResponse<IEnumerable<LocationCapacityDto>>> CreateMultipleAsync(CreateMultipleLocationCapacityDto dto)
        {
            try
            {
                _logger.LogInformation("Creating multiple capacities for Location {LocationId} from {StartDate} to {EndDate}",
                    dto.LocationId, dto.EffectiveDate, dto.ExpiryDate);

                // Validate date range
                if (dto.EffectiveDate >= dto.ExpiryDate)
                {
                    return new ApiResponse<IEnumerable<LocationCapacityDto>>(HttpStatusCode.BadRequest,
                        "Effective date must be before expiry date");
                }

                // Define hourly slots with Vietnam time zone (UTC+7) to UTC conversion
                // Vietnam is UTC+7, so to get UTC time we subtract 7 hours
                // For example: 7AM Vietnam = 0AM UTC, 8AM Vietnam = 1AM UTC
                var hourlySlots = new List<(string TimeSlot, int VietnamHour, int UtcHour)>();

                // Morning slots (7AM-12PM Vietnam time)
                for (int hour = 7; hour < 12; hour++)
                {
                    // Convert Vietnam hour to UTC (subtract 7)
                    int utcHour = hour - 7;
                    if (utcHour < 0) utcHour += 24; // Handle day boundary crossing

                    hourlySlots.Add((
                        TimeSlot: "Morning",
                        VietnamHour: hour,
                        UtcHour: utcHour
                    ));
                }

                // Afternoon slots (1PM-5PM Vietnam time)
                for (int hour = 13; hour < 17; hour++)
                {
                    // Convert Vietnam hour to UTC (subtract 7)
                    int utcHour = hour - 7;

                    hourlySlots.Add((
                        TimeSlot: "Afternoon",
                        VietnamHour: hour,
                        UtcHour: utcHour
                    ));
                }

                // Evening slots (5PM-7PM Vietnam time)
                for (int hour = 17; hour < 19; hour++)
                {
                    // Convert Vietnam hour to UTC (subtract 7)
                    int utcHour = hour - 7;

                    hourlySlots.Add((
                        TimeSlot: "Evening",
                        VietnamHour: hour,
                        UtcHour: utcHour
                    ));
                }

                var created = new List<LocationCapacityDto>();
                var updated = new List<LocationCapacityDto>();
                var allCapacities = (await _unitOfWork.LocationCapacities.GetByLocationIdAsync(dto.LocationId)).ToList();

                // Process each date in the range
                for (var date = dto.EffectiveDate!.Value.Date; date <= dto.ExpiryDate!.Value.Date; date = date.AddDays(1))
                {
                    var dayOfWeek = date.DayOfWeek;

                    // Skip Sunday
                    if (dayOfWeek == DayOfWeek.Sunday) continue;

                    // Check if day is in the specified range
                    if (!IsDayInRange(dayOfWeek, dto.StartDayOfWeek, dto.EndDayOfWeek))
                        continue;

                    // Create capacity for each hourly slot
                    foreach (var slot in hourlySlots)
                    {
                        // We need to handle day boundary crossing for UTC time
                        // If a Vietnam hour converts to a negative UTC hour, we need to shift to the previous day
                        DateTime effectiveUtcDate = date;
                        if (slot.UtcHour < 0)
                        {
                            effectiveUtcDate = date.AddDays(-1);
                        }

                        // Create DateTimeOffset objects with proper UTC time
                        var effectiveDate = new DateTimeOffset(
                            effectiveUtcDate.Year,
                            effectiveUtcDate.Month,
                            effectiveUtcDate.Day,
                            (slot.UtcHour + 24) % 24, // Ensure hour is 0-23 range
                            0, 0, 0,
                            TimeSpan.Zero); // UTC offset

                        var expiryDate = effectiveDate.AddHours(1); // Always 1 hour later

                        _logger.LogInformation(
                            "Creating slot: {TimeSlot}, Vietnam time: {VNHour}:00-{VNHour2}:00, UTC time: {EffectiveDate} to {ExpiryDate}",
                            slot.TimeSlot,
                            slot.VietnamHour,
                            slot.VietnamHour + 1,
                            effectiveDate.ToString("o"),
                            expiryDate.ToString("o"));

                        // Check if capacity already exists for this specific time slot
                        var existing = allCapacities.FirstOrDefault(x =>
                            x.DayOfWeek == dayOfWeek &&
                            x.TimeSlot == slot.TimeSlot &&
                            x.EffectiveDate?.Hour == effectiveDate.Hour &&
                            x.EffectiveDate?.Day == effectiveDate.Day &&
                            x.EffectiveDate?.Month == effectiveDate.Month &&
                            x.EffectiveDate?.Year == effectiveDate.Year);

                        if (existing != null)
                        {
                            // Update existing capacity
                            var updateDto = new UpdateLocationCapacityDto
                            {
                                TimeSlot = slot.TimeSlot,
                                TotalCapacity = dto.TotalCapacity,
                                DayOfWeek = dayOfWeek,
                                EffectiveDate = effectiveDate,
                                ExpiryDate = expiryDate,
                                Notes = dto.Notes,
                                IsActive = dto.IsActive
                            };

                            var updateResult = await UpdateAsync(existing.Id, updateDto);
                            if (updateResult.Success && updateResult.Data != null)
                            {
                                updated.Add(updateResult.Data);
                            }
                        }
                        else
                        {
                            // Create new capacity
                            var createDto = new CreateLocationCapacityDto
                            {
                                LocationId = dto.LocationId,
                                TimeSlot = slot.TimeSlot,
                                TotalCapacity = dto.TotalCapacity,
                                DayOfWeek = dayOfWeek,
                                EffectiveDate = effectiveDate,
                                ExpiryDate = expiryDate,
                                Notes = dto.Notes,
                                IsActive = dto.IsActive
                            };

                            var createResult = await CreateAsync(createDto);
                            if (createResult.Success && createResult.Data != null)
                            {
                                created.Add(createResult.Data);
                            }
                        }
                    }
                }

                var allResults = created.Concat(updated).ToList();
                var message = $"Successfully processed {allResults.Count} capacity slots - {created.Count} created, {updated.Count} updated";

                _logger.LogInformation("Bulk capacity creation completed: {Message}", message);

                return new ApiResponse<IEnumerable<LocationCapacityDto>>(allResults, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating multiple capacities for Location {LocationId}", dto.LocationId);
                return new ApiResponse<IEnumerable<LocationCapacityDto>>(HttpStatusCode.InternalServerError,
                    "An error occurred while creating capacities");
            }
        }

        private bool IsDayInRange(DayOfWeek current, DayOfWeek start, DayOfWeek end)
        {
            int currentInt = (int)current;
            int startInt = (int)start;
            int endInt = (int)end;

            // Handle week wrap-around (e.g., Saturday to Monday)
            if (endInt < startInt)
            {
                return currentInt >= startInt || currentInt <= endInt;
            }
            
            return currentInt >= startInt && currentInt <= endInt;
        }

        private LocationCapacityDto MapToDto(LocationCapacity c)
        {
            return new LocationCapacityDto
            {
                Id = c.Id,
                LocationId = c.LocationId,
                TimeSlot = c.TimeSlot,
                TotalCapacity = c.TotalCapacity,
                DayOfWeek = c.DayOfWeek,
                EffectiveDate = c.EffectiveDate,
                ExpiryDate = c.ExpiryDate,
                IsActive = c.IsActive,
                Notes = c.Notes,
                CreatedTime = c.CreatedTime,
                LastUpdatedTime = c.LastUpdatedTime
            };
        }
    }
}