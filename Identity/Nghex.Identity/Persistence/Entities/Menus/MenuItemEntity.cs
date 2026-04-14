using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nghex.Plugins;

namespace Nghex.Identity.Persistence.Entities
{
    /// <summary>
    /// Persistence model for MenuItem (server-driven navigation tree)
    /// </summary>
    [Table("SYS_MENU_ITEMS")]
    public class MenuItemEntity : BaseEntity
    {
        /// <summary>
        /// Unique key for referencing the menu item (stable across versions)
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("MENU_KEY")]
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// Parent menu key (null/empty for root items)
        /// </summary>
        [MaxLength(100)]
        [Column("PARENT_KEY")]
        public string? ParentKey { get; set; }

        /// <summary>
        /// Display title (UI label)
        /// </summary>
        [Required]
        [MaxLength(200)]
        [Column("TITLE")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Route/path in client application (optional for group nodes)
        /// </summary>
        [MaxLength(500)]
        [Column("ROUTE")]
        public string? Route { get; set; }

        /// <summary>
        /// Icon identifier (optional)
        /// </summary>
        [MaxLength(200)]
        [Column("ICON")]
        public string? Icon { get; set; }

        /// <summary>
        /// Plugin name
        /// </summary>
        [MaxLength(100)]
        [Column("PLUGIN_NAME")]
        public string? PluginName { get; set; }

        /// <summary>
        /// Optional permission code prefix for filtering permission candidates when assigning permissions to this menu.
        /// Example: "ACCOUNT_" will match permissions like "ACCOUNT_READ", "ACCOUNT_WRITE", etc.
        /// </summary>
        [MaxLength(100)]
        [Column("PERMISSION_PREFIX")]
        public string? PermissionPrefix { get; set; }

        /// <summary>
        /// Sort order under the same parent
        /// </summary>
        [Column("SORT_ORDER")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Is active (visible if user has permission and item is active)
        /// </summary>
        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Computed property for activation based on plugin status
        /// </summary>
        [NotMapped]
        public bool IsEnabled {
            get {
                if (!IsActive) return false;
                if (string.IsNullOrWhiteSpace(PluginName))
                    return true;
                return PluginRegistry.EnabledPluginNames.Contains(PluginName!);
            }
        }
    }
}
