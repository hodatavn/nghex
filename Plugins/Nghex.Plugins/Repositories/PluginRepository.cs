using System.Data;
using Dapper;
using Nghex.Plugins.Persistence.Entities;
using Nghex.Plugins.Repositories.Interfaces;
using Nghex.Data;
using Nghex.Core.Logging;

namespace Nghex.Plugins.Repositories
{
    /// <summary>
    /// Plugin Repository implementation (uses hard delete)
    /// </summary>
    public class PluginRepository(IDatabaseExecutor databaseExecutor, ILogging loggingService) : IPluginRepository
    {
        private readonly IDatabaseExecutor _databaseExecutor = databaseExecutor;
        private readonly ILogging _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

        private readonly string[] _pluginFields = ["ID", "NAME", "VERSION", "DESCRIPTION", "ASSEMBLY_PATH", "IS_LOADED", "IS_ENABLED",
            "LAST_LOADED_AT", "ERROR_MESSAGE", "DEPENDENCIES", "CONFIGURATION", "PRIORITY"
        ];

        public async Task<PluginEntity?> GetByIdAsync(long id)
        {
            try
            {
                var fields = string.Join(", ", _pluginFields);
                string query = $@"SELECT {fields} FROM SYS_PLUGINS WHERE ID = :Id";

                var parameters = new DynamicParameters();
                parameters.Add("Id", id, DbType.Int64);

                return await _databaseExecutor.ExecuteQuerySingleAsync<PluginEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting plugin by ID: {id}",
                    ex,
                    source: "PluginRepository.GetByIdAsync",
                    module: "Plugin",
                    action: "GetPluginById",
                    details: new { Id = id }
                );
                throw;
            }
        }

        public async Task<PluginEntity?> GetByNameAsync(string name)
        {
            try
            {
                var fields = string.Join(", ", _pluginFields);
                string query = $@"SELECT {fields} FROM SYS_PLUGINS WHERE NAME = :Name";

                var parameters = new DynamicParameters();
                parameters.Add("Name", name, DbType.String);

                return await _databaseExecutor.ExecuteQuerySingleAsync<PluginEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting plugin by Name: {name}",
                    ex,
                    source: "PluginRepository.GetByNameAsync",
                    module: "Plugin",
                    action: "GetPluginByName",
                    details: new { Name = name }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginEntity>> GetAllAsync(bool enabledOnly = false)
        {
            try
            {
                var fields = string.Join(", ", _pluginFields);
                string query = $@"
                SELECT {fields} FROM SYS_PLUGINS
                {(enabledOnly ? "WHERE IS_ENABLED = 1" : string.Empty)}
                ORDER BY PRIORITY, NAME";

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginEntity>(query);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error getting all plugins",
                    ex,
                    source: "PluginRepository.GetAllAsync",
                    module: "Plugin",
                    action: "GetAllPlugins"
                );
                throw;
            }
        }

        public async Task<long> AddAsync(PluginEntity plugin)
        {
            try
            {
                const string query = @"
                    INSERT INTO SYS_PLUGINS (
                        NAME, VERSION, DESCRIPTION, ASSEMBLY_PATH,
                        IS_LOADED, IS_ENABLED, LAST_LOADED_AT, ERROR_MESSAGE,
                        DEPENDENCIES, CONFIGURATION, PRIORITY,
                        CREATED_BY, CREATED_AT
                    ) VALUES (
                        :Name, :Version, :Description, :AssemblyPath,
                        :IsLoaded, :IsEnabled, :LastLoadedAt, :ErrorMessage,
                        :Dependencies, :Configuration, :Priority,
                        :CreatedBy, SYSDATE
                    )
                    RETURNING ID INTO :Id";

                var parameters = new DynamicParameters();
                parameters.Add("Name", plugin.Name, DbType.String);
                parameters.Add("Version", plugin.Version, DbType.String);
                parameters.Add("Description", plugin.Description, DbType.String);
                parameters.Add("AssemblyPath", plugin.AssemblyPath, DbType.String);
                parameters.Add("IsLoaded", plugin.IsLoaded ? 1 : 0, DbType.Int32);
                parameters.Add("IsEnabled", plugin.IsEnabled ? 1 : 0, DbType.Int32);
                parameters.Add("LastLoadedAt", plugin.LastLoadedAt, DbType.DateTime);
                parameters.Add("ErrorMessage", plugin.ErrorMessage, DbType.String);
                parameters.Add("Dependencies", plugin.Dependencies, DbType.String);
                parameters.Add("Configuration", plugin.Configuration, DbType.String);
                parameters.Add("Priority", plugin.Priority, DbType.Int32);
                parameters.Add("CreatedBy", plugin.CreatedBy, DbType.String);

                return await _databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error adding plugin: {plugin.Name}",
                    ex,
                    source: "PluginRepository.AddAsync",
                    module: "Plugin",
                    action: "AddPlugin",
                    details: new { Name = plugin.Name }
                );
                throw;
            }
        }

        public async Task<bool> UpdateAsync(PluginEntity plugin)
        {
            try
            {
                const string query = @"
                    UPDATE SYS_PLUGINS SET
                        VERSION = :Version,
                        DESCRIPTION = :Description,
                        ASSEMBLY_PATH = :AssemblyPath,
                        IS_ENABLED = :IsEnabled,
                        DEPENDENCIES = :Dependencies,
                        CONFIGURATION = :Configuration,
                        PRIORITY = :Priority,
                        UPDATED_BY = :UpdatedBy,
                        UPDATED_AT = SYSDATE
                    WHERE ID = :Id";

                var parameters = new DynamicParameters();
                parameters.Add("Id", plugin.Id, DbType.Int64);
                parameters.Add("Version", plugin.Version, DbType.String);
                parameters.Add("Description", plugin.Description, DbType.String);
                parameters.Add("AssemblyPath", plugin.AssemblyPath, DbType.String);
                parameters.Add("IsEnabled", plugin.IsEnabled ? 1 : 0, DbType.Int32);
                parameters.Add("Dependencies", plugin.Dependencies, DbType.String);
                parameters.Add("Configuration", plugin.Configuration, DbType.String);
                parameters.Add("Priority", plugin.Priority, DbType.Int32);
                parameters.Add("UpdatedBy", plugin.UpdatedBy, DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error updating plugin: {plugin.Id}",
                    ex,
                    source: "PluginRepository.UpdateAsync",
                    module: "Plugin",
                    action: "UpdatePlugin",
                    details: new { Id = plugin.Id }
                );
                throw;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                const string query = "DELETE FROM SYS_PLUGINS WHERE ID = :Id";

                var parameters = new DynamicParameters();
                parameters.Add("Id", id, DbType.Int64);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error deleting plugin: {id}",
                    ex,
                    source: "PluginRepository.DeleteAsync",
                    module: "Plugin",
                    action: "DeletePlugin",
                    details: new { Id = id }
                );
                throw;
            }
        }

        public async Task<bool> UpdateRuntimeStateAsync(string name, bool isLoaded, DateTime? lastLoadedAt = null, string? errorMessage = null)
        {
            try
            {
                string query = $@"
                    UPDATE SYS_PLUGINS SET
                        IS_LOADED = :IsLoaded,
                        LAST_LOADED_AT = :LastLoadedAt,
                        ERROR_MESSAGE = :ErrorMessage,
                        UPDATED_AT = SYSDATE
                    WHERE NAME = :Name";

                var parameters = new DynamicParameters();
                parameters.Add("Name", name, DbType.String);
                parameters.Add("IsLoaded", isLoaded ? 1 : 0, DbType.Int32);
                parameters.Add("LastLoadedAt", lastLoadedAt, DbType.DateTime);
                parameters.Add("ErrorMessage", errorMessage, DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error updating runtime state for plugin: {name}",
                    ex,
                    source: "PluginRepository.UpdateRuntimeStateAsync",
                    module: "Plugin",
                    action: "UpdateRuntimeState",
                    details: new { Name = name, IsLoaded = isLoaded }
                );
                throw;
            }
        }

        public async Task<bool> UpdateEnabledStateAsync(string name, bool isEnabled, string updatedBy)
        {
            try
            {
                const string query = @"
                    UPDATE SYS_PLUGINS SET
                        IS_ENABLED = :IsEnabled,
                        UPDATED_BY = :UpdatedBy,
                        UPDATED_AT = SYSDATE
                    WHERE NAME = :Name";

                var parameters = new DynamicParameters();
                parameters.Add("Name", name, DbType.String);
                parameters.Add("IsEnabled", isEnabled ? 1 : 0, DbType.Int32);
                parameters.Add("UpdatedBy", updatedBy, DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error updating enabled state for plugin: {name}",
                    ex,
                    source: "PluginRepository.UpdateEnabledStateAsync",
                    module: "Plugin",
                    action: "UpdateEnabledState",
                    details: new { Name = name, IsEnabled = isEnabled }
                );
                throw;
            }
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            try
            {
                const string query = "SELECT COUNT(1) FROM SYS_PLUGINS WHERE NAME = :Name";

                var parameters = new DynamicParameters();
                parameters.Add("Name", name, DbType.String);

                var count = await _databaseExecutor.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error checking plugin exists: {name}",
                    ex,
                    source: "PluginRepository.ExistsByNameAsync",
                    module: "Plugin",
                    action: "CheckPluginExists",
                    details: new { Name = name }
                );
                throw;
            }
        }

        public async Task<IEnumerable<PluginEntity>> GetDependentPluginsAsync(string pluginName)
        {
            try
            {
                var fields = string.Join(", ", _pluginFields);
                string query = $@"
                    SELECT {fields}
                    FROM SYS_PLUGINS
                    WHERE DEPENDENCIES LIKE :DependencyPattern
                    ORDER BY PRIORITY, NAME";

                var parameters = new DynamicParameters();
                parameters.Add("DependencyPattern", $"%\"{pluginName}\"%", DbType.String);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<PluginEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting dependent plugins for: {pluginName}",
                    ex,
                    source: "PluginRepository.GetDependentPluginsAsync",
                    module: "Plugin",
                    action: "GetDependentPlugins",
                    details: new { PluginName = pluginName }
                );
                throw;
            }
        }

        public async Task ResetAllRuntimeStatesAsync()
        {
            try
            {
                const string query = @"
                    UPDATE SYS_PLUGINS SET
                        IS_LOADED = 0,
                        ERROR_MESSAGE = NULL,
                        UPDATED_AT = SYSDATE";

                await _databaseExecutor.ExecuteNonQueryAsync(query);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error resetting all plugin runtime states",
                    ex,
                    source: "PluginRepository.ResetAllRuntimeStatesAsync",
                    module: "Plugin",
                    action: "ResetAllRuntimeStates"
                );
                throw;
            }
        }
    }
}
