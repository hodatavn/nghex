using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.MenuItemPermission
{
    /// <summary>
    /// Response model for list of menu item permissions
    /// </summary>
    public class MenuItemPermissionListResponseModel : BaseResponseModel
    {
        /// <summary>
        /// List of menu item permissions
        /// </summary>
        public List<MenuItemPermissionResponseModel> Permissions { get; set; } = new();

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}




