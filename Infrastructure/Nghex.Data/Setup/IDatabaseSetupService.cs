namespace Nghex.Data.Setup
{
    /// <summary>
    /// Database bootstrap / schema setup (runs provider-specific SQL scripts).
    /// </summary>
    public interface IDatabaseSetupService
    {
        /// <summary>
        /// True when the expected base tables exist (e.g. SYS_ACCOUNTS for Oracle).
        /// </summary>
        Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Setup database by executing appropriate SQL script based on DataProvider.
        /// </summary>
        Task<DatabaseSetupResult> SetupDatabaseAsync();

        /// <summary>
        /// Get current DataProvider name from configuration.
        /// </summary>
        string GetDataProvider();
    }
}
