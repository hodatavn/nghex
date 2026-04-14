
using Microsoft.AspNetCore.Builder;

namespace Nghex.Plugins
{
    /// <summary>
    /// Interface for plugin that can provide API endpoints
    /// </summary>
    public interface IApiPlugin : IPlugin
    {
        /// <summary>
        /// Register API endpoints using minimal API routing
        /// </summary>
        /// <param name="app">WebApplication instance for registering endpoints</param>
        Task RegisterEndpointsAsync(WebApplication app);

        /// <summary>
        /// Unregister API endpoints
        /// </summary>
        Task UnregisterEndpointsAsync();

        /// <summary>
        /// List of API endpoints provided by the plugin
        /// </summary>
        IEnumerable<string> GetEndpoints();
    }
}
