using Nghex.Identity.Api.Models.Responses;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IAuthManagementService
    {
        #region Account - Role - Permission Management

        Task<bool> AssignRolesToAccountAsync(long accountId, IEnumerable<long>? roleIds);
        Task<bool> RemoveRolesFromAccountAsync(long accountId, IEnumerable<long> roleIds);
        Task<bool> RemoveAllRolesFromAccountAsync(long accountId);

        #endregion

        #region Role - Permission

        Task<IEnumerable<RoleResponse>> GetRolesOfAccountAsync(long accountId);
        Task<IEnumerable<PermissionResponse>> GetPermissionsOfAccountAsync(long accountId);
        Task<bool> GrantPermissionsToRoleAsync(long roleId, IEnumerable<long>? permissionIds);
        Task<IEnumerable<PermissionResponse>> GetPermissionsOfRoleAsync(long roleId);

        #endregion

        #region MenuItem-Permission Relationship

        Task<bool> SetPermissionsOnMenuAsync(string menuKey, IEnumerable<string>? permissionCodes);
        Task<bool> RemovePermissionsFromMenuAsync(string menuKey, IEnumerable<string> permissionCodes);
        Task<IEnumerable<string>> GetPermissionsOfMenuAsync(string menuKey);
        Task<IEnumerable<PermissionWithAssignStatus>> GetPermissionCandidatesOfMenuAsync(string menuKey);

        #endregion
    }
}
