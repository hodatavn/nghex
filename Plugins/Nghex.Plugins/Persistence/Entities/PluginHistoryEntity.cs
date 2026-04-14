using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Plugins.Persistence.Entities
{
    /// <summary>
    /// Plugin action types for history tracking
    /// </summary>
    public static class PluginAction
    {
        public const string Install = "INSTALL";
        public const string Uninstall = "UNINSTALL";
        public const string Enable = "ENABLE";
        public const string Disable = "DISABLE";
        public const string Update = "UPDATE";
        public const string Load = "LOAD";
        public const string Unload = "UNLOAD";
        public const string Error = "ERROR";
    }

    /// <summary>
    /// Persistence model for Plugin history (audit trail, never deleted)
    /// </summary>
    [Table("SYS_PLUGIN_HISTORY")]
    public class PluginHistoryEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        /// <summary>
        /// Plugin name
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("PLUGIN_NAME")]
        public string PluginName { get; set; } = string.Empty;

        /// <summary>
        /// Plugin version at the time of action
        /// </summary>
        [MaxLength(20)]
        [Column("PLUGIN_VERSION")]
        public string? PluginVersion { get; set; }

        /// <summary>
        /// Action type (INSTALL, UNINSTALL, ENABLE, DISABLE, UPDATE, LOAD, UNLOAD, ERROR)
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("ACTION")]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Assembly path at the time of action
        /// </summary>
        [MaxLength(500)]
        [Column("ASSEMBLY_PATH")]
        public string? AssemblyPath { get; set; }

        /// <summary>
        /// Plugin description at the time of action
        /// </summary>
        [MaxLength(1000)]
        [Column("DESCRIPTION")]
        public string? Description { get; set; }

        /// <summary>
        /// Plugin configuration snapshot (JSON string)
        /// </summary>
        [Column("CONFIGURATION")]
        public string? Configuration { get; set; }

        /// <summary>
        /// Who performed the action
        /// </summary>
        [MaxLength(100)]
        [Column("ACTION_BY")]
        public string? ActionBy { get; set; }

        /// <summary>
        /// When the action was performed
        /// </summary>
        [Required]
        [Column("ACTION_AT")]
        public DateTime ActionAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Reason for the action (optional)
        /// </summary>
        [MaxLength(500)]
        [Column("REASON")]
        public string? Reason { get; set; }

        /// <summary>
        /// Additional details (JSON string)
        /// </summary>
        [Column("DETAILS")]
        public string? Details { get; set; }
    }
}
