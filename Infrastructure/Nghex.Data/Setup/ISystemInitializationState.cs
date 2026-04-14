namespace Nghex.Data.Setup
{
    /// <summary>
    /// In-memory flag: whether the database schema has been initialized (setup completed).
    /// Used with setup/login flows — e.g. allow setup user login only while not initialized, block otherwise.
    /// Populated at startup via <see cref="IDatabaseSetupService.IsInitializedAsync"/> and after successful <see cref="IDatabaseSetupService.SetupDatabaseAsync"/>.
    /// </summary>
    public interface ISystemInitializationState
    {
        bool IsInitialized { get; }
        void MarkInitialized();
        void SetInitialized(bool initialized);
    }
}
