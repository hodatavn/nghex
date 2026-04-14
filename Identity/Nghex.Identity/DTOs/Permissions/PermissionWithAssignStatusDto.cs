namespace Nghex.Identity.DTOs.Permissions;

/// <summary>
/// Data Transfer Object for Permission
/// </summary>
public class PermissionWithAssignStatusDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? PluginName { get; set; }
    public bool IsAssigned { get; set; }
}
