using System.Text.Json;
using Nghex.Core.Setting;
using Nghex.Plugins.Models;

namespace Nghex.Plugins.Services
{
    /// <summary>
    /// Service to manage plugin settings from JSON file
    /// </summary>
    public class PluginSettingsService()
    {
        private static string SettingsFilePath => AppSettings.IsInitialized && !string.IsNullOrEmpty(AppSettings.PluginSettingsInfo)
                    ? AppSettings.PluginSettingsInfo
                    : Path.Combine("plugins", "pluginSettings.json");

        public async Task<PluginSettings> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    var defaultSettings = new PluginSettings();
                    await SaveSettingsAsync(defaultSettings);
                    return defaultSettings;
                }

                var json = await File.ReadAllTextAsync(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<PluginSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                });

                return settings ?? new PluginSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugin settings from {SettingsFilePath} - {ex.Message}");
                return new PluginSettings();
            }
        }

        public async Task SaveSettingsAsync(PluginSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save plugin settings to {SettingsFilePath} - {ex.Message}");
            }
        }

        public async Task AddOrUpdatePluginAsync(PluginConfig pluginConfig)
        {
            var settings = await LoadSettingsAsync();
            var existingPlugin = settings.Plugins.FirstOrDefault(p => p.Name == pluginConfig.Name);

            if (existingPlugin != null)
            {
                existingPlugin.DllFileName = pluginConfig.DllFileName;
                existingPlugin.IsEnabled = pluginConfig.IsEnabled;
                existingPlugin.Version = pluginConfig.Version ?? existingPlugin.Version;
                existingPlugin.Description = pluginConfig.Description ?? existingPlugin.Description;
                existingPlugin.Configuration = pluginConfig.Configuration ?? existingPlugin.Configuration;
            }
            else
            {
                settings.Plugins.Add(pluginConfig);
            }

            await SaveSettingsAsync(settings);
        }

        public async Task RemovePluginAsync(string pluginName)
        {
            var settings = await LoadSettingsAsync();
            settings.Plugins.RemoveAll(p => p.Name == pluginName);
            await SaveSettingsAsync(settings);
        }

        public async Task<IEnumerable<PluginConfig>> GetEnabledPluginsAsync()
        {
            var settings = await LoadSettingsAsync();
            return settings.Plugins.Where(p => p.IsEnabled);
        }
    }
}
