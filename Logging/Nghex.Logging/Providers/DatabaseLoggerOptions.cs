using Microsoft.Extensions.Logging;

namespace Nghex.Logging.Providers
{
    /// <summary>
    /// Options cho Database Logger
    /// </summary>
    public class DatabaseLoggerOptions
    {
        /// <summary>
        /// Minimum log level để ghi vào database
        /// </summary>
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Có ghi logs vào database hay không
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Maximum số lượng logs ghi đồng thời
        /// </summary>
        public int MaxConcurrentLogs { get; set; } = 100;

        /// <summary>
        /// Timeout cho việc ghi log vào database (milliseconds)
        /// </summary>
        public int DatabaseTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Có ghi exception details hay không
        /// </summary>
        public bool IncludeExceptionDetails { get; set; } = true;

        /// <summary>
        /// Có ghi structured logging data hay không
        /// </summary>
        public bool IncludeStructuredData { get; set; } = true;
    }
}
