using Nghex.Identity.DTOs.Permissions;
using Nghex.Identity.DTOs.Roles;

namespace Nghex.Identity.Services.Interfaces
{
    /// <summary>
    /// Interface for Permission service
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Get permission by ID
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>The permission DTO</returns>
        Task<PermissionDto?> GetByIdAsync(long id);

        /// <summary>
        /// Get all permissions
        /// </summary>
        /// <returns>The permission DTOs</returns>
        Task<IEnumerable<PermissionDto>> GetAllAsync();

        /// <summary>
        /// Create new permission
        /// </summary>
        /// <param name="createDto">Permission data to create</param>
        /// <returns>The created permission DTO with ID</returns>
        Task<PermissionDto> CreateAsync(CreatePermissionDto createDto);

        /// <summary>
        /// Update permission
        /// </summary>
        /// <param name="updateDto">Permission data to update</param>
        /// <returns>True if the permission is updated, false otherwise</returns>
        Task<bool> UpdateAsync(UpdatePermissionDto updateDto);

        /// <summary>
        /// Delete permission (soft delete)
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <param name="deletedBy">Deleted by</param>
        /// <returns>True if the permission is deleted, false otherwise</returns>
        Task<bool> DeleteAsync(long id, string deletedBy);

        /// <summary>
        /// Get roles by permission
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>The role DTOs</returns>
        Task<IEnumerable<RoleDto>> GetRolesByPermissionAsync(long permissionId);

        /// <summary>
        /// Check if permission has role
        /// </summary>
        /// <param name="permissionId">Permission ID</param>
        /// <returns>True if the permission has role, false otherwise</returns>
        Task<bool> PermissionHasRoleAsync(long permissionId);
    }
}
