namespace Nghex.Data.Setup
{
    /// <inheritdoc cref="ISystemInitializationState"/>
    public sealed class SystemInitializationState : ISystemInitializationState
    {
        private int _initialized;

        public bool IsInitialized => Volatile.Read(ref _initialized) == 1;

        public void MarkInitialized() => Interlocked.Exchange(ref _initialized, 1);

        public void SetInitialized(bool initialized) => Interlocked.Exchange(ref _initialized, initialized ? 1 : 0);
    }
}
