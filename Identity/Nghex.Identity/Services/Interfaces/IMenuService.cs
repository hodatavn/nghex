using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Models;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IMenuService
    {
        Task<MenuItemResponse?> GetByMenuKeyAsync(string menuKey);
        Task<IEnumerable<MenuItemResponse>> GetAllAsync(bool activeOnly = true);
        Task<MenuItemResponse> CreateAsync(CreateMenuItemRequest request);
        Task<bool> UpdateAsync(UpdateMenuItemRequest request);
        Task<bool> DeleteAsync(long id, string deletedBy);
        Task<IReadOnlyList<MenuItemAccess>> GetMenuOfPermissionsAsync(IEnumerable<string> permissionCodes);
        Task<IReadOnlyList<MenuNodeDto>> GetMenuTreeFromPermissionsAsync(IEnumerable<string> permissionCodes);
    }
}
