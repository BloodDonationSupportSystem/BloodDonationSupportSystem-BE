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
            var slotDefs = new List<(string TimeSlot, int StartHour, int EndHour)>();
            for (int hour = 7; hour < 19; hour++)
            {
                if (hour == 11 || hour == 12) continue; // nghỉ trưa
                slotDefs.Add((
                    TimeSlot: $"{hour:00}:00-{hour + 1:00}:00",
                    StartHour: hour,
                    EndHour: hour + 1
                ));
            }

            var created = new List<LocationCapacityDto>();
            var allCapacities = (await _unitOfWork.LocationCapacities.GetByLocationIdAsync(dto.LocationId)).ToList();

            for (var date = dto.EffectiveDate!.Value.Date; date <= dto.ExpiryDate!.Value.Date; date = date.AddDays(1))
            {
                var dayOfWeek = date.DayOfWeek;
                if (dayOfWeek == DayOfWeek.Sunday) continue;
                int start = (int)dto.StartDayOfWeek;
                int end = (int)dto.EndDayOfWeek;
                int current = (int)dayOfWeek;
                if (end < start) end += 7;
                if (current < start) current += 7;
                if (current < start || current > end) continue;

                foreach (var slot in slotDefs)
                {
                    var slotStart = DateTime.SpecifyKind(date.AddHours(slot.StartHour), DateTimeKind.Utc);
                    var slotEnd = DateTime.SpecifyKind(date.AddHours(slot.EndHour), DateTimeKind.Utc);

                    var existing = allCapacities.FirstOrDefault(x =>
                        x.DayOfWeek == dayOfWeek &&
                        x.TimeSlot == slot.TimeSlot &&
                        x.EffectiveDate == slotStart &&
                        x.ExpiryDate == slotEnd);

                    var createDto = new CreateLocationCapacityDto
                    {
                        LocationId = dto.LocationId,
                        TimeSlot = slot.TimeSlot,
                        TotalCapacity = dto.TotalCapacity,
                        DayOfWeek = dayOfWeek,
                        EffectiveDate = slotStart,
                        ExpiryDate = slotEnd,
                        Notes = dto.Notes,
                        IsActive = dto.IsActive
                    };

                    if (existing != null)
                    {
                        var updateDto = new UpdateLocationCapacityDto
                        {
                            TimeSlot = slot.TimeSlot,
                            TotalCapacity = dto.TotalCapacity,
                            DayOfWeek = dayOfWeek,
                            EffectiveDate = slotStart,
                            ExpiryDate = slotEnd,
                            Notes = dto.Notes,
                            IsActive = dto.IsActive
                        };
                        var updateResult = await UpdateAsync(existing.Id, updateDto);
                        if (updateResult.Success && updateResult.Data != null)
                            created.Add(updateResult.Data);
                    }
                    else
                    {
                        var createResult = await CreateAsync(createDto);
                        if (createResult.Success && createResult.Data != null)
                            created.Add(createResult.Data);
                    }
                }
            }

            return new ApiResponse<IEnumerable<LocationCapacityDto>>(created, "Created/updated slots successfully");
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