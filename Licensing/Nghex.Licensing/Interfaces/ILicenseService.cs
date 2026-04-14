using Nghex.Licensing.Models;

namespace Nghex.Licensing.Interfaces
{
    /// <summary>
    /// License service interface
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Load license from encrypted file
        /// </summary>
        Task<LicenseInfo?> LoadLicenseAsync();

        /// <summary>
        /// Save license to encrypted file
        /// </summary>
        Task<bool> SaveLicenseAsync(LicenseInfo license);

        /// <summary>
        /// Validate current license
        /// </summary>
        Task<LicenseValidationResult> ValidateLicenseAsync();

        /// <summary>
        /// Activate license with license key (backward compatibility)
        /// </summary>
        Task<bool> ActivateLicenseAsync(string licenseKey, string organizationName);

        /// <summary>
        /// Activate license with license key and location information
        /// </summary>
        Task<bool> ActivateLicenseAsync(
            string licenseKey,
            string organizationName,
            string deploymentId
        );

        /// <summary>
        /// Check if feature is enabled in license
        /// </summary>
        bool IsFeatureEnabled(string feature);

        /// <summary>
        /// Check if license is valid
        /// </summary>
        bool IsLicenseValid();

        /// <summary>
        /// Create trial license
        /// </summary>
        /// <param name="trialDays">Number of trial days. Use 0 to use default from licenseSetting.json</param>
        Task<LicenseInfo> CreateTrialLicenseAsync(int trialDays = 0);
    }
}

