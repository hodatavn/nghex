using Nghex.Identity.DTOs.Permissions;
using Nghex.Identity.DTOs.Roles;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IAuthManagementService
    {


        #region Account - Role - Permission Management

        /// <summary>
        /// Assign roles to an account
        /// </summary>
        /// <param name="accountId">The ID of the account to assign roles to</param>
        /// <param name="roleIds">The IDs of the roles to assign to the account</param>
        /// <returns>True if the roles were assigned, false otherwise</returns>
        Task<bool> AssignRolesToAccountAsync(long accountId, IEnumerable<long>? roleIds);

        /// <summary>
        /// Remove roles from an account
        /// </summary>
        /// <param name="accountId">The ID of the account to remove roles from</param>
        /// <param name="roleIds">The IDs of the roles to remove from the account</param>
        /// <returns>True if the roles were removed, false otherwise</returns>
        Task<bool> RemoveRolesFromAccountAsync(long accountId, IEnumerable<long> roleIds);

        /// <summary>
        /// Remove all roles from an account
        /// </summary>
        /// <param name="accountId">The ID of the account to remove all roles from</param>
        /// <returns>True if the roles were removed, false otherwise</returns>
        Task<bool> RemoveAllRolesFromAccountAsync(long accountId);


        #endregion
    
        #region Role - Permission

        /// <summary>
        /// Get roles of an account
        /// </summary>
        /// <param name="accountId">The ID of the account to get roles for</param>
        /// <returns>The role DTOs</returns>
        Task<IEnumerable<RoleDto>> GetRolesOfAccountAsync(long accountId);

        /// <summary>
        /// Get permissions of an account
        /// </summary>
        /// <param name="accountId">The ID of the account to get permissions for</param>
        /// <returns>The permission DTOs</returns>
        Task<IEnumerable<PermissionDto>> GetPermissionsOfAccountAsync(long accountId);
        /// <summary>
        /// Grant permissions to a role
        /// </summary>
        /// <param name="roleId">The role ID</param>
        /// <param name="permissionIds">The permission IDs to assign</param>
        /// <returns>True if updated, false otherwise</returns>
        Task<bool> GrantPermissionsToRoleAsync(long roleId, IEnumerable<long>? permissionIds);

        /// <summary>
        /// Get permissions by role
        /// </summary>
        /// <param name="roleId">The role ID</param>
        /// <returns>The permission DTOs</returns>
        Task<IEnumerable<PermissionDto>> GetPermissionsOfRoleAsync(long roleId);
        #endregion


        #region MenuItem-Permission Relationship (MenuItem is the main entity)

        /// <summary>
        /// Set permissions for a menu
        /// </summary>
        /// <param name="menuKey">The key of the menu to set permissions for</param>
        /// <param name="permissionCodes">The codes of the permissions to set</param>
        /// <returns>True if the permissions were set, false otherwise</returns>
        Task<bool> SetPermissionsOnMenuAsync(string menuKey, IEnumerable<string>? permissionCodes);

        /// <summary>
        /// Remove permissions from a menu
        /// </summary>
        /// <param name="menuKey">The key of the menu to remove permissions from</param>
        /// <param name="permissionCodes">The codes of the permissions to remove</param>
        /// <returns>True if the permissions were removed, false otherwise</returns>
        Task<bool> RemovePermissionsFromMenuAsync(string menuKey, IEnumerable<string> permissionCodes);

        /// <summary>
        /// Get permissions of a menu
        /// </summary>
        /// <param name="menuKey">The key of the menu to get permissions for</param>
        /// <returns>The permission codes of the menu</returns>
        Task<IEnumerable<string>> GetPermissionsOfMenuAsync(string menuKey);

        /// <summary>
        /// Get permission candidates for assigning to a menu, filtered by the menu's PermissionPrefix.
        /// </summary>
        /// <param name="menuKey">Menu key</param>
        /// <returns>Permissions that match the configured prefix</returns>
        Task<IEnumerable<PermissionWithAssignStatusDto>> GetPermissionCandidatesOfMenuAsync(string menuKey);

        #endregion

    }
}