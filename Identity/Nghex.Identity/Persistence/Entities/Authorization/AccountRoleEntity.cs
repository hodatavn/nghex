using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Identity.Persistence.Entities;

/// <summary>
/// Persistence model for Account-Role relationship (many-to-many junction table)
/// </summary>
[Table("SYS_ACCOUNT_ROLES")]
public class AccountRoleEntity : BaseEntity
{
    /// <summary>
    /// Account ID
    /// </summary>
    [Required]
    [Column("ACCOUNT_ID")]
    public long AccountId { get; set; }

    /// <summary>
    /// Role ID
    /// </summary>
    [Required]
    [Column("ROLE_ID")]
    public long RoleId { get; set; }

    /// <summary>
    /// Navigation property to Account
    /// </summary>
    [ForeignKey("AccountId")]
    public virtual AccountEntity? Account { get; set; }

    /// <summary>
    /// Navigation property to Role
    /// </summary>
    [ForeignKey("RoleId")]
    public virtual RoleEntity? Role { get; set; }
}
