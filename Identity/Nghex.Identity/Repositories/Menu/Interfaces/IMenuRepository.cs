using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Base.Repositories;

namespace Nghex.Identity.Repositories.Menu.Interfaces
{
    public interface IMenuRepository : IRepository<MenuItemEntity>
    {
        /// <summary>
        /// Get menu item by menu key
        /// </summary>
        /// <param name="menuKey">The menu key</param>
        /// <returns>The menu item if found, null otherwise</returns>
        Task<MenuItemEntity?> GetMenuByKeyAsync(string menuKey);

        /// <summary>
        /// Get all menu items (active only by default)
        /// </summary>
        /// <param name="activeOnly">Whether to only return active menu items</param>
        /// <returns>A list of menu items</returns>
        Task<IEnumerable<MenuItemEntity>> GetAllAsync(bool activeOnly = true);
        
        /// <summary>
        /// Check if a menu key exists
        /// </summary>
        /// <param name="menuKey">The menu key to check</param>
        /// <returns>True if the menu key exists, false otherwise</returns>
        Task<bool> MenuKeyExistsAsync(string menuKey);
        
    }
}

