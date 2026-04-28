using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for list of auth permissions
    /// </summary>
    public class AuthPermissionListResponse : BaseResponseModel
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

    public class AuthModuleNode
    {
        public string Module { get; set; } = string.Empty;
        public List<PermissionWithAssignStatus> Permissions { get; set; } = [];
    }

    public class AuthPluginNode
    {
        public string PluginName { get; set; } = string.Empty;
        public List<AuthModuleNode> Modules { get; set; } = [];
    }
}
