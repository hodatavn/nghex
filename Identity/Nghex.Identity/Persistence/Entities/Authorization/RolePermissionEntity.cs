using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Identity.Persistence.Entities;

/// <summary>
/// Persistence model for Role-Permission relationship (many-to-many junction table)
/// </summary>
[Table("SYS_ROLE_PERMISSIONS")]
public class RolePermissionEntity : BaseEntity
{
    /// <summary>
    /// Role ID
    /// </summary>
    [Required]
    [Column("ROLE_ID")]
    public long RoleId { get; set; }

    /// <summary>
    /// Permission ID
    /// </summary>
    [Required]
    [Column("PERMISSION_ID")]
    public long PermissionId { get; set; }

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    [ForeignKey("RoleId")]
    public virtual RoleEntity? Role { get; set; }

    /// <summary>
    /// Navigation property to Permission
    /// </summary>
    [ForeignKey("PermissionId")]
    public virtual PermissionEntity? Permission { get; set; }
}
