using System.Data;
using Microsoft.Data.SqlClient;
using Nghex.Data.Enum;

namespace Nghex.Data.ExecutorHelpers
{
    /// <summary>
    /// SQL Server-specific database execution helper
    /// </summary>
    public class SqlServerExecutorHelper : IDatabaseExecutorHelper
    {
        public DataProvider ProviderName => DataProvider.SqlServer;

        /// <summary>
        /// Execute reader with SQL Server-specific async handling
        /// </summary>
        public async Task<IDataReader> ExecuteReaderAsync(IDbCommand command)
        {
            if (command is SqlCommand sqlCommand)
            {
                return await sqlCommand.ExecuteReaderAsync();
            }
            
            throw new InvalidOperationException($"Expected SqlCommand but got {command.GetType().Name}");
        }

        /// <summary>
        /// Execute command with SQL Server-specific async handling
        /// </summary>
        public async Task<int> ExecuteCommandAsync(IDbCommand command)
        {
            if (command is SqlCommand sqlCommand)
            {
                return await sqlCommand.ExecuteNonQueryAsync();
            }
            
            throw new InvalidOperationException($"Expected SqlCommand but got {command.GetType().Name}");
        }

        /// <summary>
        /// Execute scalar with SQL Server-specific async handling
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(IDbCommand command)
        {
            if (command is SqlCommand sqlCommand)
            {
                return await sqlCommand.ExecuteScalarAsync();
            }
            
            throw new InvalidOperationException($"Expected SqlCommand but got {command.GetType().Name}");
        }

        /// <summary>
        /// Open connection with SQL Server-specific async handling
        /// </summary>
        public async Task OpenConnectionAsync(IDbConnection connection)
        {
            if (connection is SqlConnection sqlConnection)
            {
                await sqlConnection.OpenAsync();
            }
            else
            {
                throw new InvalidOperationException($"Expected SqlConnection but got {connection.GetType().Name}");
            }
        }
    }
}
