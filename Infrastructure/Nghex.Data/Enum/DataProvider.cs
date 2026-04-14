namespace Nghex.Data.Enum
{
    /// <summary>
    /// Enum for supported data providers
    /// </summary>
    public enum DataProvider
    {
        /// <summary>
        /// Oracle Database
        /// </summary>
        Oracle,
        /// <summary>
        /// Microsoft SQL Server
        /// </summary>
        SqlServer,
        /// <summary>
        /// Microsoft SQL Server (Alias)
        /// </summary>
        MSSQL,
        /// <summary>
        /// Unknown is unsupported provider
        /// </summary>
        Unknown
    }

    public static class DataProviderExtensions
    {
        /// <summary>
        /// Get provider name as string
        /// </summary>
        public static string GetProviderName(this DataProvider dataProvider)
        {
            return dataProvider switch
            {
                DataProvider.Oracle => "Oracle",
                DataProvider.SqlServer or DataProvider.MSSQL => "SqlServer",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Get DataProvider enum from string
        /// </summary>
        public static DataProvider ProviderString(this string providerName)
        {
            return providerName.ToLower() switch
            {
                "oracle" => DataProvider.Oracle,
                "sqlserver" or "mssql" => DataProvider.SqlServer,
                _ => DataProvider.Unknown
            };
        }
    }
}