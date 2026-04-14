using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    /// <summary>
    /// Interface for Role Permission Repository
    /// </summary>
    public interface IRolePermissionRepository
    {
        
        // Batch operations
        /// <summary>
        /// Add permissions to role
        /// </summary>
        /// <param name="roleId">The ID of the role to add permissions to</param>
        /// <param name="permissionIds">The IDs of the permissions to add</param>
        /// <returns>True if the permissions were added, false otherwise</returns>
        Task<bool> AddPermissionsToRoleAsync(long roleId, IReadOnlyList<long> permissionIds);

        /// <summary>
        /// Remove permissions from role
        /// </summary>
        /// <param name="roleId">The ID of the role to remove permissions from</param>
        /// <param name="permissionIds">The IDs of the permissions to remove</param>
        /// <returns>True if the permissions were removed, false otherwise</returns>
        Task<bool> RemovePermissionsFromRoleAsync(long roleId, IReadOnlyList<long> permissionIds);
        
        /// <summary>
        /// Remove all permissions from role
        /// </summary>
        /// <param name="roleId">The ID of the role to remove all permissions from</param>
        /// <returns>True if the permissions were removed, false otherwise</returns>
        Task<bool> RemoveAllPermissionsFromRoleAsync(long roleId);


        /// <summary>
        /// Get permissions of role ID
        /// </summary>
        /// <param name="roleId">The ID of the role to get permissions for</param>
        /// <returns>The permissions for the role</returns>
        Task<IEnumerable<PermissionEntity>> GetPermissionsOfRoleIdAsync(long roleId);

        /// <summary>
        /// Get roles of permission ID
        /// </summary>
        /// <param name="permissionId">The ID of the permission to get roles for</param>
        /// <returns>The roles for the permission</returns>
        Task<IEnumerable<RoleEntity>> GetRolesOfPermissionIdAsync(long permissionId);

        /// <summary>
        /// Check if role is assigned to any permission
        /// </summary>
        /// <param name="roleId">The ID of the role to check</param>
        /// <returns>True if the role is assigned to any permission, false otherwise</returns>
        Task<bool> RoleHasPermissionAsync(long roleId);

        /// <summary>
        /// Check if permission is assigned to any role
        /// </summary>
        /// <param name="permissionId">The ID of the permission to check</param>
        /// <returns>True if the permission is assigned to any role, false otherwise</returns>
        Task<bool> PermissionHasRoleAsync(long permissionId);
        
        //Task<IEnumerable<dynamic>> GetRolePermissionsPagedAsync(long lastId = 0, int pageSize = 100);
        
    }
}
