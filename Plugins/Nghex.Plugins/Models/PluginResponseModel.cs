namespace Nghex.Plugins.Models;

public class PluginResponseModel
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssemblyPath { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsLoaded { get; set; }
    public int Priority { get; set; }
    public string? Configuration { get; set; }
}
