using System.Data;
using Nghex.Data.Enum;

namespace Nghex.Data.Factory.Interfaces
{
    /// <summary>
    /// Factory for creating database executor helpers
    /// </summary>
    public interface IDatabaseExecutorHelperFactory
    {
        /// <summary>
        /// Get executor helper for specific provider
        /// </summary>
        IDatabaseExecutorHelper GetHelper(DataProvider dataProvider);

        /// <summary>
        /// Get executor helper based on connection type
        /// </summary>
        IDatabaseExecutorHelper GetHelper(IDbConnection connection);
    }
}
