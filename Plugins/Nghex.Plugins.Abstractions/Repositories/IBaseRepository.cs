using Dapper;

namespace Nghex.Plugins.Abstractions.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Query get many records
        /// </summary>
        /// <typeparam name="T">Type of the record</typeparam>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>List of records</returns>
        Task<IEnumerable<T>> QueryManyAsync(string sql, DynamicParameters? parameters = null);

        /// <summary>
        /// Query get single record
        /// </summary>
        /// <typeparam name="T">Type of the record</typeparam>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>Record</returns>
        Task<T?> QuerySingleAsync(string sql, DynamicParameters? parameters = null);
        
        /// <summary>
        /// Insert record
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>ID of the inserted record</returns>
        Task<long> InsertAsync(string sql, DynamicParameters? parameters = null);
        
        /// <summary>
        /// Execute non-query command
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>Number of rows affected</returns>
        Task<int> ExecuteAsync(string sql, DynamicParameters? parameters = null);
    }
}

