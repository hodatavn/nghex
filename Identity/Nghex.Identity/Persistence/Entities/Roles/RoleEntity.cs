using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nghex.Identity.Enum;

namespace Nghex.Identity.Persistence.Entities;

/// <summary>
/// Persistence model for Role
/// </summary>
[Table("SYS_ROLES")]
public class RoleEntity : BaseEntity
{
    /// <summary>
    /// Role code
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("CODE")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Role name
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("NAME")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role description
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
    /// Role level
    /// </summary>
    [Column("ROLE_LEVEL")]
    public RoleLevel RoleLevel { get; set; } = RoleLevel.User;

    /// <summary>
    /// Is deleted
    /// </summary>
    [Column("IS_DELETED")]
    public bool IsDeleted { get; set; } = false;
}
