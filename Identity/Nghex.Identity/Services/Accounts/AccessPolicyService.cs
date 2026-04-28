using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Repositories.DataPolicy.Interfaces;
using Nghex.Identity.Services.Interfaces;
using Nghex.Plugins.Abstractions.DataPolicy;

namespace Nghex.Identity.Services.Accounts;

public class AccessPolicyService(
    IAccessPolicyRepository accessPolicyRepository,
    IAccessPolicyCache accessPolicyCache) : IAccessPolicyService
{
    public async Task<AccessPolicyResponse> GetByAccountIdAsync(long accountId)
    {
        var rows = (await accessPolicyRepository.GetAllPoliciesAsync(accountId)).ToList();

        return new AccessPolicyResponse
        {
            AccountId = accountId,
            Policies = rows
                .GroupBy(r => r.policyType)
                .Select(g => new AccessPolicyTypeResponse
                {
                    PolicyType = g.Key,
                    Mode = g.First().policyMode,
                    Codes = g.Where(r => r.policyCode != null).Select(r => r.policyCode!).ToList()
                })
                .ToList()
        };
    }

    public async Task UpsertAsync(long accountId, string policyType, string mode, string updatedBy)
    {
        await accessPolicyRepository.UpsertAsync(accountId, policyType, mode, updatedBy);
        accessPolicyCache.Invalidate(accountId);
    }

    public async Task SaveDetailsAsync(long accountId, string policyType, IEnumerable<string> poCodes, string updatedBy)
    {
        await accessPolicyRepository.ReplaceDetailsAsync(accountId, policyType, poCodes);
        accessPolicyCache.Invalidate(accountId);
    }

    public async Task DeleteByTypeAsync(long accountId, string policyType, string deletedBy)
    {
        await accessPolicyRepository.DeleteByTypeAsync(accountId, policyType);
        accessPolicyCache.Invalidate(accountId);
    }

    public async Task DeleteAllAsync(long accountId, string deletedBy)
    {
        await accessPolicyRepository.DeleteAllAsync(accountId);
        accessPolicyCache.Invalidate(accountId);
    }
}
