using System.Data;

namespace Nghex.Data.Abstractions
{
    public interface IDatabaseExecutorHelper
    {
        Task<IDataReader> ExecuteReaderAsync(IDbCommand command);
        Task<int> ExecuteCommandAsync(IDbCommand command);
        Task<object?> ExecuteScalarAsync(IDbCommand command);
        Task OpenConnectionAsync(IDbConnection connection);
    }
}
