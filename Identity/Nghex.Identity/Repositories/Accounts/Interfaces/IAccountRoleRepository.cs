using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    public interface IAccountRoleRepository 
    {
        /// <summary>
        /// Get roles by account ID
        /// </summary>
        /// <param name="accountId">The ID of the account to get roles for</param>
        /// <returns>The roles</returns>
        Task<IEnumerable<RoleEntity>> GetRolesByAccountIdAsync(long accountId);

        /// <summary>
        /// Add role to account
        /// </summary>
        /// <param name="accountId">The ID of the account to add the role to</param>
        /// <param name="roleIds">The IDs of the roles to add</param>
        /// <returns>True if the role was added, false otherwise</returns>
        Task<bool> AddRolesToAccountAsync(long accountId, IReadOnlyList<long> roleIds);

        /// <summary>
        /// Remove role from account
        /// </summary>
        /// <param name="accountId">The ID of the account to remove the role from</param>
        /// <param name="roleIds">The IDs of the roles to remove</param>
        /// <returns>True if the role was removed, false otherwise</returns>
        Task<bool> RemoveRolesFromAccountAsync(long accountId, IReadOnlyList<long> roleIds);

        /// <summary>
        /// Remove all roles from account
        /// </summary>
        /// <param name="accountId">The ID of the account to remove all roles from</param>
        /// <returns>True if the roles were removed, false otherwise</returns>
        Task<bool> RemoveAllRolesFromAccountAsync(long accountId);


    }
}
