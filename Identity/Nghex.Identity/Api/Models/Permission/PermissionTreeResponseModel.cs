namespace Nghex.Identity.Api.Models.Permission
{
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
        public List<PermissionResponseModel> Permissions { get; set; } = [];
    }
}
