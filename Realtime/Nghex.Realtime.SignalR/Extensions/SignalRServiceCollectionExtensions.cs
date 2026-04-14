using Nghex.Data;
using Nghex.Data.Interfaces;

namespace Nghex.Realtime.SignalR.Extensions
{
    public static class SignalRServiceCollectionExtensions
    {
        /// <summary>
        /// Registers SignalR-related services required for database change notifications.
        /// </summary>
        public static IServiceCollection AddSignalRChangeNotification(this IServiceCollection services)
        {
            services.AddSingleton<DataChangeNotification>();
            services.AddSingleton<IDatabaseChangeNotification>(sp => sp.GetRequiredService<DataChangeNotification>());

            services.AddHostedService(sp => sp.GetRequiredService<DataChangeNotification>());
            services.AddHostedService<DbChangeNotificationHandler>();

            return services;
        }
    }
}
