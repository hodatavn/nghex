using Nghex.Configuration.Persistence.Entities;
using Nghex.Base.Repositories;

namespace Nghex.Configuration.Repositories.Interfaces
{
    /// <summary>
    /// Interface cho Configuration Repository
    /// </summary>
    public interface IConfigurationRepository : IRepository<ConfigurationEntity>
    {
        /// <summary>
        /// Get all configurations
        /// </summary>
        /// <param name="isActive">Whether to include active configurations</param>
        /// <returns>The configurations</returns>
        Task<IEnumerable<ConfigurationEntity>> GetAllAsync(bool isActive);

        /// <summary>
        /// Get configuration by key
        /// </summary>
        /// <param name="key">The key of the configuration</param>
        /// <returns>The configuration</returns>
        Task<ConfigurationEntity?> GetByKeyAsync(string key);

        /// <summary>
        /// Get configurations by module
        /// </summary>
        /// <param name="module">The module of the configurations</param>
        /// <returns>The configurations</returns>
        Task<IEnumerable<ConfigurationEntity>> GetByModuleAsync(string module);

        
        /// <summary>
        /// Check if the configuration is a system configuration
        /// </summary>
        /// <param name="id">The ID of the configuration</param>
        /// <returns>True if the configuration is a system configuration, false otherwise</returns>
        Task<bool> IsSystemConfigAsync(long id);
    }
}
