namespace Nghex.Plugins.Models
{
    /// <summary>
    /// Plugin settings model for JSON file
    /// </summary>
    public class PluginSettings
    {
        /// <summary>
        /// List of installed plugins
        /// </summary>
        public List<PluginConfig> Plugins { get; set; } = [];
    }

    /// <summary>
    /// Plugin configuration
    /// </summary>
    public class PluginConfig
    {
        /// <summary>
        /// Plugin name (from plugin implementation)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Plugin DLL file name (relative to plugins directory)
        /// </summary>
        public string DllFileName { get; set; } = string.Empty;

        /// <summary>
        /// Is plugin enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Plugin version (optional, will be read from plugin if not provided)
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Plugin description (optional, will be read from plugin if not provided)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Plugin priority (lower = load first)
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Custom configuration (JSON string)
        /// </summary>
        public string? Configuration { get; set; }
    }
}
