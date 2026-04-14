using Nghex.Licensing.Enum;
using Nghex.Licensing.Models;

namespace Nghex.Licensing.Interfaces
{
    /// <summary>
    /// Service for generating and validating license keys
    /// </summary>
    public interface ILicenseKeyGeneratorService
    {
        /// <summary>
        /// Generate license key with checksum (includes OrganizationName and Location for security)
        /// </summary>
        /// <param name="licenseType">License type</param>
        /// <param name="expiryDate">Expiry date</param>
        /// <param name="features">List of features</param>
        /// <param name="organizationName">Organization name (required for security)</param>
        /// <param name="deploymentId">Deployment ID (required for security)</param>
        /// <param name="generationKey">License generation key (optional, will use default if not provided)</param>
        /// <returns>License key string</returns>
        string GenerateLicenseKey(
            LicenseType licenseType,
            DateTime expiryDate,
            List<string> features,
            string organizationName,
            string deploymentId,
            string? generationKey = null);

        /// <summary>
        /// Validate license key
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <param name="generationKey">License generation key (optional, will use default if not provided)</param>
        /// <returns>True if valid</returns>
        bool ValidateLicenseKey(string licenseKey, string? generationKey = null);

        /// <summary>
        /// Parse license key and extract information
        /// </summary>
        /// <param name="licenseKey">License key to parse</param>
        /// <param name="generationKey">License generation key (optional, will use default if not provided)</param>
        /// <returns>License key information or null if invalid</returns>
        LicenseKeyInfo? ParseLicenseKey(string licenseKey, string? generationKey = null);

        /// <summary>
        /// Validate license key with OrganizationName and DeploymentId
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <param name="organizationName">Organization name to validate</param>
        /// <param name="deploymentId">Deployment ID to validate</param>
        /// <param name="generationKey">License generation key (optional, will use default if not provided)</param>
        /// <returns>True if valid and matches OrganizationName and DeploymentId</returns>
        bool ValidateLicenseKeyForActivation(
            string licenseKey,
            string organizationName,
            string deploymentId,
            string? generationKey = null);

        /// <summary>
        /// Validate license key with exact expiry date (for use after generation)
        /// </summary>
        /// <param name="licenseKey">License key to validate</param>
        /// <param name="expiryDate">Exact expiry date to validate</param>
        /// <param name="organizationName">Organization name to validate</param>
        /// <param name="deploymentId">Deployment ID to validate</param>
        /// <param name="generationKey">License generation key (optional, will use default if not provided)</param>
        /// <returns>True if valid and matches all parameters</returns>
        bool ValidateLicenseKeyWithExpiry(
            string licenseKey,
            DateTime expiryDate,
            string organizationName,
            string deploymentId,
            string? generationKey = null);

        /// <summary>
        /// Extract expiry date from license key by validating with organization name and deployment ID
        /// </summary>
        /// <param name="licenseKey">License key to extract expiry date from</param>
        /// <param name="organizationName">Organization name to validate</param>
        /// <param name="deploymentId">Deployment ID to validate</param>
        /// <param name="generationKey">License generation key (optional, will use default if not provided)</param>
        /// <returns>Expiry date if valid, null otherwise</returns>
        DateTime? ExtractExpiryDateFromLicenseKey(
            string licenseKey,
            string organizationName,
            string deploymentId,
            string? generationKey = null);
    }

}
