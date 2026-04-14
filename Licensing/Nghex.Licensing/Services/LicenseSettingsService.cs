using System.Text.Json;
using Nghex.Licensing.Models;
using Nghex.Licensing.Interfaces;

namespace Nghex.Licensing.Services
{
    /// <summary>
    /// Service to manage license settings from JSON file
    /// </summary>
    public class LicenseSettingsService : ILicenseSettingsService
    {
        private readonly string _settingsFilePath;

        public LicenseSettingsService()
        {
            _settingsFilePath = Path.Combine("data", "licenseSetting.json");
        }

        /// <summary>
        /// Load license settings from JSON file
        /// </summary>
        public async Task<LicenseSettings> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    // Create default settings file
                    var defaultSettings = new LicenseSettings();
                    await SaveSettingsAsync(defaultSettings);
                    return defaultSettings;
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<LicenseSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                });

                return settings ?? new LicenseSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to load license settings from {_settingsFilePath} - {ex.Message}");
                return new LicenseSettings();
            }
        }

        /// <summary>
        /// Save license settings to JSON file
        /// </summary>
        public async Task SaveSettingsAsync(LicenseSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to save license settings to {_settingsFilePath} - {ex.Message}");
            }
        }
    }
}
