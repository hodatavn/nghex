using System.Data;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Nghex.Utilities;

namespace Nghex.Data
{
    /// <summary>
    /// Implementation of Database Connection with configurable provider and pooling
    /// </summary>
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly IConfiguration _configuration;
        private readonly IDatabaseProviderFactory _providerFactory;
        private readonly string _connectionString;
        private readonly string _provider;

        private const int _defaultMinPool = 5;
        private const int _defaultMaxPool = 20;
        private const int _defaultConnectionTimeout = 120;

        public DatabaseConnection(IConfiguration configuration, IDatabaseProviderFactory providerFactory)
        {
            _configuration = configuration;
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            
            // Load from dataSetting.json: DataSettings:ConnectionString, DataSettings:DataProvider, and pooling options
            _provider = _configuration["DataSettings:DataProvider"] ?? "Oracle";

            // Check if ConnectionStringEncrypted is true
            var isEncrypted = IsConnectionStringEncrypted;
            var baseConn = isEncrypted ? _configuration["DataSettings:ConnectionString"] : _configuration["DataSettings:ConnectionStringNormal"];

            // If both are null, use default connection string
            if (string.IsNullOrWhiteSpace(baseConn))
            {
                baseConn = _configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string not found");
            }

            // Decrypt connection string if it's encrypted
            if (isEncrypted)
            {
                baseConn = DecryptConnectionString(baseConn);
            }

            string minPool = _configuration["DataSettings:MinPoolSize"] ?? _defaultMinPool.ToString();
            string maxPool = _configuration["DataSettings:MaxPoolSize"] ?? _defaultMaxPool.ToString();
            string connectionTimeout = _configuration["DataSettings:ConnectionTimeout"] ?? _defaultConnectionTimeout.ToString();

            // Append pooling parameters based on provider
            if (string.Equals(_provider, "Oracle", StringComparison.OrdinalIgnoreCase))
            {
                var oracleBuilder = new OracleConnectionStringBuilder(baseConn)
                {
                    MinPoolSize = int.Parse(minPool),
                    MaxPoolSize = int.Parse(maxPool),
                    ConnectionTimeout = int.Parse(connectionTimeout)
                };
                _connectionString = oracleBuilder.ConnectionString;                
            }
            else
            {
                // Future: support other providers (SqlServer, MySql, etc.)
                _connectionString = baseConn;
            }
        }

        /// <summary>
        /// Decrypts the connection string if it's encrypted, otherwise returns the original string
        /// </summary>
        /// <param name="connectionString">The connection string to decrypt</param>
        /// <returns>Decrypted connection string or original string if decryption is not needed or fails</returns>
        private string DecryptConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return connectionString;

            // Check if connection string is marked as encrypted (supports both boolean and string)
            var isExplicitlyEncrypted = IsConnectionStringEncrypted;

            // If not explicitly marked as encrypted, check if it looks encrypted
            if (!isExplicitlyEncrypted)
            {
                if (!IsEncrypted(connectionString))    
                    return connectionString;
            }

            try
            {
                // Attempt to decrypt
                Providers.ConfigurationKeyProvider? keyProvider = new(_configuration);
                var encryptionKey = _configuration["Security:Key"];
                
                if (string.IsNullOrWhiteSpace(encryptionKey) && isExplicitlyEncrypted)
                {
                    throw new InvalidOperationException(
                        "Encryption key not found in configuration section in appsettings.json.\n" +
                        "The encryption key must be the same key used to encrypt the connection string.");
                }
                
                var cryptography = new Cryptography(keyProvider);
                var decrypted = cryptography.Decrypt(connectionString);
                
                // Validate decrypted string is not empty
                if (string.IsNullOrWhiteSpace(decrypted))
                {
                    if (isExplicitlyEncrypted)
                        throw new InvalidOperationException("Decrypted connection string is empty. Please check your encryption key and encrypted connection string.");
                    return connectionString;
                }
                return decrypted;
            }
            catch (CryptographicException ex)
            {
                // If explicitly marked as encrypted, we must decrypt it - throw error
                if (isExplicitlyEncrypted)
                {
                    // Get the encryption key that was used (for debugging)
                    var usedKey = _configuration["Security:Key"] ?? "NOT FOUND";
                    
                    var keyInfo = string.IsNullOrWhiteSpace(usedKey) || usedKey == "NOT FOUND"
                        ? "No encryption key found in configuration"
                        : $"Encryption key found (length: {usedKey.Length} characters)";
                    
                    throw new InvalidOperationException(
                        $"Failed to decrypt connection string. {ex.Message}\n\n" +
                        $"Debugging information:\n" +
                        $"- {keyInfo}\n" +
                        $"- Connection string length: {connectionString.Length} characters\n" +
                        $"- Connection string starts with: {connectionString.Substring(0, Math.Min(20, connectionString.Length))}...\n\n" +
                        $"Possible causes:\n" +
                        $"1. The encryption key in configuration does not match the key used to encrypt the connection string\n" +
                        $"2. The connection string was encrypted with a different key\n" +
                        $"3. The connection string is corrupted or not properly encrypted\n\n" +
                        $"Solution:\n" +
                        $"1. Verify Security:Key in appsettings.json matches the key used to encrypt the connection string\n" +
                        $"2. Re-encrypt the connection string using the CryptographyConsole tool with the correct key\n" +
                        $"3. Or set 'ConnectionStringEncrypted' to false if the connection string is not encrypted", ex);
                }
                
                // If not explicitly encrypted and decryption fails, return original string
                // This allows auto-detection to fail gracefully
                return connectionString;
            }
            catch (Exception ex)
            {
                // If explicitly marked as encrypted, rethrow with context
                if (isExplicitlyEncrypted)
                {
                    throw new InvalidOperationException($"Error decrypting connection string: {ex.Message}", ex);
                }
                
                // If not explicitly encrypted, return original string
                return connectionString;
            }
        }

        public IDbConnection CreateConnection()
        {
            try
            {
                var databaseProvider = _providerFactory.GetProvider(_provider);
                return databaseProvider.CreateConnection(_connectionString);
            }
            catch (NotSupportedException ex)
            {
                throw new NotSupportedException($"Unsupported data provider: {_provider}. {ex.Message}", ex);
            }
        }

        public string GetConnectionString() => _connectionString;
        
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var databaseProvider = _providerFactory.GetProvider(_provider);
                var result = await databaseProvider.TestConnectionAsync(_connectionString);
                if (!result)
                {
                    Console.WriteLine($"Failed to connect to database. Provider: {_provider}");
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing database connection: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public string GetProviderName()
        {
            return _provider;
        }

        public bool IsProviderSupported()
        {
            return _providerFactory.IsProviderSupported(_provider);
        }


        /// <summary>
        /// Checks if a string is encrypted (valid base64 format)
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns>True if the string appears to be encrypted (valid base64 and reasonably long)</returns>
        private static bool IsEncrypted(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            if (value.Length < 16)
                return false;
            try
            {
                var bytes = Convert.FromBase64String(value);
                return bytes.Length > 16;
            }
            catch
            {
                return false;
            }
        }

        private bool IsConnectionStringEncrypted => bool.TryParse(_configuration["DataSettings:ConnectionStringEncrypted"], out var isEncrypted) && isEncrypted;
        
        
    }
}
