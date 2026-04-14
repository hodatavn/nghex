using Nghex.Plugins.Models;

namespace Nghex.Plugins.Interfaces
{
    /// <summary>
    /// Interface for Plugin Service (File-based architecture)
    /// Uses JSON file (pluginSettings.json) for persistence
    /// </summary>
    public interface IPluginService
    {
        #region Configuration Operations (Persistent - File)

        Task<IEnumerable<PluginConfig>> GetAllPluginsAsync();
        Task<PluginConfig?> GetPluginByNameAsync(string name);
        Task<PluginInstallResult> InstallPluginAsync(Stream fileStream, string fileName, string[] pluginNames, bool isEnabled, string? installedBy);
        Task<bool> UninstallPluginAsync(string pluginName, string uninstalledBy, bool deleteFiles = false);
        Task<bool> EnablePluginAsync(string pluginName, string enabledBy);
        Task<bool> DisablePluginAsync(string pluginName, string disabledBy);
        Task<bool> UpdatePluginConfigurationAsync(string pluginName, string? configuration, string updatedBy);

        #endregion

        #region Runtime Operations (In-Memory via PluginManager)

        Task<bool> ReloadPluginAsync(string pluginName);
        Task<PluginStatusResult?> GetPluginStatusAsync(string pluginName);
        bool IsPluginLoaded(string pluginName);

        #endregion

        #region Lifecycle Operations

        Task InitializeAllPluginsAsync();
        Task ShutdownAllPluginsAsync();

        #endregion

        #region Validation Operations

        Task<IEnumerable<string>> GetMissingDependenciesAsync(string pluginName);
        Task<IEnumerable<PluginConfig>> GetDependentPluginsAsync(string pluginName);
        Task<bool> PluginExistsAsync(string pluginName);

        #endregion
    }
}
