
namespace Nghex.Plugins
{
    /// <summary>
    /// Interface for plugin manager
    /// </summary>
    public interface IPluginManager
    {
        /// <summary>
        /// Analyze plugin DLL and optionally create plugin instance
        /// This method provides unified plugin loading logic used by both install API and runtime loading
        /// </summary>
        /// <param name="pluginPath">Path to the plugin DLL file</param>
        /// <param name="createInstance">Whether to create plugin instance (default: true)</param>
        /// <returns>PluginAnalysisResult containing metadata and optionally the plugin instance</returns>
        Task<PluginAnalysisResult> AnalyzePluginAsync(string pluginPath, bool createInstance = true);

        /// <summary>
        /// Load plugin from file dll
        /// </summary>
        Task<IPlugin> LoadPluginAsync(string pluginPath);

        /// <summary>
        /// Unload plugin
        /// </summary>
        Task UnloadPluginAsync(string pluginName);

        /// <summary>
        /// Get plugin by name
        /// </summary>
        IPlugin? GetPlugin(string pluginName);

        /// <summary>
        /// Get all loaded plugins
        /// </summary>
        IEnumerable<IPlugin> GetAllPlugins();

        /// <summary>
        /// Check if plugin is loaded
        /// </summary>
        bool IsPluginLoaded(string pluginName);

        /// <summary>
        /// Reload plugin
        /// </summary>
        Task ReloadPluginAsync(string pluginName);

        /// <summary>
        /// Load all plugins in the directory
        /// </summary>
        Task LoadAllPluginsAsync();

        /// <summary>
        /// Get plugin assembly by plugin name
        /// </summary>
        System.Reflection.Assembly? GetPluginAssembly(string pluginName);

        /// <summary>
        /// Get all loaded plugin assemblies
        /// </summary>
        IEnumerable<System.Reflection.Assembly> GetAllPluginAssemblies();
    }
}
