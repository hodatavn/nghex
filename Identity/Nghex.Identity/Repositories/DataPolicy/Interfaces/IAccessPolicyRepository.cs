using Nghex.Identity.Persistence.Entities.DataPolicy;

namespace Nghex.Identity.Repositories.DataPolicy.Interfaces;

public interface IAccessPolicyRepository
{
    /// <summary>Load all policy types and their detail codes for an account in one query.</summary>
    Task<IEnumerable<(string policyType, string policyMode, string? policyCode)>> GetAllPoliciesAsync(long accountId);

    /// <summary>Load all policy rows for an account (for admin view).</summary>
    Task<IEnumerable<AccessPolicyEntity>> GetByAccountIdAsync(long accountId);

    /// <summary>Load all detail codes for an account (for admin view).</summary>
    Task<IEnumerable<AccessPolicyDetailEntity>> GetDetailsByAccountIdAsync(long accountId);

    /// <summary>Upsert mode for a specific policy type.</summary>
    Task UpsertAsync(long accountId, string policyType, string policyMode, string updatedBy);

    /// <summary>Replace all detail codes for a specific policy type (delete + insert in transaction).</summary>
    Task ReplaceDetailsAsync(long accountId, string policyType, IEnumerable<string> policyCodes);

    /// <summary>Delete a specific policy type and its details (in transaction).</summary>
    Task DeleteByTypeAsync(long accountId, string policyType);

    /// <summary>Delete all policy types and details for an account (in transaction).</summary>
    Task DeleteAllAsync(long accountId);
}
