
namespace Nghex.Plugins
{
    /// <summary>
    /// Interface for all plugins
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Plugin name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Plugin version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Plugin description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Initialize plugin
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Cleanup when unloading plugin
        /// </summary>
        Task CleanupAsync();

        /// <summary>
        /// Check if plugin is enabled
        /// </summary>
        bool IsEnabled { get; }
    }
}
