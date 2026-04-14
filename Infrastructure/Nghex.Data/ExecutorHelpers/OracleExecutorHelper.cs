using System.Data;
using Nghex.Data.Enum;
using Oracle.ManagedDataAccess.Client;

namespace Nghex.Data.ExecutorHelpers
{
    /// <summary>
    /// Oracle-specific database execution helper
    /// </summary>
    public class OracleExecutorHelper : IDatabaseExecutorHelper
    {
        public DataProvider ProviderName => DataProvider.Oracle;

        /// <summary>
        /// Execute reader with Oracle-specific async handling
        /// </summary>
        public async Task<IDataReader> ExecuteReaderAsync(IDbCommand command)
        {
            if (command is OracleCommand oracleCommand)
            {
                return await oracleCommand.ExecuteReaderAsync();
            }
            
            throw new InvalidOperationException($"Expected OracleCommand but got {command.GetType().Name}");
        }

        /// <summary>
        /// Execute command with Oracle-specific async handling
        /// </summary>
        public async Task<int> ExecuteCommandAsync(IDbCommand command)
        {
            if (command is OracleCommand oracleCommand)
            {
                return await oracleCommand.ExecuteNonQueryAsync();
            }
            
            throw new InvalidOperationException($"Expected OracleCommand but got {command.GetType().Name}");
        }

        /// <summary>
        /// Execute scalar with Oracle-specific async handling
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(IDbCommand command)
        {
            if (command is OracleCommand oracleCommand)
            {
                return await oracleCommand.ExecuteScalarAsync();
            }
            
            throw new InvalidOperationException($"Expected OracleCommand but got {command.GetType().Name}");
        }

        /// <summary>
        /// Open connection with Oracle-specific async handling
        /// </summary>
        public async Task OpenConnectionAsync(IDbConnection connection)
        {
            try
            {
                if (connection is OracleConnection oracleConnection)
                    await oracleConnection.OpenAsync();
                else
                {
                    throw new InvalidOperationException($"Expected OracleConnection but got {connection.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error opening Oracle connection: {ex.Message}", ex);
            }
        }
    }
}
