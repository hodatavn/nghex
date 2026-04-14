using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nghex.Core.Configuration;
using Nghex.Core.Logging;

namespace Nghex.Core.Extension
{
    public static class NghexCoreServiceExtensions
    {
        /// <summary>
        /// Registers <see cref="IAppConfigurationReader"/> backed only by <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>
        /// (appsettings.json, environment variables, etc.). No database — safe without referencing Nghex.Configuration.
        /// If you later call <c>AddNghexConfiguration()</c>, it replaces this registration with DB + appsettings fallback.
        /// </summary>
        public static IServiceCollection AddNghexAppConfiguration(this IServiceCollection services)
        {
            services.TryAddSingleton<IAppConfigurationReader, AppSettingsConfigurationReader>();
            return services;
        }

        /// <summary>
        /// Default logging mode — writes audit logs to text file. No DB required.
        /// Call this when you don't need DB-queryable logs.
        /// For DB logging, use AddNghexDatabaseLogging() from Nghex.Logging instead.
        /// </summary>
        public static IServiceCollection AddNghexLogging(this IServiceCollection services,
            string directory = "logs", string fileName = "nghex.log",
            int maxFileSizeMb = 10, int retentionDays = 30)
        {
            services.AddScoped<ILogging>(_ => new FileLoggingService(directory, fileName, maxFileSizeMb, retentionDays));
            return services;
        }
    }
}
