using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Permission
{
    /// <summary>
    /// Response model for list of permissions
    /// </summary>
    public class PermissionListResponseModel : BaseResponseModel
    {

        /// <summary>
        /// Tree structure: PluginName -> Module -> Permission list
        /// </summary>
        public List<PluginNode> PermissionTree { get; set; } = [];

        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; set; }
    }
}
