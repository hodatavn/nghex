using Nghex.Base.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Identity.Persistence.Entities;

/// <summary>
/// Persistence model for JWT Token management
/// </summary>
[Table("SYS_JWT_TOKENS")]
public class JwtTokenEntity : BaseEntity
{
    /// <summary>
    /// Account ID
    /// </summary>
    [Required]
    [Column("ACCOUNT_ID")]
    public long AccountId { get; set; }

    /// <summary>
    /// JWT Token ID (jti claim)
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("TOKEN_ID")]
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Refresh Token
    /// </summary>
    [Required]
    [MaxLength(500)]
    [Column("REFRESH_TOKEN")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expires at
    /// </summary>
    [Required]
    [Column("EXPIRES_AT")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Refresh token expires at
    /// </summary>
    [Required]
    [Column("REFRESH_EXPIRES_AT")]
    public DateTime RefreshExpiresAt { get; set; }

    /// <summary>
    /// Is token revoked
    /// </summary>
    [Column("IS_REVOKED")]
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Revoked at
    /// </summary>
    [Column("REVOKED_AT")]
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// IP Address when token was created
    /// </summary>
    [MaxLength(45)]
    [Column("IP_ADDRESS")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent when token was created
    /// </summary>
    [MaxLength(500)]
    [Column("USER_AGENT")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Navigation property to Account
    /// </summary>
    [ForeignKey("AccountId")]
    public virtual AccountEntity? Account { get; set; }
}
