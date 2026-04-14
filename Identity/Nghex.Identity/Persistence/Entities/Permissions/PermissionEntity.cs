using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nghex.Plugins;

namespace Nghex.Identity.Persistence.Entities;

/// <summary>
/// Persistence model for Permission
/// </summary>
[Table("SYS_PERMISSIONS")]
public class PermissionEntity : BaseEntity
{
    /// <summary>
    /// Permission code
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("CODE")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Permission name
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("NAME")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Plugin name
    /// </summary>
    [MaxLength(100)]
    [Column("PLUGIN_NAME")]
    public string? PluginName { get; set; }

    /// <summary>
    /// Module
    /// </summary>
    [MaxLength(100)]
    [Column("MODULE")]
    public string? Module { get; set; }

    /// <summary>
    /// Permission description
    /// </summary>
    [MaxLength(500)]
    [Column("DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// Is active
    /// </summary>
    [Column("IS_ACTIVE")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is deleted
    /// </summary>
    [Column("IS_DELETED")]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Computed property for activation based on plugin status
    /// </summary>
    [NotMapped]
    public bool IsEnabled {
        get {
            if (!IsActive || IsDeleted) return false;
            if (string.IsNullOrWhiteSpace(PluginName))
                return true;
            return PluginRegistry.EnabledPluginNames.Contains(PluginName!);
        }
    }
}
