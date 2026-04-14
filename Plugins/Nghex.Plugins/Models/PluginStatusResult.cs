namespace Nghex.Plugins.Models
{
    /// <summary>
    /// Plugin status result (combines file config and runtime state)
    /// </summary>
    public class PluginStatusResult
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AssemblyPath { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
        public bool IsLoaded { get; set; }
        public DateTime? LastLoadedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
