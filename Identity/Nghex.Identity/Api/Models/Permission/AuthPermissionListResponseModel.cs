using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Permission
{
    /// <summary>
    /// Response model for list of permissions
    /// </summary>
    public class AuthPermissionListResponseModel : BaseResponseModel
    {

        /// <summary>
        /// Tree structure: PluginName -> Module -> Permission list
        /// </summary>
        public List<AuthPluginNode> PermissionTree { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}




