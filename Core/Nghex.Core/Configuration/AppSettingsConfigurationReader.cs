using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Nghex.Core.Configuration
{
    /// <summary>
    /// Reads configuration from <see cref="IConfiguration"/> (appsettings.json, env vars, etc.).
    /// Does not use the database — register via <see cref="Nghex.Core.Extension.NghexCoreServiceExtensions.AddNghexAppConfiguration"/>.
    /// </summary>
    public sealed class AppSettingsConfigurationReader : IAppConfigurationReader
    {
        private readonly IConfiguration _configuration;

        public AppSettingsConfigurationReader(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Shared appsettings resolution (colon keys, sections, GetValue). Use from DB-backed readers as fallback.
        /// </summary>
        public static string GetAppSettingValueFromConfiguration(IConfiguration configuration, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            var value = configuration[key];

            if (string.IsNullOrEmpty(value) && key.Contains(':'))
            {
                var section = configuration.GetSection(key);
                if (section.Value != null)
                    value = section.Value;
            }

            if (string.IsNullOrEmpty(value))
                value = configuration.GetValue<string>(key, string.Empty);

            return value ?? string.Empty;
        }

        public string GetAppSettingValueByKey(string key) =>
            GetAppSettingValueFromConfiguration(_configuration, key);

        public Task<string?> GetValueAsync(string key)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(string.IsNullOrEmpty(s) ? null : s);
        }

        public Task<string> GetValueAsync(string key, string defaultValue)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(string.IsNullOrEmpty(s) ? defaultValue : s);
        }

        /// <inheritdoc />
        /// <remarks><paramref name="useDefaultValue"/> applies to DB-backed configuration only; for appsettings it is ignored.</remarks>
        public Task<string> GetValueAsync(string key, bool useDefaultValue)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(s);
        }

        public Task<int> GetIntValueAsync(string key)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(int.TryParse(s, out var v) ? v : 0);
        }

        public Task<int> GetIntValueAsync(string key, int defaultValue)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(int.TryParse(s, out var v) ? v : defaultValue);
        }

        public Task<bool> GetBoolValueAsync(string key)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(bool.TryParse(s, out var v) && v);
        }

        public Task<double> GetDoubleValueAsync(string key)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(double.TryParse(s, out var v) ? v : 0);
        }

        public Task<DateTime?> GetDateTimeValueAsync(string key)
        {
            var s = GetAppSettingValueByKey(key);
            return Task.FromResult(DateTime.TryParse(s, out var v) ? v : (DateTime?)null);
        }

        public Task<T?> GetJsonValueAsync<T>(string key) where T : class
        {
            var s = GetAppSettingValueByKey(key);
            if (string.IsNullOrWhiteSpace(s)) return Task.FromResult<T?>(null);
            try
            {
                var result = JsonSerializer.Deserialize<T>(s, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                return Task.FromResult(result);
            }
            catch
            {
                return Task.FromResult<T?>(null);
            }
        }
    }
}
