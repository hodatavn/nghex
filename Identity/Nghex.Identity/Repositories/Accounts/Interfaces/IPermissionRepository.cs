using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Base.Repositories;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    /// <summary>
    /// Interface for Permission Repository
    /// </summary>
    public interface IPermissionRepository : IRepository<PermissionEntity>
    {
        /// <summary>
        /// Check if permission code exists
        /// </summary>
        /// <param name="code">Permission code</param>
        /// <returns>True if the permission code exists, false otherwise</returns>
        Task<bool> CodeExistsAsync(string code);

        /// <summary>
        /// Get active (non-deleted) permissions by code prefix.
        /// Intended for filtering permission candidates when assigning permissions to a menu.
        /// </summary>
        /// <param name="codePrefix">Prefix to match (e.g. "ACCOUNT_")</param>
        /// <param name="limit">Max number of rows to return</param>
        Task<IEnumerable<PermissionEntity>> GetActiveByCodePrefixAsync(string codePrefix, int limit = 500);

        /// <summary>
        /// Get roles of a permission
        /// </summary>
        /// <param name="permissionId">The ID of the permission to get roles for</param>
        /// <returns>The roles of the permission</returns>
        Task<IEnumerable<RoleEntity>> GetRolesOfPermissionIdAsync(long permissionId);

    }
}
