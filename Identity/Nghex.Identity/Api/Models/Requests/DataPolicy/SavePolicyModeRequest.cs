namespace Nghex.Identity.Api.Models.Requests.DataPolicy;

public class SavePolicyModeRequest
{
    public long AccountId { get; set; }
    public string PolicyType { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsValid(out string error)
    {
        if (AccountId <= 0) { error = "AccountId is required"; return false; }
        if (string.IsNullOrWhiteSpace(PolicyType)) { error = "PolicyType is required"; return false; }
        if (Mode != "ALL" && Mode != "RESTRICTED") { error = "Mode must be 'ALL' or 'RESTRICTED'"; return false; }
        error = string.Empty;
        return true;
    }
}
