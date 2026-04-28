namespace Nghex.Identity.Models
{
    /// <summary>
    /// DTO returned to clients for menu rendering (tree).
    /// </summary>
    public class MenuNodeDto
    {
        public long Id { get; set; }
        public string MenuKey { get; set; } = string.Empty;
        public string? ParentKey { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Route { get; set; }  
        public string? Icon { get; set; }
        public string? PluginName { get; set; }
        public string? PermissionPrefix { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsEnabled { get; set; }

        /// <summary>
        /// True if the user is allowed to directly access this node (has required VIEW permission or none required).
        /// </summary>
        public bool IsAccessible { get; set; }

        public List<MenuNodeDto> Children { get; set; } = new();
    }

    /// <summary>
    /// DTO for per-screen capabilities (actions) to enable/disable UI controls.
    /// </summary>
    public class ScreenCapabilitiesDto
    {
        public string Screen { get; set; } = string.Empty;

        /// <summary>
        /// Allowed permission codes relevant to the requested screen/module.
        /// </summary>
        public List<string> AllowedPermissions { get; set; } = new();
    }
}










