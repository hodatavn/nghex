using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses;

public class AccessPolicyResponse : BaseResponseModel
{
    public long AccountId { get; set; }
    public List<AccessPolicyTypeResponse> Policies { get; set; } = [];
}

public class AccessPolicyTypeResponse
{
    public string PolicyType { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public List<string> Codes { get; set; } = [];
}
