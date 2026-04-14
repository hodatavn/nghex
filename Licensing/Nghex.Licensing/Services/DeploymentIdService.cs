using Nghex.Core.Setting;
using Nghex.Licensing.Interfaces;
using Nghex.Utilities;

namespace Nghex.Licensing.Services
{
    /// <summary>
    /// Manages DeploymentID in a deployment file (license / installation fingerprint).
    /// </summary>
    public class DeploymentIdService : IDeploymentIdService
    {
        private readonly string _deploymentFilePath;
        private string? _cachedDeploymentId;

        public DeploymentIdService()
        {
            _deploymentFilePath = Path.Combine(AppSettings.ApplicationDataPath, ".deployment.dat");
        }

        public string GetOrCreateDeploymentId()
        {
            if (!string.IsNullOrWhiteSpace(_cachedDeploymentId))
                return _cachedDeploymentId;

            try
            {
                var appDir = Path.GetDirectoryName(_deploymentFilePath);
                if (!string.IsNullOrEmpty(appDir) && !Directory.Exists(appDir))
                    Directory.CreateDirectory(appDir);

                if (File.Exists(_deploymentFilePath))
                {
                    var deploymentId = File.ReadAllText(_deploymentFilePath).Trim();
                    if (!string.IsNullOrWhiteSpace(deploymentId))
                    {
                        _cachedDeploymentId = deploymentId;
                        return deploymentId;
                    }
                }

                var newDeploymentId = SecretKeyGenerator.CreateRandomSecretKey(32);
                File.WriteAllText(_deploymentFilePath, newDeploymentId);
                _cachedDeploymentId = newDeploymentId;
                return newDeploymentId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to read/write deployment file: {ex.Message}");
                var tempId = SecretKeyGenerator.CreateRandomSecretKey(32);
                _cachedDeploymentId = tempId;
                return tempId;
            }
        }

        public string? GetDeploymentId()
        {
            if (!string.IsNullOrWhiteSpace(_cachedDeploymentId))
                return _cachedDeploymentId;

            try
            {
                if (!File.Exists(_deploymentFilePath))
                    return null;

                var deploymentId = File.ReadAllText(_deploymentFilePath).Trim();
                if (string.IsNullOrWhiteSpace(deploymentId))
                    return null;

                _cachedDeploymentId = deploymentId;
                return deploymentId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to read deployment file: {ex.Message}");
                return null;
            }
        }

        public string InitializeDeploymentId() => GetOrCreateDeploymentId();

        public void ClearCache() => _cachedDeploymentId = null;
    }
}
