namespace Nghex.Data.Abstractions
{
    public interface IDatabaseProviderFactory
    {
        IDatabaseProvider GetProvider(string providerName);
        IEnumerable<IDatabaseProvider> GetAllProviders();
        bool IsProviderSupported(string providerName);
    }
}
