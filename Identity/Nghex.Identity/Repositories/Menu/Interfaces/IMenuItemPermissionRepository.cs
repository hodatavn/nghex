using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Repositories.Menu.Interfaces
{
    public interface IMenuItemPermissionRepository
    {

        /// <summary>
        /// Get all menu item permissions
        /// </summary>
        /// <returns>The menu item permissions</returns>
        Task<IEnumerable<MenuItemPermissionEntity>> GetAllAsync();

        /// <summary>
        /// Get permissions for a menu item by menu key
        /// </summary>
        /// <param name="menuKey">The key of the menu to get permissions for</param>
        /// <returns>The permissions of the menu</returns>
        Task<IEnumerable<MenuItemPermissionEntity>> GetPermissionsOfMenuAsync(string menuKey);

        /// <summary>
        /// Add permissions to a menu
        /// </summary>
        /// <param name="menuKey">The key of the menu to add permissions to</param>
        /// <param name="permissionCodes">The codes of the permissions to add</param>
        /// <returns>True if the permissions were added, false otherwise</returns>  
        Task<bool> AddPermissionsToMenuAsync(string menuKey, IReadOnlyList<string> permissionCodes);

        /// <summary>
        /// Remove permissions from a menu
        /// </summary>
        /// <param name="menuKey">The key of the menu to remove permissions from</param>
        /// <param name="permissionCodes">The codes of the permissions to remove</param>
        /// <returns>True if the permissions were removed, false otherwise</returns>
        Task<bool> RemovePermissionsFromMenuAsync(string menuKey, IReadOnlyList<string> permissionCodes);

        /// <summary>
        /// Remove all permissions for a menu
        /// </summary>
        /// <param name="menuKey">The key of the menu to remove all permissions from</param>
        /// <returns>True if the permissions were removed, false otherwise</returns>
        Task<bool> RemoveAllPermissionsForMenuAsync(string menuKey);

        /// <summary>
        /// Check if a menu has a specific permission
        /// </summary>
        /// <param name="menuKey">The key of the menu to check the permission for</param>
        /// <returns>True if the menu has the permission, false otherwise</returns>
        Task<bool> MenuHasPermissionAsync(string menuKey);
    }
}

