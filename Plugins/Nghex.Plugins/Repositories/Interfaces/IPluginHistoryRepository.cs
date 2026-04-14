using Nghex.Plugins.Persistence.Entities;

namespace Nghex.Plugins.Repositories.Interfaces
{
    /// <summary>
    /// Interface for Plugin History Repository (audit trail - never deleted)
    /// </summary>
    public interface IPluginHistoryRepository
    {
        Task<long> AddAsync(PluginHistoryEntity history);
        Task<IEnumerable<PluginHistoryEntity>> GetByPluginNameAsync(string pluginName);
        Task<IEnumerable<PluginHistoryEntity>> GetByActionAsync(string action);
        Task<IEnumerable<PluginHistoryEntity>> GetByUserAsync(string actionBy);
        Task<IEnumerable<PluginHistoryEntity>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<PluginHistoryEntity>> GetRecentAsync(int count = 50);
        Task<PluginHistoryEntity?> GetLastActionAsync(string pluginName);
        Task<IEnumerable<PluginHistoryEntity>> GetInstallHistoryAsync(string pluginName);
    }
}
