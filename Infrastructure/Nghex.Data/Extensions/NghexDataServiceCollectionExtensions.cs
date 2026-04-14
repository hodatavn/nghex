using Microsoft.Extensions.DependencyInjection;
using Nghex.Data;
using Nghex.Data.Factory;
using Nghex.Data.Factory.Interfaces;
using Nghex.Data.Setup;

namespace Nghex.Data.Extensions
{
    public static class NghexDataServiceCollectionExtensions
    {
        /// <summary>
        /// Registers core Nghex.Data services (connection, executor, factories) and database bootstrap (<see cref="IDatabaseSetupService"/>).
        /// </summary>
        public static IServiceCollection AddNghexDataInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseConnection, DatabaseConnection>();
            services.AddSingleton<IDatabaseProviderFactory, DatabaseProviderFactory>();
            services.AddSingleton<IDatabaseExecutorHelperFactory, DatabaseExecutorHelperFactory>();
            services.AddScoped<IDatabaseExecutor, DatabaseExecutor>();
            services.AddScoped<IDatabaseSetupService, DatabaseSetupService>();
            services.AddSingleton<ISystemInitializationState, SystemInitializationState>();

            return services;
        }
    }
}
