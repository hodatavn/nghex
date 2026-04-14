namespace Nghex.Core.Configuration
{
    /// <summary>
    /// Shared configuration reader contract for modules.
    /// Allows modules to consume configuration values without depending on a concrete configuration package.
    /// </summary>
    public interface IAppConfigurationReader
    {
        Task<string?> GetValueAsync(string key);
        Task<string> GetValueAsync(string key, bool useDefaultValue);
        Task<string> GetValueAsync(string key, string defaultValue);
        Task<int> GetIntValueAsync(string key);
        Task<int> GetIntValueAsync(string key, int defaultValue);
        Task<bool> GetBoolValueAsync(string key);
        Task<double> GetDoubleValueAsync(string key);
        Task<DateTime?> GetDateTimeValueAsync(string key);
        Task<T?> GetJsonValueAsync<T>(string key) where T : class;

        /// <summary>
        /// Reads a value from <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> (appsettings, env, etc.).
        /// Implemented in Core by <see cref="Configuration.AppSettingsConfigurationReader"/>; DB-backed services should delegate to the same logic for fallback.
        /// </summary>
        string GetAppSettingValueByKey(string key);
    }
}
