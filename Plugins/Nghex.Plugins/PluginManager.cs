using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Nghex.Core.Logging;

namespace Nghex.Plugins
{
    /// <summary>
    /// Result of plugin analysis containing metadata and optionally the plugin instance
    /// </summary>
    public class PluginAnalysisResult
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Assembly Assembly { get; set; } = null!;
        public Type PluginType { get; set; } = null!;
        public PluginLoadContext LoadContext { get; set; } = null!;
        public IPlugin? PluginInstance { get; set; }
    }

    /// <summary>
    /// Plugin Manager implementation
    /// </summary>
    public partial class PluginManager : IPluginManager
    {
        private readonly ConcurrentDictionary<string, IPlugin> _loadedPlugins;
        private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _pluginDirectory;
        private readonly ConcurrentDictionary<string, PluginLoadContext> _contexts;

        public PluginManager(IServiceScopeFactory scopeFactory, string pluginDirectory = "plugins")
        {
            _scopeFactory = scopeFactory;
            _pluginDirectory = pluginDirectory;
            _loadedPlugins = new ConcurrentDictionary<string, IPlugin>();
            _loadedAssemblies = new ConcurrentDictionary<string, Assembly>();
            _contexts = new ConcurrentDictionary<string, PluginLoadContext>();

            // Create plugin directory if it doesn't exist
            if (!Directory.Exists(_pluginDirectory))
            {
                Directory.CreateDirectory(_pluginDirectory);
            }

            // Update global registry directory
            PluginRegistry.SetPluginDirectory(_pluginDirectory);
        }

        public async Task<IPlugin> LoadPluginAsync(string pluginPath)
        {
            try
            {
                if (!File.Exists(pluginPath))
                    throw new FileNotFoundException($"Plugin file not found: {pluginPath}");
                
                var alc = new PluginLoadContext(pluginPath);
                var asmBytes = await File.ReadAllBytesAsync(pluginPath);

                // Pre-load Nghex.Plugins into plugin context BEFORE loading plugin assembly
                // This ensures IPlugin interface is available when checking types
                try
                {
                    var corePluginsAssembly = typeof(PluginMetadataAttribute).Assembly;
                    var corePluginsLocation = corePluginsAssembly.Location;
                    
                    if (!string.IsNullOrEmpty(corePluginsLocation) && File.Exists(corePluginsLocation))
                    {
                        alc.LoadFromAssemblyPath(corePluginsLocation);
                    }
                    else
                    {
                        var corePluginsAssemblyName = corePluginsAssembly.GetName();
                        alc.LoadFromAssemblyName(corePluginsAssemblyName);
                    }
                }
                catch (Exception ex)
                {
                    await LogWarningAsync($"Failed to pre-load Nghex.Plugins into plugin context: {ex.Message}");
                }

                var assembly = alc.LoadPluginFromBytes(asmBytes, pdbBytes: null);
                
                // Find plugin type that implements IPlugin
                // Check by interface name to handle cross-AssemblyLoadContext scenarios
                var iPluginFullName = typeof(IPlugin).FullName ?? "Nghex.Plugins.IPlugin";
                var allTypes = GetLoadableTypes(assembly).ToList();
                var pluginType = allTypes.FirstOrDefault(t =>
                    t != null &&
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t.GetInterfaces().Any(i => i.FullName == iPluginFullName));
                    
                if (pluginType == null) 
                    throw new InvalidOperationException($"No plugin implementation found in {pluginPath}");

                // Create plugin instance using CreatePluginInstance to handle cross-context casting
                var plugin = CreatePluginInstance(assembly, pluginType);

                // Initialize plugin
                await plugin.InitializeAsync();

                // Store plugin and assembly
                _loadedPlugins[plugin.Name] = plugin;
                _loadedAssemblies[plugin.Name] = assembly;
                _contexts[plugin.Name] = alc;

                // Update global registry
                PluginRegistry.Register(plugin, assembly);

                await LogInformationAsync($"Plugin {plugin.Name} v{plugin.Version} loaded successfully");
                return plugin;
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"Failed to load plugin from {pluginPath}", ex);
                throw;
            }
        }

        public async Task UnloadPluginAsync(string pluginName)
        {
            try
            {
                if (!_loadedPlugins.TryRemove(pluginName, out var plugin))
                {
                    await LogWarningAsync($"Plugin {pluginName} is not loaded");
                    return;
                }

                // Cleanup plugin
                await plugin.CleanupAsync();

                // Remove assembly and unload context
                _loadedAssemblies.TryRemove(pluginName, out _);
                if (_contexts.TryRemove(pluginName, out var context))
                {
                    try { context.Unload(); } catch { }
                }

                // Update global registry
                PluginRegistry.Unregister(pluginName);

                await LogInformationAsync($"Plugin {pluginName} unloaded successfully");
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"Failed to unload plugin {pluginName}", ex);
                throw;
            }
        }

        public IPlugin? GetPlugin(string pluginName)
        {
            _loadedPlugins.TryGetValue(pluginName, out var plugin);
            return plugin;
        }

        public IEnumerable<IPlugin> GetAllPlugins()
        {
            return _loadedPlugins.Values;
        }

        public bool IsPluginLoaded(string pluginName)
        {
            return _loadedPlugins.ContainsKey(pluginName);
        }

        public async Task ReloadPluginAsync(string pluginName)
        {
            try
            {
                // Unload first
                await UnloadPluginAsync(pluginName);

                // Find plugin file
                var pluginFiles = Directory.GetFiles(_pluginDirectory, "*.dll", SearchOption.AllDirectories);
                var pluginFile = pluginFiles.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f)
                    .Equals(pluginName, StringComparison.OrdinalIgnoreCase)) ?? throw new FileNotFoundException($"Plugin file for {pluginName} not found");

                // Load again
                await LoadPluginAsync(pluginFile);
            }
            catch (Exception ex)
            {
                await LogErrorAsync($"Failed to reload plugin {pluginName}", ex);
                throw;
            }
        }

        /// <summary>
        /// Load all plugins in the directory
        /// </summary>
        public async Task LoadAllPluginsAsync()
        {
            var pluginFiles = Directory.GetFiles(_pluginDirectory, "*.dll", SearchOption.AllDirectories);

            foreach (var pluginFile in pluginFiles)
            {
                try
                {
                    await LoadPluginAsync(pluginFile);
                }
                catch (Exception ex)
                {
                    await LogErrorAsync($"Failed to load plugin from {pluginFile}", ex);
                }
            }
        }

        /// <summary>
        /// Get loaded assembly by plugin name
        /// </summary>
        public Assembly? GetPluginAssembly(string pluginName)
        {
            _loadedAssemblies.TryGetValue(pluginName, out var assembly);
            return assembly;
        }

        /// <summary>
        /// Get all loaded plugin assemblies
        /// </summary>
        public IEnumerable<Assembly> GetAllPluginAssemblies()
        {
            return _loadedAssemblies.Values;
        }


        /// <summary>
        /// Analyze plugin DLL and optionally create plugin instance
        /// This method provides unified plugin loading logic used by both install API and runtime loading
        /// </summary>
        /// <param name="pluginPath">Path to the plugin DLL file</param>
        /// <param name="createInstance">Whether to create plugin instance (default: true)</param>
        /// <returns>PluginAnalysisResult containing metadata and optionally the plugin instance</returns>
        public async Task<PluginAnalysisResult> AnalyzePluginAsync(string pluginPath, bool createInstance = true)
        {
            if (!File.Exists(pluginPath))
                throw new FileNotFoundException($"Plugin file not found: {pluginPath}");

            var alc = new PluginLoadContext(pluginPath);
            var asmBytes = await File.ReadAllBytesAsync(pluginPath);
            // PDB files are not included in production releases

            try
            {
                // Pre-load Nghex.Plugins into plugin context BEFORE loading plugin assembly
                // This ensures PluginMetadataAttribute and IPlugin types are available when needed
                // We load from default context's file path to maintain type compatibility
                try
                {
                    var corePluginsAssembly = typeof(PluginMetadataAttribute).Assembly;
                    var corePluginsLocation = corePluginsAssembly.Location;
                    
                    if (!string.IsNullOrEmpty(corePluginsLocation) && File.Exists(corePluginsLocation))
                    {
                        // Load from file path - this will trigger PluginLoadContext.Load() which
                        // will load it from the same file, maintaining type compatibility
                        alc.LoadFromAssemblyPath(corePluginsLocation);
                    }
                    else
                    {
                        // Fallback: try loading by assembly name (will use PluginLoadContext.Load())
                        var corePluginsAssemblyName = corePluginsAssembly.GetName();
                        alc.LoadFromAssemblyName(corePluginsAssemblyName);
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue - GetCustomAttributesData might still work via fallback
                    await LogWarningAsync($"Failed to pre-load Nghex.Plugins into plugin context: {ex.Message}");
                }

                var assembly = alc.LoadPluginFromBytes(asmBytes, pdbBytes: null);
                if (assembly == null)
                {
                    throw new InvalidOperationException($"Failed to load assembly from {pluginPath}");
                }

                // Find plugin type that implements IPlugin
                // Check by interface name to handle cross-AssemblyLoadContext scenarios
                var iPluginFullName = typeof(IPlugin).FullName ?? "Nghex.Plugins.IPlugin";
                var allTypes = GetLoadableTypes(assembly).ToList();
                var pluginType = allTypes.FirstOrDefault(t =>
                    t != null &&
                    !t.IsAbstract &&
                    !t.IsInterface &&
                    t.GetInterfaces().Any(i => i.FullName == iPluginFullName));

                if (pluginType == null)
                {
                    throw new InvalidOperationException($"No plugin implementation found in {pluginPath}");
                }

                // Get metadata from PluginMetadataAttribute (required)
                var pluginInfo = await ExtractPluginMetadataAsync(pluginType, assembly);
                if (pluginInfo == null)
                {
                    throw new InvalidOperationException($"Failed to extract plugin metadata from {pluginType.Name}");
                }

                // Create plugin instance if requested
                IPlugin? pluginInstance = null;
                if (createInstance)
                {
                    pluginInstance = CreatePluginInstance(assembly, pluginType);
                }

                return new PluginAnalysisResult
                {
                    Name = pluginInfo.Name,
                    Version = pluginInfo.Version,
                    Description = pluginInfo.Description,
                    Assembly = assembly,
                    PluginType = pluginType,
                    LoadContext = alc,
                    PluginInstance = pluginInstance
                };
            }
            catch
            {
                // Cleanup context on error
                try { alc.Unload(); } catch { }
                throw;
            }
        }

        /// <summary>
        /// Extract plugin metadata from PluginMetadataAttribute
        /// </summary>
        private async Task<PluginMetadata?> ExtractPluginMetadataAsync(Type pluginType, Assembly assembly)
        {
            IList<CustomAttributeData>? allAttributes = null;
            try
            {
                allAttributes = pluginType.GetCustomAttributesData();
            }
            catch (Exception ex)
            {
                await LogWarningAsync($"Failed to get custom attributes data from {pluginType.Name} in {assembly.FullName}: {ex.GetType().Name} - {ex.Message}");
                return null;
            }

            if (allAttributes == null || allAttributes.Count == 0)
            {
                await LogWarningAsync($"No custom attributes found on {pluginType.Name}");
                return null;
            }

            CustomAttributeData? metadataAttr = null;
            var pluginMetadataAttributeFullName = typeof(PluginMetadataAttribute).FullName ?? "Nghex.Plugins.PluginMetadataAttribute";

            // Find attribute by name without accessing AttributeType.FullName (which may trigger type resolution)
            foreach (var attr in allAttributes)
            {
                try
                {
                    var attrTypeName = attr.AttributeType.Name;
                    if (attrTypeName != "PluginMetadataAttribute")
                        continue;

                    string? attrTypeFullName = null;
                    try
                    {
                        attrTypeFullName = attr.AttributeType.FullName;
                    }
                    catch
                    {
                        if (attrTypeName == "PluginMetadataAttribute")
                        {
                            metadataAttr = attr;
                            break;
                        }
                        continue;
                    }

                    if (attrTypeFullName == pluginMetadataAttributeFullName ||
                        (attrTypeFullName?.Contains("Nghex.Plugins.PluginMetadataAttribute") == true))
                    {
                        metadataAttr = attr;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    await LogWarningAsync($"Skipping attribute due to error: {ex.GetType().Name} - {ex.Message}");
                    continue;
                }
            }

            if (metadataAttr == null)
            {
                await LogWarningAsync($"PluginMetadataAttribute not found on {pluginType.Name}. Found {allAttributes.Count} attribute(s)");
                return null;
            }

            if (metadataAttr.ConstructorArguments.Count < 3)
            {
                await LogWarningAsync($"PluginMetadataAttribute on {pluginType.Name} has insufficient constructor arguments ({metadataAttr.ConstructorArguments.Count} < 3)");
                return null;
            }

            try
            {
                var name = metadataAttr.ConstructorArguments[0].Value?.ToString() ?? "";
                var version = metadataAttr.ConstructorArguments[1].Value?.ToString() ?? "";
                var description = metadataAttr.ConstructorArguments[2].Value?.ToString() ?? "";

                if (string.IsNullOrEmpty(name))
                {
                    await LogWarningAsync($"PluginMetadataAttribute on {pluginType.Name} has empty name");
                    return null;
                }

                return new PluginMetadata
                {
                    Name = name,
                    Version = version,
                    Description = description
                };
            }
            catch (Exception ex)
            {
                await LogWarningAsync($"Error extracting PluginMetadataAttribute values from {pluginType.Name}: {ex.GetType().Name} - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get loadable types from assembly, handling ReflectionTypeLoadException
        /// </summary>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types?.Where(t => t != null).Cast<Type>() ?? Enumerable.Empty<Type>();
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        /// <summary>
        /// Plugin metadata extracted from PluginMetadataAttribute
        /// </summary>
        private class PluginMetadata
        {
            public string Name { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        /// <summary>
        /// Create plugin instance from assembly and type
        /// This method handles cross-AssemblyLoadContext scenarios by using reflection to properly cast
        /// </summary>
        /// <param name="assembly">The assembly containing the plugin type</param>
        /// <param name="pluginType">The plugin type to instantiate</param>
        /// <returns>The created plugin instance</returns>
        /// <exception cref="InvalidOperationException">Thrown when instance creation fails</exception>
        private IPlugin CreatePluginInstance(Assembly assembly, Type pluginType)
        {
            object? instance = null;
            
            // Try to create instance using assembly.CreateInstance (works within the same context)
            try
            {
                instance = assembly.CreateInstance(pluginType.FullName ?? pluginType.Name);
            }
            catch
            {
                // Fallback to Activator if assembly.CreateInstance fails
                try
                {
                    instance = Activator.CreateInstance(pluginType);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to create plugin instance from {pluginType.Name}. " +
                        $"Assembly: {assembly.FullName}, Type: {pluginType.FullName}. " +
                        $"Error: {ex.Message}", ex);
                }
            }

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create plugin instance from {pluginType.Name}. " +
                    $"Instance creation returned null. " +
                    $"Assembly: {assembly.FullName}, Type: {pluginType.FullName}");
            }

            // Handle cross-AssemblyLoadContext casting issue
            // When types are in different contexts, direct casting may fail due to type identity
            IPlugin? plugin = null;
            
            // First, try direct cast (should work if Nghex.Plugins is loaded correctly in plugin context)
            plugin = instance as IPlugin;
            
            if (plugin == null)
            {
                // If direct cast fails, it's likely a type identity issue between contexts
                // Check if the type actually implements IPlugin by checking for required members
                var hasName = pluginType.GetProperty("Name") != null;
                var hasVersion = pluginType.GetProperty("Version") != null;
                var hasDescription = pluginType.GetProperty("Description") != null;
                var hasIsEnabled = pluginType.GetProperty("IsEnabled") != null;
                var hasInitializeAsync = pluginType.GetMethod("InitializeAsync") != null;
                var hasCleanupAsync = pluginType.GetMethod("CleanupAsync") != null;
                
                if (hasName && hasVersion && hasDescription && hasIsEnabled && hasInitializeAsync && hasCleanupAsync)
                {
                    // Type has all IPlugin members - create wrapper to bridge the context gap
                    plugin = new PluginWrapper(instance, pluginType);
                }
                else
                {
                    // Type doesn't have required IPlugin members
                    throw new InvalidOperationException(
                        $"Plugin type {pluginType.Name} does not implement required IPlugin members. " +
                        $"Missing: Name={!hasName}, Version={!hasVersion}, Description={!hasDescription}, " +
                        $"IsEnabled={!hasIsEnabled}, InitializeAsync={!hasInitializeAsync}, CleanupAsync={!hasCleanupAsync}");
                }
            }

            return plugin;
        }

        /// <summary>
        /// Wrapper class to handle cross-AssemblyLoadContext plugin instances
        /// This wrapper implements IPlugin and delegates calls to the actual plugin instance
        /// </summary>
        private class PluginWrapper : IPlugin
        {
            private readonly object _instance;
            private readonly Type _pluginType;
            private readonly PropertyInfo _nameProp;
            private readonly PropertyInfo _versionProp;
            private readonly PropertyInfo _descriptionProp;
            private readonly PropertyInfo _isEnabledProp;
            private readonly MethodInfo _initializeMethod;
            private readonly MethodInfo _cleanupMethod;

            public PluginWrapper(object instance, Type pluginType)
            {
                _instance = instance ?? throw new ArgumentNullException(nameof(instance));
                _pluginType = pluginType ?? throw new ArgumentNullException(nameof(pluginType));
                
                _nameProp = pluginType.GetProperty("Name") ?? throw new InvalidOperationException("Plugin type must have Name property");
                _versionProp = pluginType.GetProperty("Version") ?? throw new InvalidOperationException("Plugin type must have Version property");
                _descriptionProp = pluginType.GetProperty("Description") ?? throw new InvalidOperationException("Plugin type must have Description property");
                _isEnabledProp = pluginType.GetProperty("IsEnabled") ?? throw new InvalidOperationException("Plugin type must have IsEnabled property");
                _initializeMethod = pluginType.GetMethod("InitializeAsync") ?? throw new InvalidOperationException("Plugin type must have InitializeAsync method");
                _cleanupMethod = pluginType.GetMethod("CleanupAsync") ?? throw new InvalidOperationException("Plugin type must have CleanupAsync method");
            }

            public string Name => _nameProp.GetValue(_instance)?.ToString() ?? string.Empty;
            public string Version => _versionProp.GetValue(_instance)?.ToString() ?? string.Empty;
            public string Description => _descriptionProp.GetValue(_instance)?.ToString() ?? string.Empty;
            public bool IsEnabled => _isEnabledProp.GetValue(_instance) as bool? ?? false;

            public Task InitializeAsync()
            {
                var result = _initializeMethod.Invoke(_instance, null);
                return result as Task ?? Task.CompletedTask;
            }

            public Task CleanupAsync()
            {
                var result = _cleanupMethod.Invoke(_instance, null);
                return result as Task ?? Task.CompletedTask;
            }

            /// <summary>
            /// Get the underlying plugin instance (for casting to IApiPlugin, IServicePlugin, etc.)
            /// </summary>
            public object GetInstance() => _instance;
            
            /// <summary>
            /// Check if the underlying instance implements a specific interface
            /// </summary>
            public bool ImplementsInterface<T>() where T : class
            {
                var interfaceType = typeof(T);
                return _pluginType.GetInterfaces().Any(i => 
                    i.FullName == interfaceType.FullName || 
                    (i.Name == interfaceType.Name && i.Namespace == interfaceType.Namespace));
            }
        }

        // Shadow copy removed; assemblies are loaded from memory streams via PluginLoadContext
    }

    // Private logging helpers resolving scoped ILoggingService from a singleton via scope factory
    public partial class PluginManager
    {
        private async Task LogInformationAsync(string message, object? details = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var logging = scope.ServiceProvider.GetRequiredService<ILogging>();
            await logging.LogInformationAsync(message, module: $"Plugin: {nameof(PluginManager)}", details: details);
        }

        private async Task LogWarningAsync(string message, object? details = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var logging = scope.ServiceProvider.GetRequiredService<ILogging>();
            await logging.LogWarningAsync(message, module: $"Plugin: {nameof(PluginManager)}", details: details);
        }

        private async Task LogErrorAsync(string message, Exception? exception = null, object? details = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var logging = scope.ServiceProvider.GetRequiredService<ILogging>();
            await logging.LogErrorAsync(message, exception, module: $"Plugin: {nameof(PluginManager)}", details: details);
        }
    }
}





