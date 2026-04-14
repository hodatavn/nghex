using Nghex.Plugins.Persistence.Entities;
using Nghex.Plugins.Repositories.Interfaces;
using Nghex.Data;
using Nghex.Core.Logging;
using System.Data;
using Dapper;

namespace Nghex.Plugins.Repositories
{
    /// <summary>
    /// Plugin History Repository implementation (audit trail - never deleted)
    /// </summary>
    public class PluginHistoryRepository(IDatabaseExecutor databaseExecutor, ILogging loggingService) : IPluginHistoryRepository
    {
        private readonly IDatabaseExecutor _databaseExecutor = databaseExecutor;
        private readonly ILogging _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

        private readonly string[] _historyFields = [
            "ID", "PLUGIN_NAME", "PLUGIN_VERSION", "ACTION",
            "ASSEMBLY_PATH", "DESCRIPTION", "CONFIGURATION",
            "ACTION_BY", "ACTION_AT", "REASON", "DETAILS"
        ];

        public async Task<long> AddAsync(PluginHistoryEntity history)
        {
            try
            {
                const string query = @"
                    INSERT INTO SYS_PLUGIN_HISTORY (
                        PLUGIN_NAME, PLUGIN_VERSION, ACTION,
                        ASSEMBLY_PATH, DESCRIPTION, CONFIGURATION,
                        ACTION_BY, ACTION_AT, REASON, DETAILS
                    ) VALUES (
                        :PluginName, :PluginVersion, :Action,
                        :AssemblyPath, :Description, :Configuration,
                        :ActionBy, SYSDATE, :Reason, :Details
                    )
                    RETURNING ID INTO :Id";

                var parameters = new DynamicParameters();
                parameters.Add("PluginName", history.PluginName, DbType.String);
                parameters.Add("PluginVersion", history.PluginVersion, DbType.String);
                parameters.Add("Action", history.Action, DbType.String);
                parameters.Add("AssemblyPath", history.AssemblyPath, DbType.String);
                parameters.Add("Description", history.Description, DbType.String);
                parameters.Add("Configuration", history.Configuration, DbType.String);
                parameters.Add("ActionBy", history.ActionBy, DbType.String);
                parameters.Add("Reason", history.Reason, DbType.String);
                parameters.Add("Details", history.Details, DbType.String);

                return await _databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error adding plugin history: {history.PluginName} - {history.Action}",
                    ex,
                    source: "PluginHistoryRepository.AddAsync",
                    module: "Plugin",
                    action: "AddHistory",
                    details: new { PluginName = history.PluginName, Action = history.Action }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginHistoryEntity>> GetByPluginNameAsync(string pluginName)
        {
            try
            {
                var fields = string.Join(", ", _historyFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGIN_HISTORY
                    WHERE PLUGIN_NAME = :PluginName
                    ORDER BY ACTION_AT DESC";

                var parameters = new DynamicParameters();
                parameters.Add("PluginName", pluginName, DbType.String);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginHistoryEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting plugin history by name: {pluginName}",
                    ex,
                    source: "PluginHistoryRepository.GetByPluginNameAsync",
                    module: "Plugin",
                    action: "GetHistoryByPluginName",
                    details: new { PluginName = pluginName }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginHistoryEntity>> GetByActionAsync(string action)
        {
            try
            {
                var fields = string.Join(", ", _historyFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGIN_HISTORY
                    WHERE ACTION = :Action
                    ORDER BY ACTION_AT DESC";

                var parameters = new DynamicParameters();
                parameters.Add("Action", action, DbType.String);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginHistoryEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting plugin history by action: {action}",
                    ex,
                    source: "PluginHistoryRepository.GetByActionAsync",
                    module: "Plugin",
                    action: "GetHistoryByAction",
                    details: new { Action = action }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginHistoryEntity>> GetByUserAsync(string actionBy)
        {
            try
            {
                var fields = string.Join(", ", _historyFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGIN_HISTORY
                    WHERE ACTION_BY = :ActionBy
                    ORDER BY ACTION_AT DESC";

                var parameters = new DynamicParameters();
                parameters.Add("ActionBy", actionBy, DbType.String);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginHistoryEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting plugin history by user: {actionBy}",
                    ex,
                    source: "PluginHistoryRepository.GetByUserAsync",
                    module: "Plugin",
                    action: "GetHistoryByUser",
                    details: new { ActionBy = actionBy }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginHistoryEntity>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            try
            {
                var fields = string.Join(", ", _historyFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGIN_HISTORY
                    WHERE ACTION_AT BETWEEN :FromDate AND :ToDate
                    ORDER BY ACTION_AT DESC";

                var parameters = new DynamicParameters();
                parameters.Add("FromDate", from, DbType.DateTime);
                parameters.Add("ToDate", to, DbType.DateTime);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginHistoryEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error getting plugin history by date range",
                    ex,
                    source: "PluginHistoryRepository.GetByDateRangeAsync",
                    module: "Plugin",
                    action: "GetHistoryByDateRange",
                    details: new { From = from, To = to }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginHistoryEntity>> GetRecentAsync(int count = 50)
        {
            try
            {
                var fields = string.Join(", ", _historyFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGIN_HISTORY
                    ORDER BY ACTION_AT DESC
                    FETCH FIRST :Count ROWS ONLY";

                var parameters = new DynamicParameters();
                parameters.Add("Count", count, DbType.Int32);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginHistoryEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error getting recent plugin history",
                    ex,
                    source: "PluginHistoryRepository.GetRecentAsync",
                    module: "Plugin",
                    action: "GetRecentHistory",
                    details: new { Count = count }
                );
                throw;
            }
        }

        public async Task<PluginHistoryEntity?> GetLastActionAsync(string pluginName)
        {
            try
            {
                var fields = string.Join(", ", _historyFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGIN_HISTORY
                    WHERE PLUGIN_NAME = :PluginName
                    ORDER BY ACTION_AT DESC
                    FETCH FIRST 1 ROW ONLY";

                var parameters = new DynamicParameters();
                parameters.Add("PluginName", pluginName, DbType.String);

                return await _databaseExecutor.ExecuteQuerySingleAsync<PluginHistoryEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting last action for plugin: {pluginName}",
                    ex,
                    source: "PluginHistoryRepository.GetLastActionAsync",
                    module: "Plugin",
                    action: "GetLastAction",
                    details: new { PluginName = pluginName }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginHistoryEntity>> GetInstallHistoryAsync(string pluginName)
        {
            try
            {
                var fields = string.Join(", ", _historyFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGIN_HISTORY
                    WHERE PLUGIN_NAME = :PluginName
                    AND ACTION IN ('INSTALL', 'UNINSTALL', 'UPDATE')
                    ORDER BY ACTION_AT DESC";

                var parameters = new DynamicParameters();
                parameters.Add("PluginName", pluginName, DbType.String);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginHistoryEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting install history for plugin: {pluginName}",
                    ex,
                    source: "PluginHistoryRepository.GetInstallHistoryAsync",
                    module: "Plugin",
                    action: "GetInstallHistory",
                    details: new { PluginName = pluginName }
                );
                throw;
            }
        }
    }
}
