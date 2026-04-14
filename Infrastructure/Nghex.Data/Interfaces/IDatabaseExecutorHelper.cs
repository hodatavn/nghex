using System.Data;
using Nghex.Data.Enum;

namespace Nghex.Data
{
    /// <summary>
    /// Interface for provider-specific database execution helpers
    /// </summary>
    public interface IDatabaseExecutorHelper : Nghex.Data.Abstractions.IDatabaseExecutorHelper
    {
        /// <summary>
        /// Provider name
        /// </summary>
        DataProvider ProviderName { get; }

        
    }
}
