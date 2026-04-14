namespace Nghex.Configuration.DTOs;

/// <summary>
/// Data Transfer Object for Configuration
/// </summary>
public class ConfigurationDto
{
    public long Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string DataType { get; set; } = "string";
    public string? Module { get; set; }
    public bool IsSystemConfig { get; set; }
    public bool IsEditable { get; set; }
    public bool IsActive { get; set; }
    public string? DefaultValue { get; set; }
}

/// <summary>
/// DTO for creating a new configuration
/// </summary>
public class CreateConfigurationDto
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string DataType { get; set; } = "string";
    public string? Module { get; set; }
    public bool IsSystemConfig { get; set; }
    public bool IsEditable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? DefaultValue { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing configuration
/// </summary>
public class UpdateConfigurationDto
{
    public long Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string DataType { get; set; } = "string";
    public string? Module { get; set; }
    public bool IsEditable { get; set; }
    public bool IsActive { get; set; }
    public string? DefaultValue { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
