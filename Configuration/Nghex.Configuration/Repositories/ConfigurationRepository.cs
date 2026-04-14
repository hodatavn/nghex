using System.Data;
using Dapper;
using Nghex.Configuration.Persistence.Entities;
using Nghex.Data;
using Nghex.Configuration.Repositories.Interfaces;
using Nghex.Core.Logging;

namespace Nghex.Configuration.Repositories
{
    /// <summary>
    /// Configuration Repository implementation
    /// </summary>
    public class ConfigurationRepository(IDatabaseExecutor databaseExecutor, ILogging loggingService) : IConfigurationRepository
    {
        private readonly IDatabaseExecutor _databaseExecutor = databaseExecutor;
        private readonly ILogging _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        private readonly string[] _configurationFields = [
            "ID", "KEY", "VALUE", "DESCRIPTION", "DATA_TYPE", "MODULE", "DEFAULT_VALUE",  "IS_SYSTEM_CONFIG", "IS_EDITABLE", "IS_ACTIVE"
        ];

        // private readonly string[] _auditFields = ["CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT" ];

        public async Task<ConfigurationEntity?> GetByIdAsync(long id)
        {
            try
            {
                var fields = string.Join(", ", _configurationFields);
                string query = $@"
                SELECT {fields} FROM sys_configurations 
                WHERE ID = :Id";
                var parameters = new DynamicParameters();
                parameters.Add("Id", id, DbType.Int64);
                
                return await _databaseExecutor.ExecuteQuerySingleAsync<ConfigurationEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting configuration by ID: {id}",
                    ex,
                    source: "ConfigurationRepository.GetByIdAsync",
                    module: "Repository",
                    action: "GetConfigurationById",
                    details: new { Id = id }
                );
                throw;
            }
        }

        public async Task<IEnumerable<ConfigurationEntity>> GetAllAsync()
        {
            return await GetAllAsync(true);
        }

        public async Task<IEnumerable<ConfigurationEntity>> GetAllAsync(bool isActive)
        {
            try
            {
                var fields = string.Join(", ", _configurationFields);
                string query = $@"
                SELECT {fields} 
                FROM sys_configurations 
                WHERE IS_ACTIVE = :IsActive
                ORDER BY ID
                FETCH FIRST 1000 ROWS ONLY";

                var parameters = new DynamicParameters();
                parameters.Add("IsActive", isActive ? 1 : 0, DbType.Int32);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<ConfigurationEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error getting all configurations",
                    ex,
                    source: "ConfigurationRepository.GetAllAsync",
                    module: "ConfigurationRepository",
                    action: "GetAllConfigurations"
                );
                throw;
            }
        }


        public async Task<long> AddAsync(ConfigurationEntity entity)
        {
            try
            {
                const string query = @"
                    INSERT INTO sys_configurations (
                        KEY, VALUE, DESCRIPTION, DATA_TYPE, MODULE, IS_SYSTEM_CONFIG, IS_EDITABLE, DEFAULT_VALUE, CREATED_BY, CREATED_AT
                    ) 
                    VALUES (:Key, :Value, :Description, :DataType, :Module, :IsSystemConfig, :IsEditable, :DefaultValue, :CreatedBy, SYSDATE)
                    RETURNING Id INTO :Id";

                var parameters = new DynamicParameters();
                parameters.Add("Key", entity.Key, DbType.String);
                parameters.Add("Value", entity.Value, DbType.String);
                parameters.Add("Description", entity.Description);
                parameters.Add("Module", entity.Module, DbType.String);
                parameters.Add("DataType", entity.DataType, DbType.String);
                parameters.Add("IsSystemConfig", entity.IsSystemConfig ? 1 : 0, DbType.Int32);
                parameters.Add("IsEditable", entity.IsEditable ? 1 : 0, DbType.Int32);
                parameters.Add("DefaultValue", entity.DefaultValue, DbType.String);
                parameters.Add("CreatedBy", entity.CreatedBy, DbType.String);

                return await _databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error adding configuration: {entity.Key}",
                    ex,
                    source: "ConfigurationRepository.AddAsync",
                    module: "ConfigurationRepository",
                    action: "AddConfiguration",
                    details: new { Key = entity.Key }
                );
                throw;
            }
        }

        /// <summary>
        /// Update editable configuration
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(ConfigurationEntity entity)
        {
            try
            {
                const string query = @"
                    UPDATE sys_configurations SET 
                        VALUE = :Value,
                        DESCRIPTION = :Description,
                        MODULE = :Module,
                        DATA_TYPE = :DataType,
                        DEFAULT_VALUE = :DefaultValue,
                        IS_ACTIVE = :IsActive,
                        UPDATED_AT = SYSDATE,
                        UPDATED_BY = :UpdatedBy
                    WHERE ID = :Id AND IS_EDITABLE = 1";
                
                var parameters = new DynamicParameters();
                parameters.Add("Id", entity.Id, DbType.Int64);
                parameters.Add("Value", entity.Value, DbType.String);
                parameters.Add("Description", entity.Description, DbType.String);
                parameters.Add("Module", entity.Module, DbType.String);
                parameters.Add("DataType", entity.DataType, DbType.String);
                parameters.Add("DefaultValue", entity.DefaultValue, DbType.String);
                parameters.Add("IsActive", entity.IsActive ? 1 : 0, DbType.Int32);
                parameters.Add("UpdatedBy", entity.UpdatedBy, DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error updating configuration: {entity.Id}",
                    ex,
                    source: "ConfigurationRepository.UpdateAsync",
                    module: "ConfigurationRepository",
                    action: "UpdateConfiguration",
                    details: new { Id = entity.Id }
                );
                throw;
            }
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            ///TODO: Configuration CANNOT BE DELETED
            return await Task.FromResult(false);
        }

        public async Task<ConfigurationEntity?> GetByKeyAsync(string key)
        {
            try
            {
                var fields = string.Join(", ", _configurationFields);
                string query = $@"SELECT {fields} FROM sys_configurations WHERE KEY = :Key";
                var parameters = new DynamicParameters();
                parameters.Add("Key", key, DbType.String);

                return await _databaseExecutor.ExecuteQuerySingleAsync<ConfigurationEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting configuration by key: {key}",
                    ex,
                    source: "ConfigurationRepository.GetByKeyAsync",
                    module: "Repository",
                    action: "GetConfigurationByKey",
                    details: new { Key = key }
                );
                throw;
            }
        }

        public async Task<IEnumerable<ConfigurationEntity>> GetByModuleAsync(string module)
        {
            try
            {
                var fields = string.Join(", ", _configurationFields);
                string query = $@"SELECT {fields} 
                FROM sys_configurations 
                WHERE MODULE = :Module 
                    AND IS_ACTIVE = 1
                ORDER BY ID
                FETCH FIRST 1000 ROWS ONLY";
                var parameters = new DynamicParameters();
                parameters.Add("Module", module, DbType.String);

                return await _databaseExecutor.ExecuteQueryMultipleAsync<ConfigurationEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting configurations by module: {module}",
                    ex,
                    source: "ConfigurationRepository.GetByModuleAsync",
                    module: "Repository",
                    action: "GetConfigurationsByModule",
                    details: new { Module = module }
                );
                throw;
            }
        }

        public async Task<bool> IsSystemConfigAsync(long id)
        {
            var configuration = await GetByIdAsync(id);
            return configuration != null && configuration.IsSystemConfig;
        }
    }
}
