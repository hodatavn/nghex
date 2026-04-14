using System.Data;

namespace Nghex.Data.Abstractions
{
    public interface IDatabaseConnection
    {
        IDbConnection CreateConnection();
        string GetConnectionString();
        Task<bool> TestConnectionAsync();
        string GetProviderName();
        bool IsProviderSupported();
    }
}
