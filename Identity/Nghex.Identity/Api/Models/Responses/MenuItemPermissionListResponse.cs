using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for list of menu item permissions
    /// </summary>
    public class MenuItemPermissionListResponse : BaseResponseModel
    {
        /// <summary>
        /// List of menu item permissions
        /// </summary>
        public List<MenuItemPermissionResponse> Permissions { get; set; } = new();

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}
