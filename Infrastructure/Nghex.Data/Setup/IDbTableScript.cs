namespace Nghex.Data.Setup;

public interface IDbTableScript
{
    IEnumerable<string> GetTableStatements();
    IEnumerable<string> GetSeedStatements();
}
