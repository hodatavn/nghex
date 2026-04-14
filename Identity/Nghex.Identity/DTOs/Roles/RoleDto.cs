using Nghex.Identity.Enum;

namespace Nghex.Identity.DTOs.Roles;

/// <summary>
/// Data Transfer Object for Role
/// </summary>
public class RoleDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoleLevel RoleLevel { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a new role
/// </summary>
public class CreateRoleDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RoleLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing role
/// </summary>
public class UpdateRoleDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RoleLevel { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
