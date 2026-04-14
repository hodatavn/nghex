using Nghex.Logging.Models;
using OurLogLevel = Nghex.Core.Enum.LogLevel;

namespace Nghex.Logging.Interfaces
{
    /// <summary>
    /// Query interface for DB-backed log storage.
    /// Only available when AddNghexDatabaseLogging() is called.
    /// Use ILogQueryService? (nullable injection) when querying logs.
    /// </summary>
    public interface ILogQueryService
    {
        Task<IEnumerable<LogEntry>> GetLogsByLevelAsync(OurLogLevel level, int offset = 0, int limit = 100);
        Task<IEnumerable<LogEntry>> GetLogsByUserAsync(string username, int offset = 0, int limit = 100);
        Task<IEnumerable<LogEntry>> GetLogsByModuleAsync(string module, int offset = 0, int limit = 100);
        Task<IEnumerable<LogEntry>> GetLogsByDateRangeAsync(DateTime fromDate, DateTime toDate, int offset = 0, int limit = 100);
        Task<IEnumerable<LogEntry>> SearchLogsAsync(string keyword, int offset = 0, int limit = 100);
        Task<int> CleanupOldLogsAsync(int daysToKeep = 30);
        Task<bool> DeleteLogAsync(long id);
        Task<long> CountLogsByLevelAsync(OurLogLevel level);
        Task<long> CountLogsByUserAsync(string username);
        Task<long> CountLogsByModuleAsync(string module);
        Task<long> CountLogsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<long> CountSearchLogsAsync(string keyword);
    }
}
