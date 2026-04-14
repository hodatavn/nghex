namespace Nghex.Identity.DTOs.Menus;

/// <summary>
/// Data Transfer Object for MenuItem
/// </summary>
public class MenuItemDto
{
    public long Id { get; set; }
    public string MenuKey { get; set; } = string.Empty;
    public string? ParentKey { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public string? PluginName { get; set; }
    public string? PermissionPrefix { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<MenuItemDto> Children { get; set; } = [];
}

/// <summary>
/// DTO for creating a new menu item
/// </summary>
public class CreateMenuItemDto
{
    public string MenuKey { get; set; } = string.Empty;
    public string? ParentKey { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public string? PluginName { get; set; }
    public string? PermissionPrefix { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing menu item
/// </summary>
public class UpdateMenuItemDto
{
    public long Id { get; set; }
    public string MenuKey { get; set; } = string.Empty;
    public string? ParentKey { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public string? PluginName { get; set; }
    public string? PermissionPrefix { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
