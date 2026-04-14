namespace Nghex.Licensing.Interfaces
{
    /// <summary>
    /// Stable deployment identifier (file-backed), used for license binding and fingerprinting.
    /// </summary>
    public interface IDeploymentIdService
    {
        string GetOrCreateDeploymentId();
        string? GetDeploymentId();
        string InitializeDeploymentId();
        void ClearCache();
    }
}
