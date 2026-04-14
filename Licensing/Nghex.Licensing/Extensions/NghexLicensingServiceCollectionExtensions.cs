using Microsoft.Extensions.DependencyInjection;
using Nghex.Licensing.Interfaces;
using Nghex.Licensing.Services;

namespace Nghex.Licensing.Extensions
{
    public static class NghexLicensingServiceCollectionExtensions
    {
        /// <summary>
        /// Registers file-backed <see cref="IDeploymentIdService"/> (required for license binding).
        /// </summary>
        public static IServiceCollection AddNghexDeploymentId(this IServiceCollection services)
        {
            services.AddSingleton<IDeploymentIdService, DeploymentIdService>();
            return services;
        }
    }
}
