using Microsoft.Extensions.Configuration;
using Nghex.Data.Enum;
using Nghex.Data.Providers;

namespace Nghex.Data.Factory
{
    
    /// <summary>
    /// Implementation of database provider factory
    /// </summary>
    public class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly Dictionary<DataProvider, IDatabaseProvider> _providers;

        public DatabaseProviderFactory(IConfiguration configuration)
        {
            _providers = new Dictionary<DataProvider, IDatabaseProvider>()
            {
                { DataProvider.Oracle, new OracleProvider(configuration) },
                { DataProvider.SqlServer, new SqlServerProvider(configuration) },
                { DataProvider.MSSQL, new SqlServerProvider(configuration) }, // Alias for SqlServer
            };
        }

        public IDatabaseProvider GetProvider(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));

            if (_providers.TryGetValue(providerName.ProviderString(), out var provider))
            {
                return provider;
            }

            throw new NotSupportedException($"Database provider '{providerName}' is not supported. Supported providers: {string.Join(", ", _providers.Keys)}");
        }

        public IEnumerable<IDatabaseProvider> GetAllProviders()
        {
            return _providers.Values;
        }

        public bool IsProviderSupported(string providerName)
        {
            return !string.IsNullOrEmpty(providerName) && _providers.ContainsKey(providerName.ProviderString());
        }
    }
}


