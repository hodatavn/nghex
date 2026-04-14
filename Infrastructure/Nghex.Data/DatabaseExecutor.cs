using System.Data;
using Nghex.Data.Factory.Interfaces;
using Nghex.Data.Enum;
using Dapper;

namespace Nghex.Data
{
    /// <summary>
    /// Implementation of database operations executor
    /// </summary>
    public class DatabaseExecutor(IDatabaseConnection dbConnection, IDatabaseExecutorHelperFactory helperFactory) : IDatabaseExecutor
    {
        private readonly IDatabaseConnection _dbConnection = dbConnection;
        private readonly IDatabaseExecutorHelperFactory _helperFactory = helperFactory;
        private IDatabaseExecutorHelper? _cachedHelper;

        #region Query Execution (No Transaction)

        /// <summary>
        /// Execute query và return single result
        /// </summary>
        public async Task<T?> ExecuteQuerySingleAsync<T>(string query, DynamicParameters? parameters = null)
        {
            using var connection = await CreateAndOpenConnectionAsync();
            var data = await ExecuteQueryCoreAsync<T>(connection, null, query, parameters, "Error executing query");
            return data.FirstOrDefault();
        }

        /// <summary>
        /// Execute query và return multiple results
        /// </summary>
        public async Task<IEnumerable<T>> ExecuteQueryMultipleAsync<T>(string query, DynamicParameters? parameters = null)
        {
            using var connection = await CreateAndOpenConnectionAsync();
            return await ExecuteQueryCoreAsync<T>(connection, null, query, parameters, "Error executing query");
        }

        /// <summary>
        /// Execute non-query command (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string query, DynamicParameters? parameters = null)
        {
            using var connection = await CreateAndOpenConnectionAsync();
            return await ExecuteNonQueryCoreAsync(connection, null, query, parameters, "Error executing non-query");
        }

        /// <summary>
        /// Execute scalar query (COUNT, EXISTS, etc.)
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(string query, DynamicParameters? parameters = null)
        {
            using var connection = await CreateAndOpenConnectionAsync();
            return await ExecuteScalarCoreAsync<T>(connection, null, query, parameters, "Error executing scalar query");
        }

        /// <summary>
        /// Execute INSERT với RETURNING clause và return ID
        /// </summary>
        public async Task<long> ExecuteInsertWithReturnIdAsync(string query, DynamicParameters? parameters = null)
        {
            using var connection = await CreateAndOpenConnectionAsync();
            return await ExecuteInsertWithReturnIdCoreAsync(connection, null, query, parameters, "Error executing insert with return ID");
        }

        /// <summary>
        /// Check if value exists in database
        /// </summary>
        public async Task<bool> ExistsAsync(string query, DynamicParameters? parameters = null)
        {
            var count = await ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }

        /// <summary>
        /// Get count from database
        /// </summary>
        public async Task<long> CountAsync(string query, DynamicParameters? parameters = null)
        {
            return await ExecuteScalarAsync<long>(query, parameters);
        }

        #endregion

        #region Transaction Management

        /// <summary>
        /// Execute multiple operations within a transaction
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbTransaction, Task<T>> operation)
        {
            return await ExecuteInTransactionAsync(operation, IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Execute multiple operations within a transaction (void)
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> operation)
        {
            await ExecuteInTransactionAsync(async transaction =>
            {
                await operation(transaction);
                return true;
            }, IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Execute multiple operations within a transaction with isolation level
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbTransaction, Task<T>> operation, IsolationLevel isolationLevel)
        {
            using var connection = await CreateAndOpenConnectionAsync();
            using var transaction = connection.BeginTransaction(isolationLevel);
            
            try
            {
                var result = await operation(transaction);
                transaction.Commit();
                return result;
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    throw new Exception($"Transaction rollback failed: {rollbackEx.Message}", ex);
                }
                
                throw new Exception($"Transaction failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Execute multiple operations within a transaction with isolation level (void)
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> operation, IsolationLevel isolationLevel)
        {
            await ExecuteInTransactionAsync(async transaction =>
            {
                await operation(transaction);
                return true;
            }, isolationLevel);
        }

        #endregion

        #region Transaction Helper Methods

        /// <summary>
        /// Execute query trong transaction và return single result
        /// </summary>
        public async Task<T?> ExecuteQuerySingleInTransactionAsync<T>(
            IDbTransaction transaction,
            string query, 
            DynamicParameters? parameters = null)
        {
            var connection = GetConnectionFromTransaction(transaction);
            var data = await ExecuteQueryCoreAsync<T>(connection, transaction, query, parameters, "Error executing query in transaction");
            return data.FirstOrDefault();
        }

        /// <summary>
        /// Execute query trong transaction và return multiple results
        /// </summary>
        public async Task<IEnumerable<T>> ExecuteQueryMultipleInTransactionAsync<T>(
            IDbTransaction transaction,
            string query, 
            DynamicParameters? parameters = null)
        {
            var connection = GetConnectionFromTransaction(transaction);
            return await ExecuteQueryCoreAsync<T>(connection, transaction, query, parameters, "Error executing query in transaction");
        }

        /// <summary>
        /// Execute non-query command trong transaction (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<int> ExecuteNonQueryInTransactionAsync(
            IDbTransaction transaction,
            string query, 
            DynamicParameters? parameters = null)
        {
            var connection = GetConnectionFromTransaction(transaction);
            return await ExecuteNonQueryCoreAsync(connection, transaction, query, parameters, "Error executing non-query in transaction");
        }

        /// <summary>
        /// Execute scalar query trong transaction (COUNT, EXISTS, etc.)
        /// </summary>
        public async Task<T> ExecuteScalarInTransactionAsync<T>(
            IDbTransaction transaction,
            string query, 
            DynamicParameters? parameters = null)
        {
            var connection = GetConnectionFromTransaction(transaction);
            return await ExecuteScalarCoreAsync<T>(connection, transaction, query, parameters, "Error executing scalar query in transaction");
        }

        /// <summary>
        /// Execute INSERT với RETURNING clause trong transaction và return ID
        /// </summary>
        public async Task<long> ExecuteInsertWithReturnIdInTransactionAsync(
            IDbTransaction transaction,
            string query, 
            DynamicParameters? parameters = null)
        {
            var connection = GetConnectionFromTransaction(transaction);
            return await ExecuteInsertWithReturnIdCoreAsync(connection, transaction, query, parameters, "Error executing insert with return ID in transaction");
        }

        /// <summary>
        /// Check if value exists trong transaction
        /// </summary>
        public async Task<bool> ExistsInTransactionAsync(
            IDbTransaction transaction,
            string query, 
            DynamicParameters? parameters = null)
        {
            var count = await ExecuteScalarInTransactionAsync<int>(transaction, query, parameters);
            return count > 0;
        }

        /// <summary>
        /// Get count trong transaction
        /// </summary>
        public async Task<long> CountInTransactionAsync(
            IDbTransaction transaction,
            string query, 
            DynamicParameters? parameters = null)
        {
            return await ExecuteScalarInTransactionAsync<long>(transaction, query, parameters);
        }

        #endregion

        #region Core Execution Methods (Private)

        /// <summary>
        /// Core method for executing queries - used by both transaction and non-transaction methods
        /// </summary>
        private static async Task<IEnumerable<T>> ExecuteQueryCoreAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction,
            string query,
            DynamicParameters? parameters,
            string errorMessage)
        {
            try
            {
                parameters ??= new DynamicParameters();
                return await connection.QueryAsync<T>(query, parameters, transaction);
            }
            catch (Exception ex)
            {
                throw new Exception($"{errorMessage}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Core method for executing non-query commands - used by both transaction and non-transaction methods
        /// </summary>
        private static async Task<int> ExecuteNonQueryCoreAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            string query,
            DynamicParameters? parameters,
            string errorMessage)
        {
            try
            {
                parameters ??= new DynamicParameters();
                return await connection.ExecuteAsync(query, parameters, transaction);
            }
            catch (Exception ex)
            {
                throw new Exception($"{errorMessage}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Core method for executing scalar queries - used by both transaction and non-transaction methods
        /// </summary>
        private static async Task<T> ExecuteScalarCoreAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction,
            string query,
            DynamicParameters? parameters,
            string errorMessage)
        {
            try
            {
                parameters ??= new DynamicParameters();
                var result = await connection.ExecuteScalarAsync<object>(query, parameters, transaction);
                return result != null ? (T)Convert.ChangeType(result, typeof(T)) : default!;
            }
            catch (Exception ex)
            {
                throw new Exception($"{errorMessage}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Core method for executing insert with return ID - used by both transaction and non-transaction methods.
        /// For Oracle RETURNING ID INTO :Id, the ID is bound to an output parameter; we execute the command
        /// then read the parameter value (not a result set).
        /// </summary>
        private static async Task<long> ExecuteInsertWithReturnIdCoreAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            string query,
            DynamicParameters? parameters,
            string errorMessage)
        {
            try
            {
                parameters ??= new DynamicParameters();
                if (!parameters.ParameterNames.Contains("Id"))
                    parameters.Add("Id", dbType: DbType.Int64, direction: ParameterDirection.Output);
                await connection.ExecuteAsync(query, parameters, transaction);
                var id = parameters.Get<object>("Id");
                return id != null ? Convert.ToInt64(id) : 0L;
            }
            catch (Exception ex)
            {
                throw new Exception($"{errorMessage}: {ex.Message}", ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get connection from transaction with validation
        /// </summary>
        private static IDbConnection GetConnectionFromTransaction(IDbTransaction transaction)
        {
            return transaction.Connection 
                ?? throw new InvalidOperationException("Transaction has no associated connection");
        }

        /// <summary>
        /// Get current database executor helper based on connection type
        /// </summary>
        private IDatabaseExecutorHelper GetCurrentHelper()
        {
            if (_cachedHelper != null)
                return _cachedHelper;

            var providerName = _dbConnection.GetProviderName();
            var dataProvider = providerName.ProviderString();
            
            _cachedHelper = _helperFactory.GetHelper(dataProvider);
            
            return _cachedHelper;
        }

        /// <summary>
        /// Create and open connection with proper async handling
        /// </summary>
        private async Task<IDbConnection> CreateAndOpenConnectionAsync()
        {
            var connection = _dbConnection.CreateConnection();
            var helper = GetCurrentHelper();
            
            await helper.OpenConnectionAsync(connection);
            
            return connection;
        }

        #endregion
    }
}
