using System.Collections.Concurrent;
using System.Reflection;

namespace Nghex.Plugins
{
    /// <summary>
    /// Global registry for plugins and related metadata
    /// </summary>
    public static class PluginRegistry
    {
        private static readonly ConcurrentDictionary<string, IPlugin> _plugins = new();
        private static readonly ConcurrentDictionary<string, Assembly> _assemblies = new();

        /// <summary>
        /// Directory where plugins are located
        /// </summary>
        public static string PluginDirectory { get; private set; } = "plugins";

        /// <summary>
        /// Enabled plugin names loaded from settings
        /// </summary>
        public static List<string> EnabledPluginNames { get; private set; } = new();

        public static void SetPluginDirectory(string directory)
        {
            if (!string.IsNullOrWhiteSpace(directory))
                PluginDirectory = directory;
        }

        public static void SetEnabledPlugins(IEnumerable<string> pluginNames)
        {
            EnabledPluginNames = pluginNames?.ToList() ?? new List<string>();
        }

        public static void Register(IPlugin plugin, Assembly assembly)
        {
            _plugins[plugin.Name] = plugin;
            _assemblies[plugin.Name] = assembly;
        }

        public static void Unregister(string pluginName)
        {
            _plugins.TryRemove(pluginName, out _);
            _assemblies.TryRemove(pluginName, out _);
        }

        public static IPlugin? Get(string pluginName)
        {
            _plugins.TryGetValue(pluginName, out var plugin);
            return plugin;
        }

        public static IEnumerable<IPlugin> GetAll()
        {
            return _plugins.Values;
        }

        public static Assembly? GetAssembly(string pluginName)
        {
            _assemblies.TryGetValue(pluginName, out var assembly);
            return assembly;
        }

        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            return _assemblies.Values;
        }
    }
}


