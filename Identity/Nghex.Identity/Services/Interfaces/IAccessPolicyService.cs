using Nghex.Identity.Api.Models.Responses;

namespace Nghex.Identity.Services.Interfaces;

public interface IAccessPolicyService
{
    Task<AccessPolicyResponse> GetByAccountIdAsync(long accountId);
    Task UpsertAsync(long accountId, string policyType, string mode, string updatedBy);
    Task SaveDetailsAsync(long accountId, string policyType, IEnumerable<string> poCodes, string updatedBy);
    Task DeleteByTypeAsync(long accountId, string policyType, string deletedBy);
    Task DeleteAllAsync(long accountId, string deletedBy);
}
