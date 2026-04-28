namespace Nghex.Plugins.Abstractions.DataPolicy;

public interface IAccessPolicyCache
{
    Task<bool> IsRestrictedAsync(long accountId, string policyType);
    Task<IReadOnlyList<string>> GetAllowedCodesAsync(long accountId, string policyType);
    void Invalidate(long accountId);
}
