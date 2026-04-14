using Nghex.Identity.DTOs.Permissions;
using Nghex.Identity.DTOs.Roles;

namespace Nghex.Identity.Services.Interfaces
{
    /// <summary>
    /// Interface for Role service
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">The role ID</param>
        /// <returns>The role DTO</returns>
        Task<RoleDto?> GetByIdAsync(long id);

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <param name="isDisabled">Include disabled roles</param>
        /// <returns>The role DTOs</returns>
        Task<IEnumerable<RoleDto>> GetAllAsync(bool isDisabled);

        /// <summary>
        /// Create new role
        /// </summary>
        /// <param name="createDto">The role data to create</param>
        /// <returns>The created role DTO with ID</returns>
        Task<RoleDto> CreateAsync(CreateRoleDto createDto);

        /// <summary>
        /// Update role
        /// </summary>
        /// <param name="updateDto">The role data to update</param>
        /// <returns>True if updated, false otherwise</returns>
        Task<bool> UpdateAsync(UpdateRoleDto updateDto);

        /// <summary>
        /// Delete role (soft delete)
        /// </summary>
        /// <param name="id">The role ID</param>
        /// <param name="deletedBy">Who deleted the role</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteAsync(long id, string deletedBy);


        /// <summary>
        /// Check if role has permission
        /// </summary>
        /// <param name="roleId">The role ID</param>
        /// <returns>True if role has permissions, false otherwise</returns>
        Task<bool> RoleHasPermissionAsync(long roleId);
    }
}
