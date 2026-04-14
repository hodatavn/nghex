using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Nghex.Logging.Providers
{
    /// <summary>
    /// Options for file logging fallback
    /// </summary>
    public class FileLoggerOptions
    {
        /// <summary>
        /// Enable file logging fallback
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Directory to store log files
        /// </summary>
        public string Directory { get; set; } = "logs";

        /// <summary>
        /// Log file name
        /// </summary>
        public string FileName { get; set; } = "omed.log";

        /// <summary>
        /// Max file size in MB before rolling
        /// </summary>
        public int MaxFileSizeMb { get; set; } = 10;

        /// <summary>
        /// Days to retain rolled files
        /// </summary>
        public int RetentionDays { get; set; } = 7;

        /// <summary>
        /// Minimum log level for file fallback
        /// </summary>
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

        public static FileLoggerOptions DefaultOptions(IConfiguration? configuration = null)
        {
            // Use a very defensive configuration loader so that missing/invalid appsettings.json
            // never prevents the logging assembly from being loaded (especially on Linux deployments)
            try
            {
                configuration ??= new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .Build();
            }
            catch
            {
                // Fall back to an empty configuration if anything goes wrong
                configuration = new ConfigurationBuilder().Build();
            }

            var section = configuration.GetSection("FileLogging");
            return new FileLoggerOptions
            {
                Enabled = GetOrDefault(section, "Enabled", true),
                Directory = GetOrDefault(section, "Directory", "logs"),
                FileName = GetOrDefault(section, "FileName", "omed.log"),
                MaxFileSizeMb = GetOrDefault(section, "MaxFileSizeMb", 10),
                RetentionDays = GetOrDefault(section, "RetentionDays", 7),
                MinimumLogLevel = ParseLogLevel(section["MinimumLogLevel"]) ?? LogLevel.Information
            };
        }


        private static LogLevel? ParseLogLevel(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (System.Enum.TryParse<LogLevel>(value, true, out var parsed)) return parsed;
            return null;
        }
    
        private static T GetOrDefault<T>(IConfiguration section, string key, T defaultValue)
        {
            try
            {
                var value = section[key];
                if (string.IsNullOrWhiteSpace(value)) return defaultValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}





























