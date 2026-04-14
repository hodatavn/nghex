using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nghex.Base.Entities;

namespace Nghex.Plugins.Persistence.Entities
{
    /// <summary>
    /// Persistence model for Plugin (uses hard delete)
    /// </summary>
    [Table("SYS_PLUGINS")]
    public class PluginEntity : BaseEntity
    {
        /// <summary>
        /// Plugin name (unique identifier)
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("NAME")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Plugin version
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("VERSION")]
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Plugin description
        /// </summary>
        [MaxLength(1000)]
        [Column("DESCRIPTION")]
        public string? Description { get; set; }

        /// <summary>
        /// Assembly path (DLL file name relative to plugins directory)
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("ASSEMBLY_PATH")]
        public string AssemblyPath { get; set; } = string.Empty;

        /// <summary>
        /// Is plugin currently loaded in memory (runtime state)
        /// </summary>
        [Column("IS_LOADED")]
        public bool IsLoaded { get; set; } = false;

        /// <summary>
        /// Is plugin enabled (configuration)
        /// </summary>
        [Column("IS_ENABLED")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Last time plugin was loaded
        /// </summary>
        [Column("LAST_LOADED_AT")]
        public DateTime? LastLoadedAt { get; set; }

        /// <summary>
        /// Error message if plugin failed to load
        /// </summary>
        [MaxLength(2000)]
        [Column("ERROR_MESSAGE")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Plugin dependencies (JSON array of plugin names)
        /// </summary>
        [MaxLength(1000)]
        [Column("DEPENDENCIES")]
        public string? Dependencies { get; set; }

        /// <summary>
        /// Plugin-specific configuration (JSON string)
        /// </summary>
        [Column("CONFIGURATION")]
        public string? Configuration { get; set; }

        /// <summary>
        /// Plugin load priority (lower = load first)
        /// </summary>
        [Column("PRIORITY")]
        public int Priority { get; set; } = 0;
    }
}
