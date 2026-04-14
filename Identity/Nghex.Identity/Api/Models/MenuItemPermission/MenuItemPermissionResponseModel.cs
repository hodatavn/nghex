using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.MenuItemPermission
{
    /// <summary>
    /// Response model for menu item permission
    /// </summary>
    public class MenuItemPermissionResponseModel : BaseResponseModel
    {
        /// <summary>
        /// Permission ID
        /// </summary>
        public long PermissionId { get; set; }

        /// <summary>
        /// Menu key
        /// </summary>
        public string MenuKey { get; set; } = string.Empty;

        /// <summary>
        /// Permission code
        /// </summary>
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>
        /// Created date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Created by
        /// </summary>
        public string? CreatedBy { get; set; }
    }
}




