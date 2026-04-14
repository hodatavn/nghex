using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nghex.Base.Entities;

namespace Nghex.Configuration.Persistence.Entities;

/// <summary>
/// Persistence model for Configuration
/// </summary>
[Table("SYS_CONFIGURATIONS")]
public class ConfigurationEntity : BaseEntity
{
    /// <summary>
    /// Configuration key
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("KEY")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Configuration value
    /// </summary>
    [MaxLength(4000)]
    [Column("VALUE")]
    public string? Value { get; set; }

    /// <summary>
    /// Configuration description
    /// </summary>
    [MaxLength(1000)]
    [Column("DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// Data type
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("DATA_TYPE")]
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Module
    /// </summary>
    [MaxLength(100)]
    [Column("MODULE")]
    public string? Module { get; set; }

    /// <summary>
    /// Is system config
    /// </summary>
    [Column("IS_SYSTEM_CONFIG")]
    public bool IsSystemConfig { get; set; } = false;

    /// <summary>
    /// Is editable
    /// </summary>
    [Column("IS_EDITABLE")]
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// Is active
    /// </summary>
    [Column("IS_ACTIVE")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Default value
    /// </summary>
    [MaxLength(4000)]
    [Column("DEFAULT_VALUE")]
    public string? DefaultValue { get; set; }
}
