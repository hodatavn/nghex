
namespace Nghex.Plugins
{
    /// <summary>
    /// Interface for plugin that can provide database models
    /// </summary>
    public interface IModelPlugin : IPlugin
    {
        /// <summary>
        /// Register models with the system
        /// </summary>
        Task RegisterModelsAsync();

        /// <summary>
        /// Unregister models
        /// </summary>
        Task UnregisterModelsAsync();

        /// <summary>
        /// List of model types provided by the plugin
        /// </summary>
        IEnumerable<Type> GetModelTypes();
    }
}
