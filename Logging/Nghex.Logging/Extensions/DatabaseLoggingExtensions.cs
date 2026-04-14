using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Nghex.Logging.Providers;

namespace Nghex.Logging.Extensions
{
    /// <summary>
    /// Extension methods để đăng ký Database Logger
    /// </summary>
    public static class DatabaseLoggingExtensions
    {
        /// <summary>
        /// Thêm Database Logger vào logging pipeline
        /// </summary>
        public static ILoggingBuilder AddDatabaseLogging(this ILoggingBuilder builder, IConfiguration configuration)
        {
            // Note: DatabaseLoggerProvider uses ILoggingService which depends on ILogRepository
            // Both are registered in Startup.cs with Scoped lifetime
            // No need to register them here

            // Đăng ký options
            builder.Services.Configure<DatabaseLoggerOptions>(options =>
            {
                var section = configuration.GetSection("DatabaseLogging");
                options.Enabled = bool.Parse(section["Enabled"] ?? "true");
                options.MinimumLogLevel = System.Enum.Parse<LogLevel>(section["MinimumLogLevel"] ?? "Information");
                options.MaxConcurrentLogs = int.Parse(section["MaxConcurrentLogs"] ?? "100");
                options.DatabaseTimeoutMs = int.Parse(section["DatabaseTimeoutMs"] ?? "5000");
                options.IncludeExceptionDetails = bool.Parse(section["IncludeExceptionDetails"] ?? "true");
                options.IncludeStructuredData = bool.Parse(section["IncludeStructuredData"] ?? "true");
            });

            // Đăng ký provider với Scoped lifetime
            builder.Services.AddScoped<ILoggerProvider, DatabaseLoggerProvider>();

            return builder;
        }

        /// <summary>
        /// Thêm Database Logger với custom options
        /// </summary>
        public static ILoggingBuilder AddDatabaseLogging(this ILoggingBuilder builder, Action<DatabaseLoggerOptions> configureOptions)
        {
            // Note: DatabaseLoggerProvider uses ILoggingService which depends on ILogRepository
            // Both are registered in Startup.cs with Scoped lifetime
            // No need to register them here

            // Đăng ký options
            builder.Services.Configure<DatabaseLoggerOptions>(configureOptions);

            // Đăng ký provider với Scoped lifetime
            builder.Services.AddScoped<ILoggerProvider, DatabaseLoggerProvider>();

            return builder;
        }

        /// <summary>
        /// Thêm Database Logger với default options
        /// </summary>
        public static ILoggingBuilder AddDatabaseLogging(this ILoggingBuilder builder)
        {
            // Note: This requires ILogRepository and ILoggingService to be registered as Scoped
            // They should be registered in Startup.cs
            return builder.AddDatabaseLogging(options =>
            {
                options.MinimumLogLevel = LogLevel.Information;
                options.Enabled = true;
                options.MaxConcurrentLogs = 100;
                options.DatabaseTimeoutMs = 5000;
                options.IncludeExceptionDetails = true;
                options.IncludeStructuredData = true;
            });
        }
    }
}
