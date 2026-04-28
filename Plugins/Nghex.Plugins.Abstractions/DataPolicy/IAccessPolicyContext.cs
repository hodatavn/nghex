namespace Nghex.Plugins.Abstractions.DataPolicy;

public interface IAccessPolicyContext
{
    Task<bool> IsRestrictedAsync(string policyType);
    Task<IReadOnlyList<string>> GetAllowedCodesAsync(string policyType);
}
