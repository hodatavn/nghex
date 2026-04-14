using System.Text;
using Microsoft.Extensions.Configuration;
using Nghex.Licensing.Enum;
using Nghex.Licensing.Models;
using Nghex.Licensing.Interfaces;
using Nghex.Utilities;

namespace Nghex.Licensing.Services
{
    /// <summary>
    /// Service for generating and validating license keys
    /// Format: TYPE-FEATURES-ENCRYPTED_EXPIRY-CHECKSUM (expiry date is encrypted, not in checksum)
    /// </summary>
    public class LicenseKeyGeneratorService : ILicenseKeyGeneratorService
    {
        private readonly string _defaultGenerationKey;
        private readonly Cryptography? _cryptography;

        public LicenseKeyGeneratorService(IConfiguration configuration)
        {
            _defaultGenerationKey = configuration["License:GenerationKey"]?.Trim() ?? throw new InvalidOperationException("GenerationKey is required in configuration");
            var encryptionKey = configuration["Security:Key"]?.Trim() ?? throw new InvalidOperationException("Security:Key is required in configuration");
            _cryptography = new Cryptography(encryptionKey);
        }

        public string GenerateLicenseKey(
            LicenseType licenseType,
            DateTime expiryDate,
            List<string> features,
            string organizationName,
            string deploymentId,
            string? generationKey = null)
        {
            ValidateGenerateLicenseKeyInputs(organizationName, deploymentId);

            var typeCode = GetTypeCode(licenseType);
            var featuresStr = FormatFeatures(features);
            var key = generationKey ?? _defaultGenerationKey;
            var encryptedExpiry = EncryptExpiryDate(expiryDate);
            var checksum = GenerateChecksum(typeCode, featuresStr, organizationName, deploymentId, key);
            
            return FormatLicenseKey(typeCode, featuresStr, encryptedExpiry, checksum);
        }

        /// <summary>
        /// Validate inputs for license key generation
        /// </summary>
        private static void ValidateGenerateLicenseKeyInputs(string organizationName, string deploymentId)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
                throw new ArgumentException("OrganizationName is required for license generation", nameof(organizationName));
            
            if (string.IsNullOrWhiteSpace(deploymentId))
                throw new ArgumentException("DeploymentId is required for license generation", nameof(deploymentId));
        }

        /// <summary>
        /// Format features list into comma-separated string
        /// </summary>
        private static string FormatFeatures(List<string> features)
        {
            return string.Join(",", features);
        }

        /// <summary>
        /// Format license key components into final license key string
        /// Format: TYPE-FEATURES-ENCRYPTED_EXPIRY-CHECKSUM
        /// </summary>
        private static string FormatLicenseKey(string typeCode, string featuresStr, string encryptedExpiry, string checksum)
        {
            return $"{typeCode}-{featuresStr}-{encryptedExpiry}-{checksum}";
        }

        public bool ValidateLicenseKey(string licenseKey, string? generationKey = null)
        {
            var info = ParseLicenseKey(licenseKey, generationKey);
            return info?.IsValid ?? false;
        }

        public bool ValidateLicenseKeyForActivation(
            string licenseKey,
            string organizationName,
            string deploymentId,
            string? generationKey = null)
        {
            return ExtractExpiryDateFromLicenseKey(licenseKey, organizationName, deploymentId, generationKey).HasValue;
        }

        public bool ValidateLicenseKeyWithExpiry(
            string licenseKey,
            DateTime expiryDate,
            string organizationName,
            string deploymentId,
            string? generationKey = null)
        {
            if (!ValidateInputs(organizationName, deploymentId))
                return false;

            try
            {
                var (typeCode, featuresStr, encryptedExpiry, providedChecksum) = ParseLicenseKeyParts(licenseKey);
                if (!ValidateLicenseKeyParts(typeCode, featuresStr, encryptedExpiry, providedChecksum))
                    return false;

                if (!ValidateExpiryDate(encryptedExpiry!, expiryDate))
                    return false;

                return ValidateChecksum(typeCode!, featuresStr!, organizationName, deploymentId, providedChecksum!, generationKey);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate organization name and deployment ID inputs
        /// </summary>
        private static bool ValidateInputs(string organizationName, string deploymentId)
        {
            return !string.IsNullOrWhiteSpace(organizationName) && !string.IsNullOrWhiteSpace(deploymentId);
        }

        /// <summary>
        /// Validate license key parts are not null and checksum is valid length
        /// </summary>
        private static bool ValidateLicenseKeyParts(string? typeCode, string? featuresStr, string? encryptedExpiry, string? providedChecksum)
        {
            return typeCode != null 
                && featuresStr != null 
                && encryptedExpiry != null 
                && providedChecksum != null 
                && providedChecksum.Length >= 32;
        }

        /// <summary>
        /// Validate expiry date matches the expected date
        /// </summary>
        private bool ValidateExpiryDate(string encryptedExpiry, DateTime expectedExpiryDate)
        {
            var decryptedExpiry = DecryptExpiryDate(encryptedExpiry);
            return decryptedExpiry.HasValue && decryptedExpiry.Value.Date == expectedExpiryDate.Date;
        }

        /// <summary>
        /// Validate checksum matches expected checksum
        /// </summary>
        private bool ValidateChecksum(string typeCode, string featuresStr, string organizationName, string deploymentId, string providedChecksum, string? generationKey)
        {
            var key = generationKey ?? _defaultGenerationKey;
            var expectedChecksum = GenerateChecksum(typeCode, featuresStr, organizationName, deploymentId, key);
            return providedChecksum == expectedChecksum;
        }

        public DateTime? ExtractExpiryDateFromLicenseKey(
            string licenseKey,
            string organizationName,
            string deploymentId,
            string? generationKey = null)
        {
            if (!ValidateInputs(organizationName, deploymentId))
                return null;

            try
            {
                var (typeCode, featuresStr, encryptedExpiry, providedChecksum) = ParseLicenseKeyParts(licenseKey);
                if (typeCode == null || featuresStr == null || providedChecksum == null || providedChecksum.Length < 32)
                    return null;

                var key = generationKey ?? _defaultGenerationKey;

                return encryptedExpiry != null
                    ? ExtractExpiryFromNewFormat(typeCode, featuresStr, encryptedExpiry, providedChecksum, organizationName, deploymentId, key)
                    : ExtractExpiryFromOldFormat(typeCode, featuresStr, providedChecksum, organizationName, deploymentId, key);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract expiry date from new format (with encrypted expiry)
        /// </summary>
        private DateTime? ExtractExpiryFromNewFormat(
            string typeCode,
            string featuresStr,
            string encryptedExpiry,
            string providedChecksum,
            string organizationName,
            string deploymentId,
            string key)
        {
            // Validate checksum first
            var expectedChecksum = GenerateChecksum(typeCode, featuresStr, organizationName, deploymentId, key);
            if (providedChecksum != expectedChecksum)
                return null;

            // Decrypt and return expiry date
            return DecryptExpiryDate(encryptedExpiry);
        }

        /// <summary>
        /// Extract expiry date from old format (backward compatibility - brute force)
        /// Searches up to 2 years ahead for matching checksum
        /// </summary>
        private DateTime? ExtractExpiryFromOldFormat(
            string typeCode,
            string featuresStr,
            string providedChecksum,
            string organizationName,
            string deploymentId,
            string key)
        {
            const int maxSearchYears = 2;
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddYears(maxSearchYears);
            
            for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                var expiryStr = currentDate.ToString("yyyyMMdd");
                var oldChecksum = GenerateOldChecksum(typeCode, expiryStr, featuresStr, organizationName, deploymentId, key);
                
                if (providedChecksum == oldChecksum)
                    return currentDate;
            }

            return null;
        }

        public LicenseKeyInfo? ParseLicenseKey(string licenseKey, string? generationKey = null)
        {
            try
            {
                var (typeCode, featuresStr, encryptedExpiry, providedChecksum) = ParseLicenseKeyParts(licenseKey);
                if (typeCode == null || featuresStr == null || providedChecksum == null || providedChecksum.Length < 32)
                    return null;

                var licenseType = GetLicenseType(typeCode);
                var features = ParseFeatures(featuresStr);
                var expiryDate = encryptedExpiry != null ? DecryptExpiryDate(encryptedExpiry) : null;

                return CreateLicenseKeyInfo(licenseKey, licenseType, features, providedChecksum, expiryDate);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parse features string into list
        /// </summary>
        private static List<string> ParseFeatures(string featuresStr)
        {
            return featuresStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        /// <summary>
        /// Create LicenseKeyInfo object
        /// </summary>
        private static LicenseKeyInfo CreateLicenseKeyInfo(
            string licenseKey,
            LicenseType licenseType,
            List<string> features,
            string checksum,
            DateTime? expiryDate)
        {
            return new LicenseKeyInfo
            {
                LicenseKey = licenseKey,
                LicenseType = licenseType,
                ExpiryDate = expiryDate ?? DateTime.UtcNow,
                Features = features,
                Checksum = checksum,
                IsValid = false, // Will be validated with ValidateLicenseKeyForActivation
                OrganizationName = null,
                Location = null
            };
        }

        /// <summary>
        /// Parse license key into its components
        /// Format: TYPE-FEATURES-ENCRYPTED_EXPIRY-CHECKSUM (new) or TYPE-FEATURES-CHECKSUM (old, backward compatibility)
        /// </summary>
        private (string? typeCode, string? featuresStr, string? encryptedExpiry, string? checksum) ParseLicenseKeyParts(string licenseKey)
        {
            var parts = licenseKey.Split('-');
            const int minParts = 3;
            const int minPartsForNewFormat = 4;
            
            if (parts.Length < minParts)
                return (null, null, null, null);

            var typeCode = parts[0];
            
            return parts.Length >= minPartsForNewFormat
                ? ParseNewFormat(parts, typeCode)
                : ParseOldFormat(parts, typeCode);
        }

        /// <summary>
        /// Parse new format license key: TYPE-FEATURES-ENCRYPTED_EXPIRY-CHECKSUM
        /// </summary>
        private static (string typeCode, string featuresStr, string encryptedExpiry, string checksum) ParseNewFormat(string[] parts, string typeCode)
        {
            var featuresParts = parts.Skip(1).Take(parts.Length - 3).ToList();
            var featuresStr = string.Join(",", featuresParts);
            var encryptedExpiry = parts[parts.Length - 2];
            var checksum = parts[parts.Length - 1];
            return (typeCode, featuresStr, encryptedExpiry, checksum);
        }

        /// <summary>
        /// Parse old format license key: TYPE-FEATURES-CHECKSUM
        /// </summary>
        private static (string typeCode, string featuresStr, string? encryptedExpiry, string checksum) ParseOldFormat(string[] parts, string typeCode)
        {
            var featuresParts = parts.Skip(1).Take(parts.Length - 2).ToList();
            var featuresStr = string.Join(",", featuresParts);
            var checksum = parts[parts.Length - 1];
            return (typeCode, featuresStr, null, checksum);
        }

        /// <summary>
        /// Encrypt expiry date using Cryptography service
        /// </summary>
        private string EncryptExpiryDate(DateTime expiryDate)
        {
            if (_cryptography == null)
                throw new InvalidOperationException(
                    "Encryption key is required for license generation. Please check your configuration.");

            const string dateFormat = "yyyyMMdd";
            var expiryStr = expiryDate.ToString(dateFormat);
            return _cryptography.Encrypt(expiryStr);
        }

        /// <summary>
        /// Decrypt expiry date using Cryptography service
        /// </summary>
        private DateTime? DecryptExpiryDate(string encryptedExpiry)
        {
            if (_cryptography == null)
                return null;

            try
            {
                const string dateFormat = "yyyyMMdd";
                var expiryStr = _cryptography.Decrypt(encryptedExpiry);
                
                return DateTime.TryParseExact(expiryStr, dateFormat, null, 
                    System.Globalization.DateTimeStyles.None, out var expiryDate) 
                    ? expiryDate 
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private string GetTypeCode(LicenseType licenseType)
        {
            return licenseType switch
            {
                LicenseType.Trial => "TRI",
                LicenseType.Standard => "STD",
                LicenseType.Professional => "PRO",
                LicenseType.Enterprise => "ENT",
                _ => "TRI"
            };
        }

        private LicenseType GetLicenseType(string typeCode)
        {
            return typeCode switch
            {
                "TRI" => LicenseType.Trial,
                "STD" => LicenseType.Standard,
                "PRO" => LicenseType.Professional,
                "ENT" => LicenseType.Enterprise,
                _ => LicenseType.Trial
            };
        }

        /// <summary>
        /// Generate checksum WITHOUT expiry date (expiry is encrypted separately)
        /// </summary>
        private string GenerateChecksum(string type, string features, string organizationName, string deploymentId, string generationKey)
        {
            return GenerateChecksumInternal($"{type}-{features}-{organizationName}-{deploymentId}-{generationKey}");
        }

        /// <summary>
        /// Generate old format checksum WITH expiry date (for backward compatibility)
        /// </summary>
        private string GenerateOldChecksum(string type, string expiry, string features, string organizationName, string deploymentId, string generationKey)
        {
            return GenerateChecksumInternal($"{type}-{expiry}-{features}-{organizationName}-{deploymentId}-{generationKey}");
        }

        /// <summary>
        /// Internal method to generate checksum from data string
        /// </summary>
        private static string GenerateChecksumInternal(string data)
        {
            var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(data));
            var base64Hash = Convert.ToBase64String(hash).Replace("+", "").Replace("/", "").Replace("=", "");
            return base64Hash;
        }
    }
}
