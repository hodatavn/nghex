using Microsoft.Extensions.Configuration;
using Nghex.Utilities;

namespace Nghex.Data.Providers
{
    /// <summary>
    /// Implementation of IEncryptionKeyProvider using IConfiguration
    /// </summary>
    public class ConfigurationKeyProvider(IConfiguration configuration) : IEncryptionKeyProvider
    {

        /// <summary>
        /// Gets encryption key from configuration
        /// Only uses Security:Key, ignores the key parameter
        /// </summary>
        /// <param name="key">Configuration key (ignored, always uses Security:Key)</param>
        /// <returns>Encryption key value from Security:Key or null if not found</returns>
        public string? GetKey(string key)
        {
            // Only use Security:Key, ignore other key definitions
            return configuration["Security:Key"];
        }

        /// <summary>
        /// Sets encryption key to configuration (read-only implementation)
        /// </summary>
        /// <param name="key">Configuration key</param>
        /// <param name="value">Encryption key value</param>
        /// <returns>False - this implementation is read-only</returns>
        public bool SetKey(string key, string value)
        {
            // IConfiguration is read-only in this context
            // Setting keys would require modifying the configuration source (e.g., appsettings.json)
            // which is beyond the scope of this provider
            return false;
        }
    }
}

