using Microsoft.Extensions.DependencyInjection;

namespace Nghex.Plugins
{
    /// <summary>
    /// Interface for plugin that can register its own services and repositories
    /// </summary>
    public interface IServicePlugin : IPlugin
    {
        /// <summary>
        /// Register plugin services and repositories to the dependency injection container
        /// This method is called before the application is built, allowing plugins to register their dependencies
        /// </summary>
        /// <param name="services">Service collection to register services to</param>
        Task RegisterServicesAsync(IServiceCollection services);
    }
}


