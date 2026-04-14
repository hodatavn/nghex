using OurLogLevel = Nghex.Core.Enum.LogLevel;
using Nghex.Logging.Interfaces;
using Nghex.Logging.Models;
using Nghex.Data;
using Dapper;
using System.Data;

namespace Nghex.Logging.Repositories
{
    /// <summary>
    /// Log Repository implementation
    /// </summary>
    public class LogRepository(IDatabaseExecutor databaseExecutor) : ILogRepository
    {
        private readonly IDatabaseExecutor _databaseExecutor = databaseExecutor;

        public async Task<long> AddAsync(LogEntry entity)
        {
            try
            {
                var query = @"
                    INSERT INTO sys_logs (
                        LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME, 
                        IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION, 
                        LOG_EXCEPTION, STACK_TRACE, CREATED_AT, CREATED_BY
                    ) VALUES (
                        :LogLevel, :Message, :Details, :Source, :UserId, :Username,
                        :IpAddress, :UserAgent, :RequestId, :Module, :Action,
                        :Exception, :StackTrace,
                        SYSDATE, :CreatedBy
                    ) RETURNING ID INTO :Id";
                var parameters = new DynamicParameters();
                parameters.Add("LogLevel", entity.LogLevel, DbType.Int32);
                parameters.Add("Message", entity.Message, DbType.String);
                parameters.Add("Details", entity.Details, DbType.String);
                parameters.Add("Source", entity.Source, DbType.String);
                parameters.Add("UserId", entity.UserId, DbType.Int64);
                parameters.Add("Username", entity.Username, DbType.String);
                parameters.Add("IpAddress", entity.IpAddress, DbType.String);
                parameters.Add("UserAgent", entity.UserAgent, DbType.String);
                parameters.Add("RequestId", entity.RequestId, DbType.String);
                parameters.Add("Module", entity.Module, DbType.String);
                parameters.Add("Action", entity.Action, DbType.String);
                parameters.Add("Exception", entity.Exception, DbType.String);
                parameters.Add("StackTrace", entity.StackTrace, DbType.String);
                parameters.Add("CreatedBy", entity.CreatedBy, DbType.String);
                parameters.Add("Id", entity.Id, DbType.Int64);
                return await _databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding log to database: {ex.Message}");
                return 0;
            }
        }

        public async Task<LogEntry?> GetByIdAsync(long id)
        {
            try
            {
                var query = @"
                    SELECT ID, LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME,
                           IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION,
                           LOG_EXCEPTION, STACK_TRACE
                    FROM sys_logs 
                    WHERE ID = :Id";
                var parameters = new DynamicParameters();
                parameters.Add("Id", id, DbType.Int64);
                return await _databaseExecutor.ExecuteQuerySingleAsync<LogEntry>(
                    query,
                    parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting log by ID {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var query = "DELETE FROM sys_logs WHERE ID = :Id";
                var parameters = new DynamicParameters();
                parameters.Add("Id", id, DbType.Int64);
                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting log {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<LogEntry>> GetByLevelAsync(OurLogLevel level, int offset = 0, int limit = 100)
        {
            try
            {
                var query = @"
                    SELECT ID, LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME,
                           IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION,
                           LOG_EXCEPTION, STACK_TRACE
                    FROM sys_logs 
                    WHERE LOG_LEVEL = :LogLevel
                    ORDER BY ID ASC 
                    OFFSET :Offset ROWS FETCH NEXT :Limit ROWS ONLY";
                var parameters = new DynamicParameters();
                parameters.Add("LogLevel", (int)level, DbType.Int32);
                parameters.Add("Offset", offset, DbType.Int32);
                parameters.Add("Limit", limit, DbType.Int32);
                return await _databaseExecutor.ExecuteQueryMultipleAsync<LogEntry>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting logs by level {level}: {ex.Message}");
                return [];
            }
        }
        public async Task<IEnumerable<LogEntry>> GetByUserAsync(string username, int offset = 0, int limit = 100)
        {
            try
            {
                var query = @"
                    SELECT ID, LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME,
                           IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION,
                           LOG_EXCEPTION, STACK_TRACE
                    FROM sys_logs 
                    WHERE USERNAME = :Username
                    ORDER BY CREATED_AT DESC
                    OFFSET :Offset ROWS FETCH NEXT :Limit ROWS ONLY";
                var parameters = new DynamicParameters();
                parameters.Add("Username", username, DbType.String);
                parameters.Add("Offset", offset, DbType.Int32);
                parameters.Add("Limit", limit, DbType.Int32);
                return await _databaseExecutor.ExecuteQueryMultipleAsync<LogEntry>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting logs by user {username}: {ex.Message}");
                return [];
            }
        }

        public async Task<IEnumerable<LogEntry>> GetByModuleAsync(string module, int offset = 0, int limit = 100)
        {
            try
            {
                var query = @"
                    SELECT ID, LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME,
                           IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION,
                           LOG_EXCEPTION, STACK_TRACE
                    FROM sys_logs 
                    WHERE MODULE = :Module
                    ORDER BY CREATED_AT DESC
                    OFFSET :Offset ROWS FETCH NEXT :Limit ROWS ONLY";
                var parameters = new DynamicParameters();
                parameters.Add("Module", module, DbType.String);
                parameters.Add("Offset", offset, DbType.Int32);
                parameters.Add("Limit", limit, DbType.Int32);
                return await _databaseExecutor.ExecuteQueryMultipleAsync<LogEntry>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting logs by module {module}: {ex.Message}");
                return [];
            }
        }

        public async Task<IEnumerable<LogEntry>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int offset = 0, int limit = 100)
        {
            try
            {
                var query = @"
                    SELECT ID, LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME,
                           IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION,
                           LOG_EXCEPTION, STACK_TRACE
                    FROM sys_logs 
                    WHERE CREATED_AT BETWEEN :FromDate AND :ToDate
                    ORDER BY CREATED_AT ASC
                    OFFSET :Offset ROWS FETCH NEXT :Limit ROWS ONLY";

                var parameters = new DynamicParameters();
                parameters.Add("FromDate", fromDate, DbType.DateTime);
                parameters.Add("ToDate", toDate, DbType.DateTime);
                parameters.Add("Offset", offset, DbType.Int32);
                parameters.Add("Limit", limit, DbType.Int32);
                return await _databaseExecutor.ExecuteQueryMultipleAsync<LogEntry>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting logs by date range {fromDate} - {toDate}: {ex.Message}");
                return [];
            }
        }

        public async Task<IEnumerable<LogEntry>> SearchAsync(string keyword, int offset = 0, int limit = 100)
        {
            try
            {
                var query = @"
                    SELECT ID, LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME,
                           IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION,
                           LOG_EXCEPTION, STACK_TRACE
                    FROM sys_logs 
                    WHERE (UPPER(MESSAGE) LIKE UPPER(:Keyword) 
                           OR UPPER(DETAILS) LIKE UPPER(:Keyword)
                           OR UPPER(SOURCE) LIKE UPPER(:Keyword)
                           OR UPPER(USERNAME) LIKE UPPER(:Keyword)
                           OR UPPER(MODULE) LIKE UPPER(:Keyword)    
                           OR UPPER(ACTION) LIKE UPPER(:Keyword))
                    ORDER BY ID 
                    OFFSET :Offset ROWS FETCH NEXT :Limit ROWS ONLY";
                var parameters = new DynamicParameters();
                parameters.Add("Keyword", $"%{keyword}%", DbType.String);
                parameters.Add("Offset", offset, DbType.Int32);
                parameters.Add("Limit", limit, DbType.Int32);
                return await _databaseExecutor.ExecuteQueryMultipleAsync<LogEntry>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching logs {keyword}: {ex.Message}");
                return [];
            }
        }

        public async Task<int> DeleteOldLogsAsync(DateTime beforeDate)
        {
            try
            {
                // var query = "DELETE FROM sys_logs WHERE CREATED_AT < TO_DATE(:BeforeDate, 'DD-MM-YYYY')";
                var query = "DELETE FROM sys_logs WHERE CREATED_AT < :BeforeDate";
                var parameters = new DynamicParameters();
                // parameters.Add("BeforeDate", beforeDate.ToString("dd-MM-yyyy"), DbType.String);
                parameters.Add("BeforeDate", beforeDate, DbType.DateTime);
                return await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting old logs before {beforeDate}: {ex.Message}");
                return 0;
            }
        }

        public async Task<long> CountByLevelAsync(OurLogLevel level)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM sys_logs WHERE LOG_LEVEL = :LogLevel";
                var parameters = new DynamicParameters();
                parameters.Add("LogLevel", (int)level, DbType.Int32);
                return await _databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting logs by level {level} (value: {(int)level}): {ex.Message}");
                return 0;
            }
        }

        public async Task<long> CountByUserAsync(string username)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM sys_logs WHERE USERNAME = :Username";
                var parameters = new DynamicParameters();
                parameters.Add("Username", username, DbType.String);
                return await _databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting logs by user {username}: {ex.Message}");
                return 0;
            }
        }

        public async Task<long> CountByModuleAsync(string module)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM sys_logs WHERE MODULE = :Module";
                var parameters = new DynamicParameters();
                parameters.Add("Module", module, DbType.String);
                return await _databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting logs by module {module}: {ex.Message}");
                return 0;
            }
        }

        public async Task<long> CountByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM sys_logs WHERE CREATED_AT BETWEEN :FromDate AND :ToDate";
                var parameters = new DynamicParameters();
                parameters.Add("FromDate", fromDate, DbType.DateTime);
                parameters.Add("ToDate", toDate, DbType.DateTime);
                return await _databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting logs by date range {fromDate} - {toDate}: {ex.Message}");
                return 0;
            }
        }

        public async Task<long> CountBySearchAsync(string keyword)
        {
            try
            {
                var query = @"
                    SELECT COUNT(*) 
                    FROM sys_logs
                    WHERE (UPPER(MESSAGE) LIKE UPPER(:Keyword) 
                           OR UPPER(DETAILS) LIKE UPPER(:Keyword)
                           OR UPPER(SOURCE) LIKE UPPER(:Keyword)
                           OR UPPER(USERNAME) LIKE UPPER(:Keyword)
                           OR UPPER(MODULE) LIKE UPPER(:Keyword)    
                           OR UPPER(ACTION) LIKE UPPER(:Keyword))";
                var parameters = new DynamicParameters();
                parameters.Add("Keyword", $"%{keyword}%", DbType.String);
                return await _databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error counting search logs {keyword}: {ex.Message}");
                return 0;
            }
        }

        public async Task<IEnumerable<LogEntry>> GetByRequestIdAsync(string requestId)
        {
            try
            {
                var query = @"
                    SELECT ID, LOG_LEVEL, MESSAGE, DETAILS, SOURCE, USER_ID, USERNAME,
                           IP_ADDRESS, USER_AGENT, REQUEST_ID, MODULE, ACTION,
                           LOG_EXCEPTION, STACK_TRACE,
                           CREATED_AT,
                           CREATED_BY
                    FROM sys_logs 
                    WHERE REQUEST_ID = :RequestId
                    ORDER BY CREATED_AT DESC";

                var parameters = new DynamicParameters();
                parameters.Add("RequestId", requestId, DbType.String);
                return await _databaseExecutor.ExecuteQueryMultipleAsync<LogEntry>(query, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting logs by request ID {requestId}: {ex.Message}");
                return [];
            }
        }

    }
}