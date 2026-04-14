using Nghex.Identity.Models;

namespace Nghex.Identity.Repositories.Menu.Interfaces
{
    /// <summary>
    /// Specialized repository interface for querying only authorized menu items (and their ancestors).
    /// Kept separate from IMenuRepository to avoid impacting existing implementations.
    /// </summary>
    public interface IMenuAccessQueryRepository
    {
        /// <summary>
        /// Get menu items of the given permission codes (and include ancestors).
        /// Intended for Oracle using hierarchical query; caller provides permission codes (already loaded from account).
        /// </summary>
        /// <param name="permissionCodes">The permission codes to query for</param>
        /// <returns>A list of menu items with access information</returns>
        Task<IEnumerable<MenuItemAccess>> GetMenuItemsOfPermissionsAsync(IEnumerable<string> permissionCodes);
    }
}


