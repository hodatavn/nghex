using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nghex.Core.Setting;
using Nghex.Logging.Interfaces;

namespace Nghex.Logging.Services
{
    public class LogRetentionService(IServiceProvider serviceProvider) : BackgroundService
    {
        private static readonly TimeSpan LogCleanupInterval = TimeSpan.FromDays(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(AppSettings.InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var logQueryService = scope.ServiceProvider.GetRequiredService<ILogQueryService>();
                    await logQueryService.CleanupOldLogsAsync(AppSettings.LogRetentionDays);
                    await Task.Delay(LogCleanupInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while processing log retention: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
