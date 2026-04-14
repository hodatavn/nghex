using Microsoft.Extensions.DependencyInjection;
using Nghex.Core.Configuration;
using Nghex.Configuration.Repositories;
using Nghex.Configuration.Repositories.Interfaces;
using Nghex.Configuration.Services;
using Nghex.Configuration.Services.Interfaces;

namespace Nghex.Configuration.Extension
{
    public static class NghexConfigurationExtensions
    {
        /// <summary>
        /// Registers DB-backed configuration (repository + <see cref="IConfigurationService"/>).
        /// Runtime changes in the database are visible without restart; values not in DB fall back to <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>.
        /// Replaces any prior <c>IAppConfigurationReader</c> registration (e.g. from <c>AddNghexAppConfiguration()</c>).
        /// </summary>
        public static IServiceCollection AddNghexConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IAppConfigurationReader>(sp => sp.GetRequiredService<IConfigurationService>());
            return services;
        }
    }
}
