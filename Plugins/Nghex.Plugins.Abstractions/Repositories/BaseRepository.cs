using Dapper;
using Nghex.Core.Setting;
using Nghex.Data;
using Nghex.Plugins.Abstractions.Repositories.Interfaces;

namespace Nghex.Plugins.Abstractions.Repositories
{
    /// <summary>
    /// Base repository providing shared schema and query helpers
    /// </summary>
    public abstract class BaseRepository<T>(IDatabaseExecutor databaseExecutor) : IBaseRepository<T> where T : class
    {
        protected string Schema => AppSettings.DefaultDataSchema;
        protected IDatabaseExecutor DatabaseExecutor => databaseExecutor;

        protected string BuildSqlWithSchema(string sql)
        {
            return sql.Replace("<SCHEMA>", Schema);
        }

        /// <summary>
        /// Query get many records
        /// </summary>
        /// <typeparam name="T">Type of the record</typeparam>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>List of records</returns>
        public Task<IEnumerable<T>> QueryManyAsync(string sql, DynamicParameters? parameters = null)
        {
            return databaseExecutor.ExecuteQueryMultipleAsync<T>(BuildSqlWithSchema(sql), parameters);
        }

        /// <summary>
        /// Query get single record
        /// </summary>
        /// <typeparam name="T">Type of the record</typeparam>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>Record</returns>
        public Task<T?> QuerySingleAsync(string sql, DynamicParameters? parameters = null)
        {
            return databaseExecutor.ExecuteQuerySingleAsync<T>(BuildSqlWithSchema(sql), parameters);
        }

        /// <summary>
        /// Insert record
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>ID of the inserted record</returns>
        public Task<long> InsertAsync(string sql, DynamicParameters? parameters = null)
        {
            return databaseExecutor.ExecuteInsertWithReturnIdAsync(BuildSqlWithSchema(sql), parameters);
        }

        /// <summary>
        /// Execute non-query command
        /// </summary>
        /// <param name="sql">SQL query</param>
        /// <param name="parameters">Parameters for the query</param>
        /// <returns>Number of rows affected</returns>
        public Task<int> ExecuteAsync(string sql, DynamicParameters? parameters = null)
        {
            return databaseExecutor.ExecuteNonQueryAsync(BuildSqlWithSchema(sql), parameters);
        }
    }
}

