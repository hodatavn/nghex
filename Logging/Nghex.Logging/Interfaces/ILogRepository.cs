using Nghex.Logging.Models;
using OurLogLevel = Nghex.Core.Enum.LogLevel;

namespace Nghex.Logging.Interfaces
{
    /// <summary>
    /// Interface cho Log Repository
    /// </summary>
    public interface ILogRepository
    {
        /// <summary>
        /// Thêm entity mới
        /// </summary>
        Task<long> AddAsync(LogEntry entity);

        /// <summary>
        /// Lấy entity theo ID
        /// </summary>
        Task<LogEntry?> GetByIdAsync(long id);

        /// <summary>
        /// Xóa entity
        /// </summary>
        Task<bool> DeleteAsync(long id);
        /// <summary>
        /// Lấy logs theo level
        /// </summary>
        Task<IEnumerable<LogEntry>> GetByLevelAsync(OurLogLevel level, int skip = 0, int take = 100);

        /// <summary>
        /// Get logs by user with offset and limit
        /// </summary>
        Task<IEnumerable<LogEntry>> GetByUserAsync(string username, int offset = 0, int limit = 100);

        /// <summary>
        /// Get logs by module with offset and limit
        /// </summary>
        Task<IEnumerable<LogEntry>> GetByModuleAsync(string module, int offset = 0, int limit = 100);

        /// <summary>
        /// Get logs by date range with offset and limit
        /// </summary>
        Task<IEnumerable<LogEntry>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int offset = 0, int limit = 100);

        /// <summary>
        /// Search logs by keyword with offset and limit
        /// </summary>
        Task<IEnumerable<LogEntry>> SearchAsync(string keyword, int offset = 0, int limit = 100);

        /// <summary>
        /// Xóa logs cũ hơn ngày chỉ định
        /// </summary>
        Task<int> DeleteOldLogsAsync(DateTime beforeDate);

        /// <summary>
        /// Đếm số lượng logs theo level
        /// </summary>
        Task<long> CountByLevelAsync(OurLogLevel level);

        /// <summary>
        /// Đếm số lượng logs theo username
        /// </summary>
        Task<long> CountByUserAsync(string username);

        /// <summary>
        /// Đếm số lượng logs theo module
        /// </summary>
        Task<long> CountByModuleAsync(string module);

        /// <summary>
        /// Đếm số lượng logs theo date range
        /// </summary>
        Task<long> CountByDateRangeAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Đếm số lượng logs theo keyword search
        /// </summary>
        Task<long> CountBySearchAsync(string keyword);

        /// <summary>
        /// Lấy logs theo request ID
        /// </summary>
        Task<IEnumerable<LogEntry>> GetByRequestIdAsync(string requestId);
    }
}
