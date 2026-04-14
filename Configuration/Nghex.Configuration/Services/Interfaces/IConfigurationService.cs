using Nghex.Configuration.DTOs;
using Nghex.Core.Configuration;

namespace Nghex.Configuration.Services.Interfaces
{
    /// <summary>
    /// Interface cho Configuration Service
    /// </summary>
    public interface IConfigurationService : IAppConfigurationReader
    {
        /// <summary>
        /// Get all available data types for configuration as list of string
        /// </summary>
        /// <returns>The available data types as list of string</returns>
        IEnumerable<string> GetDataTypes();    

        /// <summary>
        /// Get configuration by ID
        /// </summary>
        /// <param name="id">The ID of the configuration to get</param>
        /// <returns>The configuration DTO</returns>
        Task<ConfigurationDto?> GetByIdAsync(long id);

        /// <summary>
        /// Get configuration by key
        /// </summary>
        /// <param name="key">The key of the configuration to get</param>
        /// <returns>The configuration DTO</returns>
        Task<ConfigurationDto?> GetByKeyAsync(string key);

        /// <summary>
        /// Check if configuration exists by key
        /// </summary>
        /// <param name="key">The key of the configuration to check</param>
        /// <returns>True if the configuration exists, false otherwise</returns>
        Task<bool> KeyExistsAsync(string key);

        /// <summary>
        /// Get all configurations
        /// </summary>
        /// <param name="isActive">Whether to include active configurations</param>
        /// <returns>The configuration DTOs</returns>
        Task<IEnumerable<ConfigurationDto>> GetAllAsync(bool isActive);


        /// <summary>
        /// Create new configuration
        /// </summary>
        /// <param name="createDto">The configuration data to create</param>
        /// <returns>The created configuration DTO with ID</returns>
        Task<ConfigurationDto> CreateAsync(CreateConfigurationDto createDto);

        /// <summary>
        /// Update configuration
        /// </summary>
        /// <param name="updateDto">The configuration data to update</param>
        /// <returns>True if the configuration was updated, false otherwise</returns>
        Task<bool> UpdateAsync(UpdateConfigurationDto updateDto);

        /// <summary>
        /// Get all active configurations by module
        /// </summary>
        /// <param name="module">The module of the configurations to get</param>
        /// <returns>The configuration DTOs</returns>
        Task<IEnumerable<ConfigurationDto>> GetByModuleAsync(string module);

        /// <summary>
        /// Reset configuration to default value
        /// </summary>
        /// <param name="id">The ID of the configuration to reset to default value</param>
        /// <param name="updatedBy">The user who updated the configuration</param>
        /// <returns>True if the configuration was reset to default value, false otherwise</returns>
        Task<bool> ResetToDefaultAsync(long id, string updatedBy);

        /// <summary>
        /// Import configurations from JSON
        /// </summary>
        /// <param name="jsonData">The JSON data to import configurations from</param>
        /// <param name="createdBy">The user who created the configurations</param>
        /// <param name="module">The module of the configurations to import, default is "Core"</param>
        /// <returns>The number of configurations imported</returns>
        Task<int> ImportFromJsonAsync(string jsonData, string createdBy, string module = "Core");

        /// <summary>
        /// Export configurations to JSON
        /// </summary>
        /// <param name="module">The module of the configurations to export</param>
        /// <param name="includeSystemConfigs">Whether to include system configurations</param>
        /// <returns>The JSON data of the configurations</returns>
        Task<string> ExportToJsonAsync(string? module = null, bool includeSystemConfigs = false);

    }
}
