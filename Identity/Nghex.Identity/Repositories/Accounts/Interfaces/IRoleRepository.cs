using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Base.Repositories;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    /// <summary>
    /// Interface for Role Repository
    /// </summary>
    public interface IRoleRepository : IRepository<RoleEntity>
    {
        /// <summary>
        /// Get all roles
        /// </summary>
        Task<IEnumerable<RoleEntity>> GetAllAsync(bool isDisabled);

        /// <summary>
        /// Check if role code exists
        /// </summary>
        Task<bool> CodeExistsAsync(string code);

        /// <summary>
        /// Get roles with pagination using cursor-based pagination
        /// </summary>
        Task<IEnumerable<RoleEntity>> GetRolesPagedAsync(long lastId = 0, int pageSize = 100);

        /// <summary>
        /// Get roles with permissions
        /// </summary>
        Task<IEnumerable<RoleEntity>> GetRolesWithPermissionsAsync();
    }
}
