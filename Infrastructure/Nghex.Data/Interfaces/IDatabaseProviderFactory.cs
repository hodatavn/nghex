namespace Nghex.Data
{
    /// <summary>
    /// Factory for creating database providers
    /// </summary>
    public interface IDatabaseProviderFactory
    {
        /// <summary>
        /// Get database provider by name
        /// </summary>
        IDatabaseProvider GetProvider(string providerName);

        IEnumerable<IDatabaseProvider> GetAllProviders();

        bool IsProviderSupported(string providerName);
    }
}