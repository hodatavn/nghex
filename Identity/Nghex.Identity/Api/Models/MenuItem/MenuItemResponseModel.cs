
namespace Nghex.Identity.Api.Models.MenuItem
{
    /// <summary>
    /// Response model for menu item
    /// </summary>
    public class MenuItemResponseModel
    {
        /// <summary>
        /// Menu item ID
        /// </summary>
        public long MenuId { get; set; }

        /// <summary>
        /// Menu key
        /// </summary>
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// Parent menu key
        /// </summary>
        public string? ParentKey { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Route
        /// </summary>
        public string? Route { get; set; }

        /// <summary>
        /// Icon
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Plugin name
        /// </summary>
        public string? PluginName { get; set; }

        /// <summary>
        /// Permission prefix
        /// </summary>
        public string? PermissionPrefix { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; }

    }
}




