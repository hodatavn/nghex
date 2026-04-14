namespace Nghex.Web.AspNetCore.Models
{
    /// <summary>
    /// Performance tracking options
    /// </summary>
    public class PerformanceTrackingOptions
    {
        /// <summary>
        /// Enable/disable performance tracking
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Minimum processing time to log warning (milliseconds)
        /// </summary>
        public long MinProcessingTimeMs { get; set; } = 100;

        /// <summary>
        /// Enable in development environment
        /// </summary>
        public bool EnableInDevelopment { get; set; } = true;

        /// <summary>
        /// Enable in production environment
        /// </summary>
        public bool EnableInProduction { get; set; } = false;

        /// <summary>
        /// Enable in staging environment
        /// </summary>
        public bool EnableInStaging { get; set; } = true;

        /// <summary>
        /// Log performance metrics when threshold is exceeded
        /// </summary>
        public bool LogSlowRequests { get; set; } = true;

        /// <summary>
        /// Log level for slow requests (Information, Warning, Error)
        /// </summary>
        public string SlowRequestLogLevel { get; set; } = "Warning";

        /// <summary>
        /// Include processing time in response
        /// </summary>
        public bool IncludeProcessingTimeInResponse { get; set; } = true;
    }
}

