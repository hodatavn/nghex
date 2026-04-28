using Microsoft.AspNetCore.Http;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Plugins.Abstractions.DataPolicy;

namespace Nghex.Identity.AccessPolicy;

public class AccessPolicyContext(IAccessPolicyCache cache, IHttpContextAccessor httpContextAccessor) : IAccessPolicyContext
{
    private long AccountId => httpContextAccessor.HttpContext?.User.GetUserId() ?? 0;

    public Task<bool> IsRestrictedAsync(string policyType)
        => cache.IsRestrictedAsync(AccountId, policyType);

    public Task<IReadOnlyList<string>> GetAllowedCodesAsync(string policyType)
        => cache.GetAllowedCodesAsync(AccountId, policyType);
}
