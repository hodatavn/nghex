using System.ComponentModel.DataAnnotations.Schema;

namespace Nghex.Identity.Persistence.Entities.DataPolicy;

[Table("SYS_ACCESS_POLICY_DETAIL")]
public class AccessPolicyDetailEntity
{
    [Column("ACCOUNT_ID")]
    public long AccountId { get; set; }

    [Column("POLICY_TYPE")]
    public string PolicyType { get; set; } = string.Empty;

    [Column("AP_CODE")]
    public string Code { get; set; } = string.Empty;
}
