using BusinessObjects.Dtos;
using BusinessObjects.Models;
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
    public class ComponentTypeService : IComponentTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComponentTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<ComponentTypeDto>>> GetAllComponentTypesAsync()
        {
            try
            {
                var componentTypes = await _unitOfWork.ComponentTypes.GetAllAsync();
                var componentTypeDtos = componentTypes.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<ComponentTypeDto>>(componentTypeDtos)
                {
                    Message = $"Retrieved {componentTypeDtos.Count} component types successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<ComponentTypeDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<ComponentTypeDto>> GetComponentTypeByIdAsync(Guid id)
        {
            try
            {
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(id);
                
                if (componentType == null)
                    return new ApiResponse<ComponentTypeDto>(HttpStatusCode.NotFound, $"Component type with ID {id} not found");

                return new ApiResponse<ComponentTypeDto>(MapToDto(componentType));
            }
            catch (Exception ex)
            {
                return new ApiResponse<ComponentTypeDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<ComponentTypeDto>> CreateComponentTypeAsync(CreateComponentTypeDto componentTypeDto)
        {
            try
            {
                // Check for existing component type with the same name
                var existingComponentType = await _unitOfWork.ComponentTypes.GetByNameAsync(componentTypeDto.Name);
                if (existingComponentType != null)
                {
                    return new ApiResponse<ComponentTypeDto>(HttpStatusCode.Conflict, $"Component type with name '{componentTypeDto.Name}' already exists");
                }

                var componentType = new ComponentType
                {
                    Name = componentTypeDto.Name,
                    ShelfLifeDays = componentTypeDto.ShelfLifeDays
                };

                await _unitOfWork.ComponentTypes.AddAsync(componentType);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<ComponentTypeDto>(MapToDto(componentType), "Component type created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ComponentTypeDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<ComponentTypeDto>> UpdateComponentTypeAsync(Guid id, UpdateComponentTypeDto componentTypeDto)
        {
            try
            {
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(id);
                
                if (componentType == null)
                    return new ApiResponse<ComponentTypeDto>(HttpStatusCode.NotFound, $"Component type with ID {id} not found");

                // Check if updating to a name that already exists (but not this component type's name)
                if (componentType.Name != componentTypeDto.Name)
                {
                    var existingComponentType = await _unitOfWork.ComponentTypes.GetByNameAsync(componentTypeDto.Name);
                    if (existingComponentType != null)
                    {
                        return new ApiResponse<ComponentTypeDto>(HttpStatusCode.Conflict, $"Component type with name '{componentTypeDto.Name}' already exists");
                    }
                }

                componentType.Name = componentTypeDto.Name;
                componentType.ShelfLifeDays = componentTypeDto.ShelfLifeDays;

                _unitOfWork.ComponentTypes.Update(componentType);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<ComponentTypeDto>(MapToDto(componentType), "Component type updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<ComponentTypeDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteComponentTypeAsync(Guid id)
        {
            try
            {
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(id);
                
                if (componentType == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Component type with ID {id} not found");

                _unitOfWork.ComponentTypes.Delete(componentType);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private ComponentTypeDto MapToDto(ComponentType componentType)
        {
            return new ComponentTypeDto
            {
                Id = componentType.Id,
                Name = componentType.Name,
                ShelfLifeDays = componentType.ShelfLifeDays
            };
        }
    }
}