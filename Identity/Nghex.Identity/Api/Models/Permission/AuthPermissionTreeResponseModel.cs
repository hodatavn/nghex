namespace Nghex.Identity.Api.Models.Permission
{
    public class AuthModuleNode
    {
        public string Module { get; set; } = string.Empty;
        public List<PermissionWithAssignStatusResponseModel> Permissions { get; set; } = [];
    }

    public class AuthPluginNode
    {
        public string PluginName { get; set; } = string.Empty;
        public List<AuthModuleNode> Modules { get; set; } = [];
    }
}
