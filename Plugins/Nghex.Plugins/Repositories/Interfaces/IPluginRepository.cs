using Nghex.Plugins.Persistence.Entities;

namespace Nghex.Plugins.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Plugin Repository (uses hard delete)
    /// </summary>
    public interface IPluginRepository
    {
        Task<PluginEntity?> GetByIdAsync(long id);
        Task<IEnumerable<PluginEntity>> GetAllAsync(bool enabledOnly = false);
        Task<PluginEntity?> GetByNameAsync(string name);
        Task<long> AddAsync(PluginEntity plugin);
        Task<bool> UpdateAsync(PluginEntity plugin);
        Task<bool> DeleteAsync(long id);
        Task<bool> UpdateRuntimeStateAsync(string name, bool isLoaded, DateTime? lastLoadedAt = null, string? errorMessage = null);
        Task<bool> UpdateEnabledStateAsync(string name, bool isEnabled, string updatedBy);
        Task<bool> ExistsByNameAsync(string name);
        Task<IEnumerable<PluginEntity>> GetDependentPluginsAsync(string pluginName);
        Task ResetAllRuntimeStatesAsync();
    }
}
