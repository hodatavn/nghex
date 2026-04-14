namespace Nghex.Identity.DTOs.Permissions;

/// <summary>
/// Data Transfer Object for Permission
/// </summary>
public class PermissionDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Module { get; set; }
    public string? PluginName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a new permission
/// </summary>
public class CreatePermissionDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Module { get; set; }
    public string? PluginName { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing permission
/// </summary>
public class UpdatePermissionDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Module { get; set; }
    public string? PluginName { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
