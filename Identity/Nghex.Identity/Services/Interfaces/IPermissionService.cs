using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionResponse?> GetByIdAsync(long id);
        Task<IEnumerable<PermissionResponse>> GetAllAsync();
        Task<PermissionResponse> CreateAsync(CreatePermissionRequest request);
        Task<bool> UpdateAsync(UpdatePermissionRequest request);
        Task<bool> DeleteAsync(long id, string deletedBy);
        Task<IEnumerable<RoleResponse>> GetRolesByPermissionAsync(long permissionId);
        Task<bool> PermissionHasRoleAsync(long permissionId);
    }
}
