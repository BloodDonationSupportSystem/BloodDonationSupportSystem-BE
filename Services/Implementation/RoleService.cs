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
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<RoleDto>>> GetAllRolesAsync()
        {
            try
            {
                var roles = await _unitOfWork.Roles.GetAllAsync();
                var roleDtos = roles.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<RoleDto>>(roleDtos)
                {
                    Message = $"Retrieved {roleDtos.Count} roles successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<RoleDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid id)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(id);
                
                if (role == null)
                    return new ApiResponse<RoleDto>(HttpStatusCode.NotFound, $"Role with ID {id} not found");

                return new ApiResponse<RoleDto>(MapToDto(role));
            }
            catch (Exception ex)
            {
                return new ApiResponse<RoleDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<RoleDto>> CreateRoleAsync(CreateRoleDto roleDto)
        {
            try
            {
                // Check for existing role with the same name
                var existingRole = await _unitOfWork.Roles.GetByNameAsync(roleDto.RoleName);
                if (existingRole != null)
                {
                    return new ApiResponse<RoleDto>(HttpStatusCode.Conflict, $"Role with name '{roleDto.RoleName}' already exists");
                }

                var role = new Role
                {
                    RoleName = roleDto.RoleName,
                    Description = roleDto.Description
                };

                await _unitOfWork.Roles.AddAsync(role);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<RoleDto>(MapToDto(role), "Role created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<RoleDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<RoleDto>> UpdateRoleAsync(Guid id, UpdateRoleDto roleDto)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(id);
                
                if (role == null)
                    return new ApiResponse<RoleDto>(HttpStatusCode.NotFound, $"Role with ID {id} not found");

                // Check if updating to a name that already exists (but not this role's name)
                if (role.RoleName != roleDto.RoleName)
                {
                    var existingRole = await _unitOfWork.Roles.GetByNameAsync(roleDto.RoleName);
                    if (existingRole != null)
                    {
                        return new ApiResponse<RoleDto>(HttpStatusCode.Conflict, $"Role with name '{roleDto.RoleName}' already exists");
                    }
                }

                role.RoleName = roleDto.RoleName;
                role.Description = roleDto.Description;

                _unitOfWork.Roles.Update(role);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<RoleDto>(MapToDto(role), "Role updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<RoleDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteRoleAsync(Guid id)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(id);
                
                if (role == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Role with ID {id} not found");

                _unitOfWork.Roles.Delete(role);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private RoleDto MapToDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Description = role.Description
            };
        }
    }
}