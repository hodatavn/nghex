using Microsoft.Extensions.DependencyInjection;
using Nghex.Core.Logging;
using Nghex.Logging.Interfaces;
using Nghex.Logging.Repositories;
using Nghex.Logging.Services;

namespace Nghex.Logging.Extensions
{
    public static class NghexLoggingExtensions
    {
        /// <summary>
        /// Đăng ký DB logging (LogRepository + LoggingService + LogRetentionService).
        /// LoggingService satisfies cả ILogging (Core) và ILoggingService (Nghex.Logging).
        /// LogRetentionService tự động chạy background để xóa log cũ.
        /// </summary>
        public static IServiceCollection AddNghexDatabaseLogging(this IServiceCollection services)
        {
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<LoggingService>();
            services.AddScoped<ILogging>(sp => sp.GetRequiredService<LoggingService>());
            services.AddScoped<ILoggingService>(sp => sp.GetRequiredService<LoggingService>());
            services.AddScoped<ILogQueryService>(sp => sp.GetRequiredService<LoggingService>());
            services.AddHostedService<LogRetentionService>();
            return services;
        }
    }
}
