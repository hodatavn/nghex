using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Identity.Persistence.Entities
{
    /// <summary>
    /// Persistence model for Account
    /// </summary>
    [Table("SYS_ACCOUNTS")]
    public class AccountEntity : BaseEntity
    {
        /// <summary>
        /// Username
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("USERNAME")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        [Column("EMAIL")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("PASSWORD")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// IP Address
        /// </summary>
        [MaxLength(255)]
        [Column("IP_ADDRESS")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        [MaxLength(200)]
        [Column("DISPLAY_NAME")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        [Column("IS_ACTIVE")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Is locked
        /// </summary>
        [Column("IS_LOCKED")]
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Is deleted
        /// </summary>
        [Column("IS_DELETED")]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Last login date
        /// </summary>
        [Column("LAST_LOGIN_AT")]
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Failed login attempts
        /// </summary>
        [Column("FAILED_LOGIN_ATTEMPTS")]
        public int FailedLoginAttempts { get; set; } = 0;

        /// <summary>
        /// Locked until
        /// </summary>
        [Column("LOCKED_UNTIL")]
        public DateTime? LockedUntil { get; set; }
    }
}
