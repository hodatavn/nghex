using Dapper;

namespace Nghex.Plugins.Abstractions.DataPolicy;

public static class AccessPolicyFilter
{
    /// <summary>
    /// Build SQL WHERE fragment for a policy type filter.
    /// Caller must provide the exact column name used in the query.
    /// Returns "1=0" when restricted but no codes allowed (blocks all rows).
    /// </summary>
    /// <param name="allowedCodes">Allowed codes from access policy cache.</param>
    /// <param name="columnAlias">Column name or alias in the query (e.g. "t.makp", "d.date_code").</param>
    public static (string condition, DynamicParameters? parameters) Build(
        IReadOnlyList<string> allowedCodes,
        string columnAlias)
    {
        if (allowedCodes.Count == 0)
            return ("1=0", null);

        // Use columnAlias as parameter name suffix to avoid collision when multiple filters applied
        var paramName = $"allowed_{columnAlias.Replace(".", "_")}";
        var p = new DynamicParameters();
        p.Add(paramName, allowedCodes);
        return ($"{columnAlias} IN :{paramName}", p);
    }
}
