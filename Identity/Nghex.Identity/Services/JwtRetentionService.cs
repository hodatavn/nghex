using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nghex.Identity.Services.Interfaces;
using Nghex.Core.Setting;

namespace Nghex.Identity.Services
{
    /// <summary>
    /// Background service to cleanup expired tokens
    /// </summary>
    public class JwtRetentionService(IServiceProvider serviceProvider) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        private static readonly TimeSpan TokenCleanupInterval = TimeSpan.FromMinutes(AppSettings.TokenRetentionDuration);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Initial delay for application startup
            await Task.Delay(AppSettings.InitialDelay, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();

                    // Rotate secret key if needed (based on SecretKeyRotationDays)
                    await jwtService.RotateSecretKeyAsync();
                    
                    // Cleanup expired tokens
                    await jwtService.CleanupExpiredTokensAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while processing JWT retention: {ex.Message}");
                }
                await Task.Delay(TokenCleanupInterval, stoppingToken);
            }
        }
    }
}
