using System.Data;
using Nghex.Data.Enum;

namespace Nghex.Data
{
    /// <summary>
    /// Interface for database provider-specific operations
    /// </summary>
    public interface IDatabaseProvider : Nghex.Data.Abstractions.IDatabaseProvider
    {
        /// <summary>
        /// Provider name (Oracle, SqlServer, MySql, PostgreSQL, etc.)
        /// </summary>
        DataProvider ProviderName { get; }
    }
}


