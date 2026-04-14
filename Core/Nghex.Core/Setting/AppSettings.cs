namespace Nghex.Core.Setting
{
    public static class AppSettings
    {
        public static int TokenRetentionDuration { get; set; } = 60;
        public static int LogRetentionDays { get; set; } = 30;
        public static string PluginSettingsInfo { get; set; } = string.Empty;
        public static string DefaultDataSchema { get; set; } = string.Empty;
        public static bool IsInitialized { get; set; } = false;
        public static int InitialDelay { get; } = 10000;

        public static string ApplicationDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".omed");

        public static LicenseFile LicenseFile => new()
        {
            Key = Path.Combine(ApplicationDataPath, "license.key"),
            Backup = Path.Combine(ApplicationDataPath, ".trial_backup")
        };

        /// <summary>
        /// Initialize from DB config values.
        /// Caller (app or Nghex.Configuration) resolves values via IConfigurationService then passes them here.
        /// This keeps Core decoupled from the Configuration package.
        /// </summary>
        public static async Task InitializeAsync(Func<string, int, Task<int>> getInt, Func<string, string, Task<string>> getString)
        {
            try
            {
                TokenRetentionDuration = await getInt("TOKEN_RETENTION_DURATION", 60);
                LogRetentionDays       = await getInt("LOG_RETENTION_DAYS", 30);
                PluginSettingsInfo     = await getString("PLUGIN_SETTINGS", Path.Combine("plugins", "pluginSettings.json"));
                DefaultDataSchema      = await getString("DEFAULT_DATA_SCHEMA", "DWHBV");
                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize app settings: {ex.Message}");
            }
        }

    }
    public struct LicenseFile
    {
        public string Key { get; set; }
        public string Backup { get; set; }
    }
}