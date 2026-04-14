using Nghex.Identity.DTOs.Menus;
using Nghex.Identity.Models;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IMenuService
    {
        /// <summary>
        /// Get menu item by Menu Key
        /// </summary>
        /// <param name="menuKey">The menu key</param>
        /// <returns>The menu item DTO if found, null otherwise</returns>
        Task<MenuItemDto?> GetByMenuKeyAsync(string menuKey);

        /// <summary>
        /// Get all menu items (active only by default)
        /// </summary>
        /// <param name="activeOnly">Whether to only return active menu items</param>
        /// <returns>A list of menu item DTOs</returns>
        Task<IEnumerable<MenuItemDto>> GetAllAsync(bool activeOnly = true);

        /// <summary>
        /// Create a new menu item
        /// </summary>
        /// <param name="createDto">The menu item data to create</param>
        /// <returns>The created menu item DTO with ID</returns>
        Task<MenuItemDto> CreateAsync(CreateMenuItemDto createDto);

        /// <summary>
        /// Update a menu item by ID
        /// </summary>
        /// <param name="updateDto">The menu item data to update</param>
        /// <returns>True if the menu item was updated, false if the menu item was not found or the update failed</returns>
        Task<bool> UpdateAsync(UpdateMenuItemDto updateDto);

        /// <summary>
        /// Delete a menu item by ID
        /// </summary>
        /// <param name="id">The ID of the menu item to delete</param>
        /// <param name="deletedBy">Who deleted the menu item</param>
        /// <returns>True if the menu item was deleted, false if the menu item was not found or the delete failed</returns>
        Task<bool> DeleteAsync(long id, string deletedBy);

        /// <summary>
        /// Get menu of permissions
        /// </summary>
        /// <param name="permissionCodes">The codes of the permissions to get the menu for</param>
        /// <returns>The menu of the permissions</returns>
        Task<IReadOnlyList<MenuItemAccess>> GetMenuOfPermissionsAsync(IEnumerable<string> permissionCodes);

        #region Build Menu Tree

        /// <summary>
        /// Get menu tree from permissions
        /// </summary>
        /// <param name="permissionCodes">The codes of the permissions to get the menu tree for</param>
        /// <returns>The menu tree of the permissions</returns>
        Task<IReadOnlyList<MenuNodeDto>> GetMenuTreeFromPermissionsAsync(IEnumerable<string> permissionCodes);

        #endregion

    }
}
