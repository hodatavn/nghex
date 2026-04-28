using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IRoleService
    {
        Task<RoleResponse?> GetByIdAsync(long id);
        Task<IEnumerable<RoleResponse>> GetAllAsync(bool isDisabled);
        Task<RoleResponse> CreateAsync(CreateRoleRequest request);
        Task<bool> UpdateAsync(UpdateRoleRequest request);
        Task<bool> DeleteAsync(long id, string deletedBy);
        Task<bool> RoleHasPermissionAsync(long roleId);
    }
}
