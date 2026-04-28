using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Nghex.Identity.Persistence.Entities.DataPolicy;
using Nghex.Identity.Repositories.DataPolicy.Interfaces;
using Nghex.Plugins.Abstractions.DataPolicy;

namespace Nghex.Identity.AccessPolicy;

// Singleton — uses IServiceScopeFactory to resolve scoped IAccessPolicyRepository per load
public class AccessPolicyCache(IMemoryCache memoryCache, IServiceScopeFactory scopeFactory) : IAccessPolicyCache
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public async Task<bool> IsRestrictedAsync(long accountId, string policyType)
    {
        var policies = await GetOrLoadAllAsync(accountId);
        return policies.TryGetValue(policyType, out var entry) && entry.IsRestrict;
    }

    public async Task<IReadOnlyList<string>> GetAllowedCodesAsync(long accountId, string policyType)
    {
        var policies = await GetOrLoadAllAsync(accountId);
        return policies.TryGetValue(policyType, out var entry)
            ? entry.AllowedCodes
            : Array.Empty<string>();
    }

    public void Invalidate(long accountId)
        => memoryCache.Remove(CacheKey(accountId));

    private async Task<IReadOnlyDictionary<string, PolicyEntry>> GetOrLoadAllAsync(long accountId)
    {
        return await memoryCache.GetOrCreateAsync(CacheKey(accountId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await LoadFromDbAsync(accountId);
        }) ?? new Dictionary<string, PolicyEntry>();
    }

    private async Task<IReadOnlyDictionary<string, PolicyEntry>> LoadFromDbAsync(long accountId)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAccessPolicyRepository>();
        var rows = (await repository.GetAllPoliciesAsync(accountId)).ToList();

        // Group by policy type: each type has one mode and zero or more po_codes
        return rows
            .GroupBy(r => r.policyType)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    bool isRestrict = g.First().policyMode == AccessPolicyMode.Restricted;
                    var codes = isRestrict
                        ? (IReadOnlyList<string>)g.Where(r => r.policyCode != null).Select(r => r.policyCode!).ToList()
                        : Array.Empty<string>();
                    return new PolicyEntry(isRestrict, codes);
                });
    }

    private static string CacheKey(long accountId) => $"access_policy:{accountId}";

    private record PolicyEntry(bool IsRestrict, IReadOnlyList<string> AllowedCodes);
}
