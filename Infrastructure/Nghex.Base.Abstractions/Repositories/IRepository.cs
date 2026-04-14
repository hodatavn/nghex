using Nghex.Base.Entities;

namespace Nghex.Base.Repositories
{
    /// <summary>
    /// Generic repository contract for <see cref="BaseEntity"/>-derived types.
    /// </summary>
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(long id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<long> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(long id, string deletedBy);
    }
}
