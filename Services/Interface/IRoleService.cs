using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IRoleService
    {
        Task<ApiResponse<IEnumerable<RoleDto>>> GetAllRolesAsync();
        Task<ApiResponse<RoleDto>> GetRoleByIdAsync(Guid id);
        Task<ApiResponse<RoleDto>> CreateRoleAsync(CreateRoleDto roleDto);
        Task<ApiResponse<RoleDto>> UpdateRoleAsync(Guid id, UpdateRoleDto roleDto);
        Task<ApiResponse> DeleteRoleAsync(Guid id);
    }
}