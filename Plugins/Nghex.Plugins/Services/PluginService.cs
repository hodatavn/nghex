using Nghex.Plugins.Interfaces;
using Nghex.Plugins.Services;
using System.Text.Json;
using Nghex.Plugins.Models;
using Nghex.Core.Logging;
using Nghex.Utilities;
using Microsoft.Extensions.Configuration;
using Nghex.Plugins.DTOs;

namespace Nghex.Plugins.Services
{
    /// <summary>
    /// Plugin Service implementation (File-based Architecture)
    /// Uses JSON file (pluginSettings.json) for persistence
    /// </summary>
    public class PluginService(
        IPluginManager pluginManager,
        PluginSettingsService fileSettingsService,
        ILogging loggingService,
        IConfiguration configuration) : IPluginService
    {
        private readonly IPluginManager _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
        private readonly PluginSettingsService _fileSettingsService = fileSettingsService ?? throw new ArgumentNullException(nameof(fileSettingsService));
        private readonly ILogging _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        private readonly string _pluginsDirectory = GetPluginsDirectory(configuration);

        // Runtime state tracking (in-memory only)
        private readonly Dictionary<string, DateTime> _lastLoadedAt = [];
        private readonly Dictionary<string, string?> _errorMessages = [];

        #region Configuration Operations

        public async Task<IEnumerable<PluginConfig>> GetAllPluginsAsync()
        {
            var settings = await _fileSettingsService.LoadSettingsAsync();
            return settings.Plugins;
        }

        public async Task<PluginConfig?> GetPluginByNameAsync(string name)
        {
            var settings = await _fileSettingsService.LoadSettingsAsync();
            return settings.Plugins.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<PluginInstallResult> InstallPluginAsync(Stream fileStream, string fileName, string[] pluginNames, bool isEnabled, string? installedBy)
        {
            var result = new PluginInstallResult();

            try
            {
                // 1. Validate file type
                var fileNameLower = fileName.ToLowerInvariant();
                if (!FileHelper.IsValidFileExtension(fileNameLower, [".zip", ".tar", ".tgz", ".tar.gz", ".dll"]))
                {
                    result.ErrorMessage = "Unsupported file type. Upload a .dll, .zip, .tar or .tar.gz";
                    return result;
                }

                // 2. Ensure plugins directory exists
                if (!Directory.Exists(_pluginsDirectory))
                    Directory.CreateDirectory(_pluginsDirectory);

                // 3. Save to temp directory
                var uploadedPath = await FileHelper.UploadFile(fileStream, fileName);

                // 4. Extract or copy
                var installedDllPaths = new List<string>();
                if (FileHelper.IsValidCompressExtension(uploadedPath))
                {
                    FileHelper.ExtractFile(uploadedPath, _pluginsDirectory, overwriteFiles: true);
                    installedDllPaths.AddRange(Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.AllDirectories));
                }
                else
                {
                    var targetPath = Path.Combine(_pluginsDirectory, fileName);
                    File.Copy(uploadedPath, targetPath, overwrite: true);
                    installedDllPaths.Add(targetPath);
                }

                // 5. Cleanup temp
                FileHelper.DeleteDirectory(uploadedPath);

                // 6. Analyze and register plugins
                var normalizedPluginNames = NormalizePluginNames(pluginNames);

                // Pre-filter DLLs
                var candidateDlls = installedDllPaths
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Where(dll => !normalizedPluginNames.HasAnyFilter || MatchesPluginName(dll, normalizedPluginNames))
                    .ToList();

                foreach (var dllPath in candidateDlls)
                {
                    try
                    {
                        // Use unified AnalyzePluginAsync from PluginManager (createInstance = false for install)
                        var analysis = await _pluginManager.AnalyzePluginAsync(dllPath, createInstance: false);
                        
                        if (normalizedPluginNames.HasAnyFilter &&
                            !normalizedPluginNames.PluginNames.Contains(analysis.Name))
                            continue;

                        // Add or update plugin in file settings
                        var pluginConfig = new PluginConfig
                        {
                            Name = analysis.Name,
                            Version = analysis.Version,
                            Description = analysis.Description,
                            DllFileName = Path.GetFileName(dllPath),
                            IsEnabled = isEnabled,
                            Priority = normalizedPluginNames.GetPriority(analysis.Name),
                            Configuration = null
                        };

                        await _fileSettingsService.AddOrUpdatePluginAsync(pluginConfig);

                        // Convert to DTO for response
                        var pluginDto = new PluginDto
                        {
                            Name = pluginConfig.Name,
                            Version = pluginConfig.Version ?? "1.0.0",
                            Description = pluginConfig.Description,
                            AssemblyPath = pluginConfig.DllFileName,
                            IsEnabled = pluginConfig.IsEnabled,
                            Priority = pluginConfig.Priority,
                            IsLoaded = _pluginManager.IsPluginLoaded(pluginConfig.Name),
                            Configuration = pluginConfig.Configuration
                        };

                        result.Plugins.Add(pluginDto);
                        
                        // Unload context after analysis
                        try { analysis.LoadContext.Unload(); } catch { }
                    }
                    catch (Exception ex)
                    {
                        await _loggingService.LogWarningAsync(
                            $"Failed to analyze plugin DLL: {dllPath}",
                            module: "PluginService",
                            details: new { Error = ex.Message }
                        );
                    }
                }

                result.Success = result.Plugins.Count > 0;
                if (!result.Success)
                    result.ErrorMessage = "No valid plugins found in uploaded file";

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to install plugin",
                    ex,
                    source: "PluginService.InstallPluginAsync",
                    module: "Plugin",
                    action: "Install"
                );
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<bool> UninstallPluginAsync(string pluginName, string uninstalledBy, bool deleteFiles = false)
        {
            try
            {
                // 1. Get plugin info before deletion
                var plugin = await GetPluginByNameAsync(pluginName);
                if (plugin == null)
                    return false;

                // 2. Unload from memory if loaded
                if (_pluginManager.IsPluginLoaded(pluginName))
                    await _pluginManager.UnloadPluginAsync(pluginName);

                // 3. Remove from file settings
                await _fileSettingsService.RemovePluginAsync(pluginName);

                // 4. Delete files if requested
                if (deleteFiles && !string.IsNullOrEmpty(plugin.DllFileName))
                {
                    var filePath = Path.Combine(_pluginsDirectory, plugin.DllFileName);
                    if (File.Exists(filePath))
                    {
                        try { FileHelper.DeleteFile(filePath); } catch { }
                    }
                    var pdbPath = Path.ChangeExtension(filePath, ".pdb");
                    if (File.Exists(pdbPath))
                    {
                        try { FileHelper.DeleteFile(pdbPath); } catch { }
                    }
                }

                // 5. Clear runtime state
                _lastLoadedAt.Remove(pluginName);
                _errorMessages.Remove(pluginName);

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to uninstall plugin: {pluginName}",
                    ex,
                    source: "PluginService.UninstallPluginAsync",
                    module: "Plugin",
                    action: "Uninstall"
                );
                throw;
            }
        }

        public async Task<bool> EnablePluginAsync(string pluginName, string enabledBy)
        {
            try
            {
                var plugin = await GetPluginByNameAsync(pluginName);
                if (plugin == null)
                    return false;

                plugin.IsEnabled = true;
                await _fileSettingsService.AddOrUpdatePluginAsync(plugin);
                await LoadPluginAsync(plugin);

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to enable plugin: {pluginName}",
                    ex,
                    source: "PluginService.EnablePluginAsync",
                    module: "Plugin",
                    action: "Enable"
                );
                throw;
            }
        }

        public async Task<bool> DisablePluginAsync(string pluginName, string disabledBy)
        {
            try
            {
                var plugin = await GetPluginByNameAsync(pluginName);
                if (plugin == null)
                    return false;

                // Unload from memory if loaded
                if (_pluginManager.IsPluginLoaded(pluginName))
                {
                    await _pluginManager.UnloadPluginAsync(pluginName);
                    _lastLoadedAt.Remove(pluginName);
                    _errorMessages.Remove(pluginName);
                }

                plugin.IsEnabled = false;
                await _fileSettingsService.AddOrUpdatePluginAsync(plugin);

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to disable plugin: {pluginName}",
                    ex,
                    source: "PluginService.DisablePluginAsync",
                    module: "Plugin",
                    action: "Disable"
                );
                throw;
            }
        }

        public async Task<bool> UpdatePluginConfigurationAsync(string pluginName, string? configuration, string updatedBy)
        {
            try
            {
                var plugin = await GetPluginByNameAsync(pluginName);
                if (plugin == null)
                    return false;

                plugin.Configuration = configuration;
                await _fileSettingsService.AddOrUpdatePluginAsync(plugin);

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to update plugin configuration: {pluginName}",
                    ex,
                    source: "PluginService.UpdatePluginConfigurationAsync",
                    module: "Plugin",
                    action: "UpdateConfiguration"
                );
                throw;
            }
        }

        #endregion

        #region Runtime Operations

        public async Task<bool> ReloadPluginAsync(string pluginName)
        {
            var plugin = await GetPluginByNameAsync(pluginName);
            if (plugin == null)
                return false;

            await UnloadPluginAsync(plugin);
            return await LoadPluginAsync(plugin);
        }

        public async Task<PluginStatusResult?> GetPluginStatusAsync(string pluginName)
        {
            var plugin = await GetPluginByNameAsync(pluginName);
            if (plugin == null)
                return null;

            // Combine file state with actual runtime state
            var isActuallyLoaded = _pluginManager.IsPluginLoaded(pluginName);
            _lastLoadedAt.TryGetValue(pluginName, out var lastLoadedAt);
            _errorMessages.TryGetValue(pluginName, out var errorMessage);

            return new PluginStatusResult
            {
                Name = plugin.Name,
                Version = plugin.Version ?? "1.0.0",
                Description = plugin.Description,
                AssemblyPath = plugin.DllFileName,
                IsEnabled = plugin.IsEnabled,
                IsLoaded = isActuallyLoaded,
                LastLoadedAt = lastLoadedAt,
                ErrorMessage = errorMessage
            };
        }

        public bool IsPluginLoaded(string pluginName)
        {
            return _pluginManager.IsPluginLoaded(pluginName);
        }

        #endregion

        #region Lifecycle Operations

        public async Task InitializeAllPluginsAsync()
        {
            try
            {
                // Clear runtime states
                _lastLoadedAt.Clear();
                _errorMessages.Clear();

                // Load all enabled plugins
                var settings = await _fileSettingsService.LoadSettingsAsync();
                var enabledPlugins = settings.Plugins.Where(p => p.IsEnabled).ToList();
                
                if (enabledPlugins.Count == 0)
                {
                    return;
                }
                
                foreach (var plugin in enabledPlugins)
                {
                    try
                    {
                        // Check if plugin is already loaded
                        if (_pluginManager.IsPluginLoaded(plugin.Name))
                        {
                            continue;
                        }
                        
                        await LoadPluginAsync(plugin);
                    }
                    catch (Exception ex)
                    {
                        await _loggingService.LogErrorAsync(
                            $"Failed to load plugin during initialization: {plugin.Name}",
                            ex,
                            module: "PluginService"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to initialize plugins",
                    ex,
                    source: "PluginService.InitializeAllPluginsAsync",
                    module: "Plugin",
                    action: "InitializeAll"
                );
            }
        }

        public async Task ShutdownAllPluginsAsync()
        {
            try
            {
                var loadedPlugins = _pluginManager.GetAllPlugins().ToList();
                foreach (var plugin in loadedPlugins)
                {
                    try
                    {
                        await _pluginManager.UnloadPluginAsync(plugin.Name);
                        _lastLoadedAt.Remove(plugin.Name);
                        _errorMessages.Remove(plugin.Name);
                    }
                    catch (Exception ex)
                    {
                        await _loggingService.LogErrorAsync(
                            $"Failed to unload plugin during shutdown: {plugin.Name}",
                            ex,
                            module: "PluginService"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to shutdown plugins",
                    ex,
                    source: "PluginService.ShutdownAllPluginsAsync",
                    module: "Plugin",
                    action: "ShutdownAll"
                );
            }
        }

        #endregion

        #region Validation Operations

        public async Task<IEnumerable<string>> GetMissingDependenciesAsync(string pluginName)
        {
            var plugin = await GetPluginByNameAsync(pluginName);
            if (plugin == null || string.IsNullOrEmpty(plugin.Configuration))
                return [];

            try
            {
                // Try to parse dependencies from configuration (if stored as JSON)
                var configJson = JsonSerializer.Deserialize<Dictionary<string, object>>(plugin.Configuration);
                if (configJson != null && configJson.TryGetValue("dependencies", out var depsObj))
                {
                    var dependencies = JsonSerializer.Deserialize<List<string>>(depsObj.ToString() ?? "[]") ?? [];
                    var missingDeps = new List<string>();

                    foreach (var dep in dependencies)
                    {
                        var exists = await PluginExistsAsync(dep);
                        if (!exists)
                            missingDeps.Add(dep);
                    }

                    return missingDeps;
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return [];
        }

        public async Task<IEnumerable<PluginConfig>> GetDependentPluginsAsync(string pluginName)
        {
            var allPlugins = await GetAllPluginsAsync();
            var dependentPlugins = new List<PluginConfig>();

            foreach (var plugin in allPlugins)
            {
                if (string.IsNullOrEmpty(plugin.Configuration))
                    continue;

                try
                {
                    var configJson = JsonSerializer.Deserialize<Dictionary<string, object>>(plugin.Configuration);
                    if (configJson != null && configJson.TryGetValue("dependencies", out var depsObj))
                    {
                        var dependencies = JsonSerializer.Deserialize<List<string>>(depsObj.ToString() ?? "[]") ?? [];
                        if (dependencies.Contains(pluginName, StringComparer.OrdinalIgnoreCase))
                        {
                            dependentPlugins.Add(plugin);
                        }
                    }
                }
                catch
                {
                    // Ignore parsing errors
                }
            }

            return dependentPlugins;
        }

        public async Task<bool> PluginExistsAsync(string pluginName)
        {
            var plugin = await GetPluginByNameAsync(pluginName);
            return plugin != null;
        }

        #endregion

        #region Private Helpers

        private async Task<bool> LoadPluginAsync(PluginConfig plugin)
        {
            try
            {
                var pluginPath = Path.Combine(_pluginsDirectory, plugin.DllFileName);
                
                if (!File.Exists(pluginPath))
                {
                    var errorMsg = $"Plugin file not found: {pluginPath}";
                    _errorMessages[plugin.Name] = errorMsg;
                    return false;
                }

                // Check if plugin is already loaded by name
                var loadedPlugin = _pluginManager.GetPlugin(plugin.Name);
                if (loadedPlugin != null)
                {
                    _lastLoadedAt[plugin.Name] = DateTime.UtcNow;
                    _errorMessages.Remove(plugin.Name);
                    return true;
                }

                await _pluginManager.LoadPluginAsync(pluginPath);
                
                // Verify plugin was actually loaded
                var actualPlugin = _pluginManager.GetPlugin(plugin.Name);
                if (actualPlugin == null)
                {
                    // Plugin might have been loaded with a different name - check all loaded plugins
                    var allLoaded = _pluginManager.GetAllPlugins().ToList();
                    var loadedByName = allLoaded.FirstOrDefault(p => 
                        p.Name.Equals(plugin.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (loadedByName == null)
                    {
                        _errorMessages[plugin.Name] = $"Plugin loaded but name mismatch. Expected: {plugin.Name}";
                        return false;
                    }
                }
                
                _lastLoadedAt[plugin.Name] = DateTime.UtcNow;
                _errorMessages.Remove(plugin.Name);

                return true;
            }
            catch (Exception ex)
            {
                _errorMessages[plugin.Name] = ex.Message;
                await _loggingService.LogErrorAsync(
                    $"Failed to load plugin: {plugin.Name}",
                    ex,
                    source: "PluginService.LoadPluginAsync",
                    module: "Plugin",
                    action: "Load"
                );
                return false;
            }
        }

        private async Task<bool> UnloadPluginAsync(PluginConfig plugin)
        {
            try
            {
                if (!_pluginManager.IsPluginLoaded(plugin.Name))
                    return true;

                await _pluginManager.UnloadPluginAsync(plugin.Name);
                _lastLoadedAt.Remove(plugin.Name);
                _errorMessages.Remove(plugin.Name);

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to unload plugin: {plugin.Name}",
                    ex,
                    source: "PluginService.UnloadPluginAsync",
                    module: "Plugin",
                    action: "Unload"
                );
                return false;
            }
        }

        /// <summary>
        /// Get absolute path for plugins directory based on application base directory
        /// </summary>
        private static string GetPluginsDirectory(IConfiguration configuration)
        {
            // Get relative path from configuration or use default
            var settingPluginDirectory = configuration["Plugins:Directory"];
            var relativePath = string.IsNullOrEmpty(settingPluginDirectory) ? "plugins" : settingPluginDirectory;
            
            // If already absolute path, return as is
            if (Path.IsPathRooted(relativePath))
                return relativePath;
            
            // Get application base directory (deployment directory)
            var baseDirectory = AppContext.BaseDirectory ?? AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory();
            
            // Combine to get absolute path
            var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, relativePath));
            return fullPath;
        }

        private sealed class NormalizedPluginNames
        {
            public HashSet<string> PluginNames { get; } = new(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> FileNamesWithExtension { get; } = new(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> FileNamesWithoutExtension { get; } = new(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> FullPaths { get; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, int> PluginPriorities { get; } = new(StringComparer.OrdinalIgnoreCase);
            
            public bool HasAnyFilter =>
                PluginNames.Count > 0 ||
                FileNamesWithExtension.Count > 0 ||
                FileNamesWithoutExtension.Count > 0 ||
                FullPaths.Count > 0;
            
            /// <summary>
            /// Get priority for a plugin name (1-based index from original array)
            /// Returns 0 if plugin name not found
            /// </summary>
            public int GetPriority(string pluginName)
            {
                return PluginPriorities.TryGetValue(pluginName, out var priority) ? priority : 0;
            }
        }

        private static NormalizedPluginNames NormalizePluginNames(string[]? pluginNames)
        {
            var result = new NormalizedPluginNames();
            if (pluginNames == null || pluginNames.Length == 0)
                return result;

            for (int index = 0; index < pluginNames.Length; index++)
            {
                var raw = pluginNames[index];
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var value = raw.Trim();
                var hasPath = value.Contains(Path.DirectorySeparatorChar) || value.Contains(Path.AltDirectorySeparatorChar);
                var isDllFile = value.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                
                // Full path
                if (hasPath)
                    result.FullPaths.Add(value);

                // Filename with extension
                if (isDllFile)
                {
                    var fileName = Path.GetFileName(value);
                    result.FileNamesWithExtension.Add(fileName);
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    result.FileNamesWithoutExtension.Add(fileNameWithoutExt);
                    
                    // Set priority for plugin name (1-based index)
                    var priority = index + 1;
                    if (!result.PluginPriorities.ContainsKey(fileNameWithoutExt))
                    {
                        result.PluginPriorities[fileNameWithoutExt] = priority;
                    }
                }
                else // Logical plugin name
                {
                    result.FileNamesWithoutExtension.Add(value);
                    
                    // Set priority for plugin name (1-based index)
                    var priority = index + 1;
                    if (!result.PluginPriorities.ContainsKey(value))
                    {
                        result.PluginPriorities[value] = priority;
                    }
                }
                
                // Metadata plugin name (ALWAYS logical name)
                var pluginName = isDllFile ? Path.GetFileNameWithoutExtension(value) : value;
                result.PluginNames.Add(pluginName);
            }

            return result;
        }

        private static bool MatchesPluginName(string dllPath, NormalizedPluginNames normalized)
        {
            if (!normalized.HasAnyFilter)
                return true;

            var fileName = Path.GetFileName(dllPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(dllPath);

            return
                normalized.FullPaths.Contains(dllPath) ||
                normalized.FileNamesWithExtension.Contains(fileName) ||
                normalized.FileNamesWithoutExtension.Contains(fileNameWithoutExt);
        }

        #endregion
    }
}
