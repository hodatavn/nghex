namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for permission with assign status
    /// </summary>
    public class PermissionWithAssignStatus
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string? PluginName { get; set; }
        public bool IsAssigned { get; set; }
    }
}
