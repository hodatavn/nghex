namespace Nghex.Logging.Interfaces
{
    /// <summary>
    /// Base repository interface cho logging
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface ILogRepository<T> where T : class
    {
        /// <summary>
        /// Thêm entity mới
        /// </summary>
        Task<long> AddAsync(T entity);

        /// <summary>
        /// Lấy entity theo ID
        /// </summary>
        Task<T?> GetByIdAsync(long id);

        /// <summary>
        /// Cập nhật entity
        /// </summary>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// Xóa entity
        /// </summary>
        Task<bool> DeleteAsync(long id);
    }
}
