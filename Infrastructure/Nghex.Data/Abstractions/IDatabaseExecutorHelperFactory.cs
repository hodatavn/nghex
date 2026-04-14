using System.Data;

namespace Nghex.Data.Abstractions
{
    public interface IDatabaseExecutorHelperFactory
    {
        IDatabaseExecutorHelper GetHelper(IDbConnection connection);
    }
}
