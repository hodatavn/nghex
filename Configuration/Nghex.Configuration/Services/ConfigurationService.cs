using Mapster;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Nghex.Configuration.Api.Models;
using Nghex.Core.Configuration;
using Nghex.Core.Helper;
using Nghex.Core.Logging;
using Nghex.Configuration.Persistence.Entities;
using Nghex.Configuration.Repositories.Interfaces;
using Nghex.Configuration.Services.Interfaces;

namespace Nghex.Configuration.Services
{
    public class ConfigurationService(
        IConfigurationRepository configurationRepository,
        IConfiguration configuration,
        ILogging loggingService
        ) : IConfigurationService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IConfigurationRepository _configurationRepository = configurationRepository;
        private readonly ILogging _loggingService = loggingService;
        private static readonly string[] ValidDataTypes = ["string", "int", "bool", "double", "decimal", "datetime"];

        public IEnumerable<string> GetDataTypes() => ValidDataTypes.AsEnumerable();

        #region Read Operations

        public async Task<ConfigurationResponseModel?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;
            var entity = await _configurationRepository.GetByIdAsync(id);
            return entity?.Adapt<ConfigurationResponseModel>();
        }

        public async Task<ConfigurationResponseModel?> GetByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            var entity = await _configurationRepository.GetByKeyAsync(key);
            return entity?.Adapt<ConfigurationResponseModel>();
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            return configuration != null;
        }

        public async Task<IEnumerable<ConfigurationResponseModel>> GetAllAsync(bool isActive)
        {
            var entities = await _configurationRepository.GetAllAsync(isActive);
            return entities.Select(e => e.Adapt<ConfigurationResponseModel>());
        }

        public async Task<IEnumerable<ConfigurationResponseModel>> GetByModuleAsync(string module)
        {
            if (string.IsNullOrWhiteSpace(module)) return [];
            var entities = await _configurationRepository.GetByModuleAsync(module);
            return entities.Select(e => e.Adapt<ConfigurationResponseModel>());
        }

        #endregion

        #region Value Getters

        public async Task<string?> GetValueAsync(string key)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null) return configuration.Value;
            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            return string.IsNullOrEmpty(fromApp) ? null : fromApp;
        }

        public async Task<string> GetValueAsync(string key, string defaultValue)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null) return configuration.Value ?? defaultValue;
            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            return string.IsNullOrEmpty(fromApp) ? defaultValue : fromApp;
        }

        public async Task<string> GetValueAsync(string key, bool useDefaultValue = false)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null)
                return (useDefaultValue ? configuration.DefaultValue : configuration.Value) ?? string.Empty;
            return AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
        }

        public async Task<int> GetIntValueAsync(string key)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null && int.TryParse(configuration.Value, out var result)) return result;
            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            return int.TryParse(fromApp, out var r) ? r : 0;
        }

        public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null && int.TryParse(configuration.Value, out var result)) return result;
            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            return int.TryParse(fromApp, out var r) ? r : defaultValue;
        }

        public async Task<bool> GetBoolValueAsync(string key)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null)
                return bool.TryParse(configuration.Value, out var result) && result;
            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            return bool.TryParse(fromApp, out var r) && r;
        }

        public async Task<double> GetDoubleValueAsync(string key)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null && double.TryParse(configuration.Value, out var result)) return result;
            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            return double.TryParse(fromApp, out var r) ? r : 0;
        }

        public async Task<DateTime?> GetDateTimeValueAsync(string key)
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration != null && DateTime.TryParse(configuration.Value, out var result)) return result;
            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            return DateTime.TryParse(fromApp, out var r) ? r : null;
        }

        public async Task<T?> GetJsonValueAsync<T>(string key) where T : class
        {
            var configuration = await _configurationRepository.GetByKeyAsync(key);
            if (configuration?.Value != null)
            {
                return JsonSerializer.Deserialize<T>(configuration.Value, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            var fromApp = AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);
            if (string.IsNullOrWhiteSpace(fromApp)) return null;

            return JsonSerializer.Deserialize<T>(fromApp, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        public string GetAppSettingValueByKey(string key) =>
            AppSettingsConfigurationReader.GetAppSettingValueFromConfiguration(_configuration, key);

        #endregion

        #region Write Operations

        public async Task<ConfigurationResponseModel> CreateAsync(CreateConfigurationRequest input)
        {
            ArgumentNullException.ThrowIfNull(input);

            await ValidateNewConfigurationAsync(input);

            var entity = input.Adapt<ConfigurationEntity>();
            var id = await _configurationRepository.AddAsync(entity);
            entity.Id = id;

            return entity.Adapt<ConfigurationResponseModel>();
        }

        public async Task<bool> UpdateAsync(UpdateConfigurationRequest input)
        {
            ArgumentNullException.ThrowIfNull(input);

            var existingEntity = await _configurationRepository.GetByIdAsync(input.Id);
            if (existingEntity == null)
                throw new InvalidOperationException("Configuration not found");

            if (!existingEntity.IsEditable)
                throw new InvalidOperationException("Configuration is not editable");

            ValidateUpdateConfiguration(input);

            existingEntity.Value = input.Value;
            existingEntity.Description = input.Description;
            existingEntity.Module = input.Module;
            existingEntity.DataType = input.DataType;
            existingEntity.DefaultValue = input.DefaultValue;
            existingEntity.IsEditable = input.IsEditable;
            existingEntity.IsActive = input.IsActive;
            existingEntity.UpdatedBy = input.UpdatedBy ?? "system";

            return await _configurationRepository.UpdateAsync(existingEntity);
        }

        public async Task<bool> ResetToDefaultAsync(long id, string updatedBy)
        {
            var configuration = await _configurationRepository.GetByIdAsync(id);
            if (configuration == null) return false;

            if (!configuration.IsEditable)
                throw new InvalidOperationException("Configuration is not editable");

            configuration.Value = configuration.DefaultValue;
            configuration.UpdatedBy = updatedBy;

            return await _configurationRepository.UpdateAsync(configuration);
        }

        #endregion

        #region Import/Export

        public async Task<int> ImportFromJsonAsync(string jsonData, string createdBy, string module = "Core")
        {
            var configs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData);
            if (configs == null) return 0;

            int importedCount = 0;
            foreach (var kvp in configs)
            {
                try
                {
                    var input = new CreateConfigurationRequest
                    {
                        Key = kvp.Key,
                        Value = kvp.Value?.ToString() ?? string.Empty,
                        Module = module,
                        DataType = "string",
                        CreatedBy = createdBy
                    };
                    await CreateAsync(input);
                    importedCount++;
                }
                catch (Exception ex)
                {
                    await _loggingService.LogErrorAsync(
                        $"Error importing configuration key: {kvp.Key}",
                        ex,
                        source: "ConfigurationService.ImportFromJsonAsync",
                        module: "Configuration",
                        action: "Import"
                    );
                    throw;
                }
            }

            return importedCount;
        }

        public async Task<string> ExportToJsonAsync(string? module = null, bool activeOnly = true)
        {
            var configurations = await _configurationRepository.GetAllAsync(activeOnly);
            if (module != null)
                configurations = configurations.Where(c => c.Module == module);

            return JsonSerializer.Serialize(configurations, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        #endregion

        #region Business Validation

        private async Task ValidateNewConfigurationAsync(CreateConfigurationRequest input)
        {
            if (!ModelHelper.IsValidCode(input.Key))
                throw new ArgumentException("Configuration key can only contain letters, numbers, and underscores");

            if (await KeyExistsAsync(input.Key))
                throw new ArgumentException("Configuration key already exists");

            if (!ValidDataTypes.Contains(input.DataType, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Data type must be one of: {string.Join(", ", ValidDataTypes)}");

            if (!string.IsNullOrWhiteSpace(input.Module) && !ModelHelper.IsValidModule(input.Module))
                throw new ArgumentException("Module can only contain letters, numbers, underscores, and dots");
        }

        private static void ValidateUpdateConfiguration(UpdateConfigurationRequest input)
        {
            if (!ValidDataTypes.Contains(input.DataType, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Data type must be one of: {string.Join(", ", ValidDataTypes)}");

            if (!string.IsNullOrWhiteSpace(input.Module) && !ModelHelper.IsValidModule(input.Module))
                throw new ArgumentException("Module can only contain letters, numbers, underscores, and dots");
        }

        #endregion
    }
}
