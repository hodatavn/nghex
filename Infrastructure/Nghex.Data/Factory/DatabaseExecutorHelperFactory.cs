using System.Data;
using Nghex.Data.Enum;
using Nghex.Data.ExecutorHelpers;
using Nghex.Data.Factory.Interfaces;

namespace Nghex.Data.Factory
{
    /// <summary>
    /// Factory implementation for creating database executor helpers
    /// </summary>
    public class DatabaseExecutorHelperFactory : IDatabaseExecutorHelperFactory
    {
        private readonly Dictionary<DataProvider, IDatabaseExecutorHelper> _helpers;

        public DatabaseExecutorHelperFactory()
        {
            _helpers = new Dictionary<DataProvider, IDatabaseExecutorHelper>
            {
                { DataProvider.Oracle, new OracleExecutorHelper() },
                { DataProvider.SqlServer, new SqlServerExecutorHelper() },
                { DataProvider.MSSQL, new SqlServerExecutorHelper() } // Alias for SqlServer
            };
        }

        /// <summary>
        /// Get executor helper for specific provider
        /// </summary>
        public IDatabaseExecutorHelper GetHelper(DataProvider dataProvider)
        {
            if (_helpers.TryGetValue(dataProvider, out var helper))
            {
                return helper;
            }

            throw new NotSupportedException($@"
            Database executor helper for provider '{dataProvider.GetProviderName()}' is not supported. 
            Supported providers: {string.Join(", ", _helpers.Keys.Select(k => k.GetProviderName()))}");
        }

        /// <summary>
        /// Get executor helper based on connection type
        /// </summary>
        public IDatabaseExecutorHelper GetHelper(IDbConnection connection)
        {
            return connection switch
            {
                Oracle.ManagedDataAccess.Client.OracleConnection => GetHelper(DataProvider.Oracle),
                Microsoft.Data.SqlClient.SqlConnection => GetHelper(DataProvider.SqlServer),
                _ => throw new NotSupportedException($"Connection type '{connection.GetType().Name}' is not supported")
            };
        }
    }
}
