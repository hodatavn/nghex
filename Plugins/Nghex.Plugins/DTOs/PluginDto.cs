namespace Nghex.Plugins.DTOs
{
    /// <summary>
    /// Plugin data transfer object
    /// </summary>
    public class PluginDto
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AssemblyPath { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public bool IsLoaded { get; set; }
        public string? Configuration { get; set; }
    }
}
