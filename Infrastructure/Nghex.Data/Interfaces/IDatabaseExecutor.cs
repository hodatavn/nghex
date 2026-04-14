using System.Data;
using Dapper;

namespace Nghex.Data
{
    /// <summary>
    /// Interface for database operations executor
    /// </summary>
    public interface IDatabaseExecutor
    {
        /// <summary>
        /// Execute query và return single result
        /// </summary>
        Task<T?> ExecuteQuerySingleAsync<T>(string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute query và return multiple results
        /// </summary>
        Task<IEnumerable<T>> ExecuteQueryMultipleAsync<T>(string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute non-query command (INSERT, UPDATE, DELETE)
        /// </summary>
        Task<int> ExecuteNonQueryAsync(string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute scalar query (COUNT, EXISTS, etc.)
        /// </summary>
        Task<T> ExecuteScalarAsync<T>(string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute INSERT với RETURNING clause và return ID
        /// </summary>
        Task<long> ExecuteInsertWithReturnIdAsync(string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Check if value exists in database
        /// </summary>
        Task<bool> ExistsAsync(string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Get count from database
        /// </summary>
        Task<long> CountAsync(string query, DynamicParameters? parameters = null);

        #region Transaction Management

        /// <summary>
        /// Execute multiple operations within a transaction
        /// </summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<IDbTransaction, Task<T>> operation);

        /// <summary>
        /// Execute multiple operations within a transaction (void)
        /// </summary>
        Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> operation);

        /// <summary>
        /// Execute multiple operations within a transaction with isolation level
        /// </summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<IDbTransaction, Task<T>> operation, IsolationLevel isolationLevel);

        /// <summary>
        /// Execute multiple operations within a transaction with isolation level (void)
        /// </summary>
        Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> operation, IsolationLevel isolationLevel);

        #endregion

        #region Transaction Helper Methods

        /// <summary>
        /// Execute query trong transaction và return single result
        /// </summary>
        Task<T?> ExecuteQuerySingleInTransactionAsync<T>(IDbTransaction transaction, string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute query trong transaction và return multiple results
        /// </summary>
        Task<IEnumerable<T>> ExecuteQueryMultipleInTransactionAsync<T>(IDbTransaction transaction, string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute non-query command trong transaction (INSERT, UPDATE, DELETE)
        /// </summary>
        Task<int> ExecuteNonQueryInTransactionAsync(IDbTransaction transaction, string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute scalar query trong transaction (COUNT, EXISTS, etc.)
        /// </summary>
        Task<T> ExecuteScalarInTransactionAsync<T>(IDbTransaction transaction, string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Execute INSERT với RETURNING clause trong transaction và return ID
        /// </summary>
        Task<long> ExecuteInsertWithReturnIdInTransactionAsync(IDbTransaction transaction, string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Check if value exists trong transaction
        /// </summary>
        Task<bool> ExistsInTransactionAsync(IDbTransaction transaction, string query, DynamicParameters? parameters = null);

        /// <summary>
        /// Get count trong transaction
        /// </summary>
        Task<long> CountInTransactionAsync(IDbTransaction transaction, string query, DynamicParameters? parameters = null);

        #endregion
    }
}
