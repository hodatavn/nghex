namespace Nghex.Identity.Api.Models.Requests.DataPolicy;

public class SavePolicyDetailsRequest
{
    public long AccountId { get; set; }
    public string PolicyType { get; set; } = string.Empty;
    public List<string> PoCodes { get; set; } = [];
    public string UpdatedBy { get; set; } = string.Empty;

    public bool IsValid(out string error)
    {
        if (AccountId <= 0) { error = "AccountId is required"; return false; }
        if (string.IsNullOrWhiteSpace(PolicyType)) { error = "PolicyType is required"; return false; }
        error = string.Empty;
        return true;
    }
}
