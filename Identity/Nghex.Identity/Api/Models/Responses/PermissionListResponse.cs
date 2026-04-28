using Nghex.Web.AspNetCore.Models.Base;

namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for list of permissions
    /// </summary>
    public class PermissionListResponse : BaseResponseModel
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

    /// <summary>
    /// Tree node for Plugin in permission tree
    /// </summary>
    public class PluginNode
    {
        /// <summary>
        /// Plugin name
        /// </summary>
        public string PluginName { get; set; } = string.Empty;

        /// <summary>
        /// List of modules under this plugin
        /// </summary>
        public List<ModuleNode> Modules { get; set; } = [];
    }

    /// <summary>
    /// Tree node for Module in permission tree
    /// </summary>
    public class ModuleNode
    {
        /// <summary>
        /// Module name
        /// </summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// List of permissions under this module
        /// </summary>
        public List<PermissionResponse> Permissions { get; set; } = [];
    }
}
