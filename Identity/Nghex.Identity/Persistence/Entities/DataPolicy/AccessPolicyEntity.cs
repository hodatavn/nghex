using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Identity.Persistence.Entities.DataPolicy;

[Table("SYS_ACCESS_POLICY")]
public class AccessPolicyEntity
{
    [Column("ACCOUNT_ID")]
    public long AccountId { get; set; }

    [Column("POLICY_TYPE")]
    public string PolicyType { get; set; } = string.Empty;

    [Column("AP_MODE")]
    public string Mode { get; set; } = AccessPolicyMode.All;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; }

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; }
}

public static class AccessPolicyMode
{
    public const string All = "ALL";
    public const string Restricted = "RESTRICTED";
}
