using System.Data;

namespace Nghex.Data.Abstractions
{
    public interface IDatabaseProvider
    {
        IDbConnection CreateConnection(string connectionString);
        Task OpenConnectionAsync(IDbConnection connection);
        Task ExecuteCommandAsync(IDbCommand command);
        Task<bool> TestConnectionAsync(string connectionString);
        string GetScriptFileName();
        List<string> SplitSqlScript(string sqlScript);
    }
}
