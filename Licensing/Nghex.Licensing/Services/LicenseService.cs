using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Nghex.Licensing.Models;
using Nghex.Licensing.Interfaces;
using Nghex.Utilities;
using Nghex.Core.Logging;
using Nghex.Core.Setting;
using Nghex.Licensing.Enum;

namespace Nghex.Licensing.Services
{
    /// <summary>
    /// License service implementation with encrypted file storage
    /// </summary>
    public class LicenseService : ILicenseService
    {
        private readonly Cryptography _cryptography;
        private readonly ILogging? _loggingService;
        private readonly IDeploymentIdService _deploymentIdService;
        private readonly LicenseSettingsService _settingsService;
        private readonly IConfiguration? _configuration;
        private readonly ILicenseKeyGeneratorService _keyGenerator;
        // private readonly string _backupFilePath;
        private LicenseFile _licenseFile;
        private LicenseInfo? _cachedLicense;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private const int CacheDurationMinutes = 5;
        private LicenseSettings? _cachedSettings;
        // Cache validation result để tránh tính toán lại mỗi lần
        // License được cấp theo năm, không hết hạn theo giờ, nên cache 1 ngày là hợp lý
        private LicenseValidationResult? _cachedValidationResult;
        private DateTime _validationCacheExpiry = DateTime.MinValue;
        private const int ValidationCacheDurationDays = 1; // 1 ngày
        public LicenseService(
            IDeploymentIdService deploymentIdService,
            Cryptography cryptography,
            LicenseSettingsService settingsService,
            IConfiguration configuration,
            ILogging? loggingService = null,
            ILicenseKeyGeneratorService? keyGenerator = null)
        {
            _deploymentIdService = deploymentIdService;
            _cryptography = cryptography;
            _settingsService = settingsService;
            _configuration = configuration;
            _loggingService = loggingService;
            _licenseFile = AppSettings.LicenseFile;

            // Initialize license key generator
            _keyGenerator = keyGenerator ?? new LicenseKeyGeneratorService(configuration);
        }

        private async Task<LicenseSettings> LoadSettingsAsync()
        {
            _licenseFile = AppSettings.LicenseFile;
            if (_cachedSettings != null)
                return _cachedSettings;
            
            _cachedSettings = await _settingsService.LoadSettingsAsync();
            return _cachedSettings;
        }

        public async Task<LicenseInfo?> LoadLicenseAsync()
        {
            await LoadSettingsAsync();

            // Check cache first
            if (IsCacheValid())
                return _cachedLicense;

            try
            {
                if (!File.Exists(_licenseFile.Key))
                    return null;

                var license = await LoadLicenseFromFileAsync(_licenseFile.Key);
                if (license != null)
                    UpdateCache(license);
                return license;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Failed to load license", ex, "LicenseService.LoadLicenseAsync");
                return null;
            }
        }

        /// <summary>
        /// Check if license cache is still valid
        /// </summary>
        private bool IsCacheValid()
        {
            return _cachedLicense != null && DateTime.UtcNow < _cacheExpiry;
        }

        /// <summary>
        /// Check if validation result cache is still valid
        /// </summary>
        private bool IsValidationCacheValid()
        {
            return _cachedValidationResult != null && DateTime.UtcNow < _validationCacheExpiry;
        }

        /// <summary>
        /// Load license from encrypted file
        /// </summary>
        private async Task<LicenseInfo?> LoadLicenseFromFileAsync(string licenseFilePath)
        {
            var rawContent = await File.ReadAllTextAsync(licenseFilePath);
            // Remove BOM, whitespace, and newlines that might have been added during file operations
            var encryptedContent = rawContent.Trim().TrimStart('\uFEFF', '\u200B');
            
            string decryptedJson;
            try
            {
                decryptedJson = _cryptography.Decrypt(encryptedContent);
            }
            catch (Exception decryptEx)
            {
                // Log diagnostic information to help identify the issue
                var contentPreview = encryptedContent.Length > 50 
                    ? encryptedContent.Substring(0, 50) + "..." 
                    : encryptedContent;
                var isValidBase64 = IsValidBase64(encryptedContent);
                var hasHiddenChars = rawContent.Length != encryptedContent.Length;
                var keyFingerprint = _cryptography.GetKeyFingerprint();
                var keyVerified = _cryptography.VerifyKey();
                
                // Get cipher info for debugging
                var cipherBytes = Convert.FromBase64String(encryptedContent);
                var cipherLength = cipherBytes.Length - 16; // minus IV
                var isValidCipherLength = cipherLength > 0 && cipherLength % 16 == 0;
                
                await LogErrorAsync(
                    $"Failed to decrypt license file. " +
                    $"KeyFingerprint: {keyFingerprint}, KeyVerified: {keyVerified}, " +
                    $"Raw size: {rawContent.Length}, Trimmed size: {encryptedContent.Length}, " +
                    $"Cipher length: {cipherLength}, Valid cipher length (multiple of 16): {isValidCipherLength}, " +
                    $"Had hidden chars: {hasHiddenChars}, Valid Base64: {isValidBase64}, " +
                    $"Content preview: [{contentPreview}]. " +
                    $"SOLUTION: If KeyVerified=True but decrypt fails, the license file was encrypted with a DIFFERENT key. " +
                    $"Delete the license file and re-activate the license.",
                    decryptEx,
                    "LicenseService.LoadLicenseFromFileAsync");
                return null;
            }
            
            var license = JsonSerializer.Deserialize<LicenseInfo>(decryptedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (license == null)
            {
                await LogErrorAsync(
                    "License file exists but deserialization returned null. License file might be corrupted.",
                    null,
                    "LicenseService.LoadLicenseFromFileAsync");
                return null;
            }

            return license;
        }

        /// <summary>
        /// Update license cache
        /// </summary>
        private void UpdateCache(LicenseInfo license)
        {
            _cachedLicense = license;
            _cacheExpiry = DateTime.UtcNow.AddMinutes(CacheDurationMinutes);
        }

        /// <summary>
        /// Update validation result cache
        /// </summary>
        private void UpdateValidationCache(LicenseValidationResult result)
        {
            _cachedValidationResult = result;
            _validationCacheExpiry = DateTime.UtcNow.AddDays(ValidationCacheDurationDays);
        }

        /// <summary>
        /// Invalidate validation result cache (call when license is changed)
        /// </summary>
        private void InvalidateValidationCache()
        {
            _cachedValidationResult = null;
            _validationCacheExpiry = DateTime.MinValue;
        }

        public async Task<bool> SaveLicenseAsync(LicenseInfo license)
        {
            await LoadSettingsAsync();

            try
            {
                var licenseFilePath = _licenseFile.Key;
                EnsureDirectoryExists(licenseFilePath);
                
                var encryptedContent = EncryptLicense(license);
                await WriteLicenseFileAtomicallyAsync(licenseFilePath, encryptedContent);
                
                UpdateCache(license);
                // Invalidate validation cache khi license được thay đổi
                InvalidateValidationCache();
                
                var keyFingerprint = _cryptography.GetKeyFingerprint();
                await LogInformationAsync(
                    $"License saved successfully: {license.LicenseType} license for {license.OrganizationName} at {license.ServerIdentifier}. " +
                    $"KeyFingerprint: {keyFingerprint} (save this to compare with decrypt errors)",
                    "LicenseService.SaveLicenseAsync");

                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Failed to save license", ex, "LicenseService.SaveLicenseAsync");
                return false;
            }
        }

        /// <summary>
        /// Ensure directory exists for license file
        /// </summary>
        private static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Check if string is valid Base64
        /// </summary>
        private static bool IsValidBase64(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            
            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypt license to JSON string
        /// </summary>
        private string EncryptLicense(LicenseInfo license)
        {
            var json = JsonSerializer.Serialize(license, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            return _cryptography.Encrypt(json);
        }

        /// <summary>
        /// Write license file atomically (write to temp file, then rename)
        /// Uses UTF-8 without BOM to ensure consistent reading across platforms
        /// </summary>
        private static async Task WriteLicenseFileAtomicallyAsync(string licenseFilePath, string encryptedContent)
        {
            var tempFilePath = licenseFilePath + ".tmp";
            // Use UTF-8 without BOM to avoid decryption issues
            await File.WriteAllTextAsync(tempFilePath, encryptedContent, new System.Text.UTF8Encoding(false));
            File.Move(tempFilePath, licenseFilePath, overwrite: true);
        }

        public async Task<LicenseValidationResult> ValidateLicenseAsync()
        {
            // Check validation cache first - tránh tính toán lại mỗi lần
            // License được cấp theo năm, không hết hạn theo giờ, nên cache 1 ngày là hợp lý
            if (IsValidationCacheValid())
            {
                return _cachedValidationResult!;
            }

            var license = await LoadLicenseAsync();

            if (license == null)
            {
                var result = new LicenseValidationResult
                {
                    IsValid = false,
                    Message = "License not found. Please activate your license."
                };
                UpdateValidationCache(result);
                return result;
            }

            // Check if license is active
            if (!license.IsActive)
            {
                var result = new LicenseValidationResult
                {
                    IsValid = false,
                    Message = "License is deactivated.",
                    License = license
                };
                UpdateValidationCache(result);
                return result;
            }

            // Load settings
            var settings = await LoadSettingsAsync();

            // Location binding validation removed - LicenseInfo no longer has Location property
            // Location validation is now handled at activation time via DeploymentId

            var validationInfo = CalculateValidationInfo(license, settings);
            
            LicenseValidationResult validationResult;
            if (validationInfo.IsInGracePeriod)
            {
                await UpdateLicenseValidationAsync(license);
                validationResult = CreateGracePeriodResult(license, validationInfo);
            }
            else if (validationInfo.IsExpired)
            {
                validationResult = CreateExpiredResult(license);
            }
            else
            {
                await UpdateLicenseValidationAsync(license);
                validationResult = CreateValidResult(license, validationInfo);
            }

            // Cache validation result để tránh tính toán lại
            UpdateValidationCache(validationResult);
            return validationResult;
        }

        /// <summary>
        /// Calculate validation information (expiry, grace period, warnings)
        /// </summary>
        private static ValidationInfo CalculateValidationInfo(LicenseInfo license, LicenseSettings settings)
        {
            var now = DateTime.UtcNow;
            var daysRemaining = (license.ExpiryDate - now).Days;
            var gracePeriodEndDate = license.ExpiryDate.AddDays(settings.GracePeriodDays);
            var isExpired = now > license.ExpiryDate;
            var isInGracePeriod = isExpired && now <= gracePeriodEndDate;
            var gracePeriodDaysRemaining = isInGracePeriod ? (gracePeriodEndDate - now).Days : 0;
            var shouldShowWarning = !isExpired && daysRemaining <= settings.WarningDaysBeforeExpiry;
            var warningMessage = shouldShowWarning
                ? $"License will expire in {daysRemaining} day(s). Please renew your license."
                : null;

            return new ValidationInfo
            {
                DaysRemaining = daysRemaining,
                IsExpired = isExpired,
                IsInGracePeriod = isInGracePeriod,
                GracePeriodDaysRemaining = gracePeriodDaysRemaining,
                ShouldShowWarning = shouldShowWarning,
                WarningMessage = warningMessage
            };
        }

        /// <summary>
        /// Update license validation timestamp and count
        /// Only saves to file periodically to avoid excessive I/O operations
        /// </summary>
        private async Task UpdateLicenseValidationAsync(LicenseInfo license)
        {
            license.LastValidated = DateTime.UtcNow;
            license.ValidationCount++;
            
            // Chỉ save định kỳ để tránh I/O operations quá nhiều
            // Save mỗi 100 lần validate (giảm từ mỗi lần validate xuống còn 1/100)
            var shouldSave = license.ValidationCount % 100 == 0;
            
            if (shouldSave)
            {
                await SaveLicenseAsync(license);
            }
            else
            {
                // Chỉ update cache, không save file
                // LastValidated và ValidationCount sẽ được lưu vào file khi save định kỳ
                UpdateCache(license);
            }
        }

        /// <summary>
        /// Create validation result for grace period
        /// </summary>
        private static LicenseValidationResult CreateGracePeriodResult(LicenseInfo license, ValidationInfo info)
        {
            return new LicenseValidationResult
            {
                IsValid = true,
                Message = $"License expired on {license.ExpiryDate:yyyy-MM-dd}. Grace period active for {info.GracePeriodDaysRemaining} more day(s).",
                License = license,
                DaysRemaining = 0,
                IsInGracePeriod = true,
                GracePeriodDaysRemaining = info.GracePeriodDaysRemaining,
                ShouldShowWarning = true,
                WarningMessage = $"License expired. Grace period ends in {info.GracePeriodDaysRemaining} day(s)."
            };
        }

        /// <summary>
        /// Create validation result for expired license
        /// </summary>
        private static LicenseValidationResult CreateExpiredResult(LicenseInfo license)
        {
            return new LicenseValidationResult
            {
                IsValid = false,
                Message = $"License expired on {license.ExpiryDate:yyyy-MM-dd}. Grace period ended.",
                License = license,
                DaysRemaining = 0,
                IsInGracePeriod = false,
                GracePeriodDaysRemaining = 0
            };
        }

        /// <summary>
        /// Create validation result for valid license
        /// </summary>
        private static LicenseValidationResult CreateValidResult(LicenseInfo license, ValidationInfo info)
        {
            return new LicenseValidationResult
            {
                IsValid = true,
                Message = "License is valid",
                License = license,
                DaysRemaining = info.DaysRemaining,
                IsInGracePeriod = false,
                GracePeriodDaysRemaining = 0,
                ShouldShowWarning = info.ShouldShowWarning,
                WarningMessage = info.WarningMessage
            };
        }

        /// <summary>
        /// Validation information helper class
        /// </summary>
        private class ValidationInfo
        {
            public int DaysRemaining { get; set; }
            public bool IsExpired { get; set; }
            public bool IsInGracePeriod { get; set; }
            public int GracePeriodDaysRemaining { get; set; }
            public bool ShouldShowWarning { get; set; }
            public string? WarningMessage { get; set; }
        }

        
        public async Task<bool> ActivateLicenseAsync(
            string licenseKey,
            string organizationName)
        {
            var deploymentId = _deploymentIdService.GetOrCreateDeploymentId();
            return await ActivateLicenseAsync(licenseKey, organizationName, deploymentId);
        }

        public async Task<bool> ActivateLicenseAsync(
            string licenseKey,
            string organizationName,
            string deploymentId)
        {
            try
            {
                var expiryDate = ExtractExpiryDateFromKey(licenseKey, organizationName, deploymentId);
                if (!expiryDate.HasValue)
                {
                    await LogActivationFailureAsync(licenseKey, organizationName, deploymentId);
                    return false;
                }

                var licenseInfo = await ParseLicenseKeyAsync(licenseKey);
                if (licenseInfo == null)
                    return false;

                SetupActivatedLicense(licenseInfo, organizationName, expiryDate.Value);
                return await SaveLicenseAsync(licenseInfo);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Failed to activate license", ex, "LicenseService.ActivateLicenseAsync");
                return false;
            }
        }

        /// <summary>
        /// Extract expiry date from license key (validates key in the process)
        /// </summary>
        private DateTime? ExtractExpiryDateFromKey(string licenseKey, string organizationName, string deploymentId)
        {
            var generationKey = _configuration?["License:GenerationKey"];
            return _keyGenerator.ExtractExpiryDateFromLicenseKey(licenseKey, organizationName, deploymentId, generationKey);
        }

        /// <summary>
        /// Log activation failure with detailed information
        /// </summary>
        private async Task LogActivationFailureAsync(string licenseKey, string organizationName, string deploymentId)
        {
            var generationKey = _configuration?["License:GenerationKey"];
            var keyPreview = licenseKey[..Math.Min(20, licenseKey.Length)];
            
            await LogWarningAsync(
                $"License activation failed: Could not extract expiry date. Possible causes: " +
                $"1) OrganizationName/DeploymentID mismatch, " +
                $"2) GenerationKey mismatch between generate and activate, " +
                $"3) Expiry date not in search range. " +
                $"Key: {keyPreview}..., " +
                $"Org: {organizationName}, " +
                $"DeploymentID: {deploymentId}, " +
                $"HasGenerationKey: {!string.IsNullOrEmpty(generationKey)}",
                "LicenseService.ActivateLicenseAsync");
        }

        /// <summary>
        /// Setup license information after successful activation
        /// </summary>
        private static void SetupActivatedLicense(LicenseInfo licenseInfo, string organizationName, DateTime expiryDate)
        {
            var now = DateTime.UtcNow;
            licenseInfo.OrganizationName = organizationName;
            licenseInfo.ServerIdentifier = Environment.MachineName;
            licenseInfo.IsActive = true;
            licenseInfo.IssuedDate = now;
            licenseInfo.ExpiryDate = expiryDate;
            licenseInfo.LastValidated = now;
        }

        public bool IsFeatureEnabled(string feature)
        {
            var license = LoadLicenseAsync().GetAwaiter().GetResult();
            if (license == null)
                return false;

            // If license has "All" feature, all features are enabled
            if (license.HasAllFeatures())
                return true;

            // Check if specific feature is in the list
            return license.Features.Contains(feature, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsLicenseValid()
        {
            var result = ValidateLicenseAsync().GetAwaiter().GetResult();
            return result.IsValid;
        }

        public async Task<LicenseInfo> CreateTrialLicenseAsync(int trialDays = 0)
        {
            var settings = await LoadSettingsAsync();
            var actualTrialDays = trialDays == 0 ? settings.TrialDays : trialDays;
            var location = GetCurrentLocation();
            
            await RestoreFromBackupAsync(settings);
            
            var (issuedDate, expiryDate) = await GetTrialDatesAsync(settings, location, actualTrialDays);
            var license = await CreateTrialLicenseInfoAsync(settings, issuedDate, expiryDate);
            
            await SaveLicenseAsync(license);
            return license;
        }

        /// <summary>
        /// Get trial dates (either from existing trial or create new)
        /// </summary>
        private async Task<(DateTime issuedDate, DateTime expiryDate)> GetTrialDatesAsync(
            LicenseSettings settings, 
            string location, 
            int trialDays)
        {
            var now = DateTime.UtcNow;
            
            if (settings.LocationTrialDates.TryGetValue(location, out var firstTrialDate))
            {
                return GetExistingTrialDates(firstTrialDate, settings.TrialDays, now);
            }
            
            return await CreateNewTrialDatesAsync(settings, location, trialDays, now);
        }

        /// <summary>
        /// Get dates for existing trial at location
        /// </summary>
        private static (DateTime issuedDate, DateTime expiryDate) GetExistingTrialDates(
            DateTime firstTrialDate, 
            int originalTrialDays, 
            DateTime now)
        {
            var originalExpiryDate = firstTrialDate.AddDays(originalTrialDays);
            var daysRemaining = (originalExpiryDate - now).Days;

            if (daysRemaining > 0)
            {
                return (firstTrialDate, originalExpiryDate);
            }

            throw new InvalidOperationException(
                $"Trial license for location was already created on {firstTrialDate:yyyy-MM-dd} and expired. " +
                "Please activate a paid license.");
        }

        /// <summary>
        /// Create new trial dates and save tracking
        /// </summary>
        private async Task<(DateTime issuedDate, DateTime expiryDate)> CreateNewTrialDatesAsync(
            LicenseSettings settings, 
            string location, 
            int trialDays, 
            DateTime now)
        {
            var issuedDate = now;
            var expiryDate = now.AddDays(trialDays);
            
            settings.LocationTrialDates[location] = issuedDate;
            await _settingsService.SaveSettingsAsync(settings);
            _cachedSettings = settings;
            
            await SaveTrialBackupAsync(location, issuedDate);
            
            return (issuedDate, expiryDate);
        }

        /// <summary>
        /// Create trial license info object
        /// </summary>
        private async Task<LicenseInfo> CreateTrialLicenseInfoAsync(
            LicenseSettings settings, 
            DateTime issuedDate, 
            DateTime expiryDate)
        {
            var now = DateTime.UtcNow;
            
            return new LicenseInfo
            {
                LicenseKey = await GenerateTrialLicenseKeyAsync(),
                OrganizationName = "Trial User",
                LicenseType = LicenseType.Trial,
                IssuedDate = issuedDate,
                ExpiryDate = expiryDate,
                MaxUsers = settings.Limits.Trial.MaxUsers,
                MaxApiCallsPerDay = settings.Limits.Trial.MaxApiCallsPerDay,
                Features = ["Basic"],
                IsActive = true,
                ServerIdentifier = Environment.MachineName,
                LastValidated = now,
                ValidationCount = 0
            };
        }

        /// <summary>
        /// Save trial backup to encrypted file (location-based)
        /// </summary>
        private async Task SaveTrialBackupAsync(string location, DateTime firstTrialDate)
        {
            try
            {
                var backupData = new
                {
                    Location = location, // Use location instead of hardware
                    FirstTrialDate = firstTrialDate,
                    CreatedAt = DateTime.UtcNow,
                    SecretKeyHash = GenerateSecretKeyHash(location, firstTrialDate)
                };

                var json = JsonSerializer.Serialize(backupData);
                var encrypted = _cryptography.Encrypt(json);

                var directory = Path.GetDirectoryName(_licenseFile.Backup);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(_licenseFile.Backup, encrypted);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Failed to save trial backup", ex, "LicenseService.SaveTrialBackupAsync");
            }
        }

        /// <summary>
        /// Load trial backup from encrypted file
        /// </summary>
        private async Task<TrialBackupData?> LoadTrialBackupAsync()
        {
            try
            {
                if (!File.Exists(_licenseFile.Backup))
                    return null;

                var encrypted = await File.ReadAllTextAsync(_licenseFile.Backup);
                var decrypted = _cryptography.Decrypt(encrypted);
                var backup = JsonSerializer.Deserialize<TrialBackupData>(decrypted, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (backup == null)
                    return null;

                // Verify location is present
                if (string.IsNullOrEmpty(backup.Location))
                    return null;

                // Verify secret key hash
                var expectedHash = GenerateSecretKeyHash(backup.Location, backup.FirstTrialDate);
                if (backup.SecretKeyHash != expectedHash)
                {
                    await LogWarningAsync(
                        "Trial backup secret key hash mismatch - possible tampering",
                        "LicenseService.LoadTrialBackupAsync");
                    return null;
                }

                return backup;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Failed to load trial backup", ex, "LicenseService.LoadTrialBackupAsync");
                return null;
            }
        }

        /// <summary>
        /// Restore trial dates from backup if settings were deleted
        /// </summary>
        private async Task RestoreFromBackupAsync(LicenseSettings settings)
        {
            try
            {
                var backup = await LoadTrialBackupAsync();
                if (backup == null)
                    return;

                // Restore to location-based tracking
                if (!string.IsNullOrEmpty(backup.Location) && !settings.LocationTrialDates.ContainsKey(backup.Location))
                {
                    settings.LocationTrialDates[backup.Location] = backup.FirstTrialDate;
                    await _settingsService.SaveSettingsAsync(settings);
                    _cachedSettings = settings; // Update cache

                    await LogInformationAsync(
                        $"Restored trial backup for location '{backup.Location}' from {backup.FirstTrialDate:yyyy-MM-dd}",
                        "LicenseService.RestoreFromBackupAsync");
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Failed to restore from backup", ex, "LicenseService.RestoreFromBackupAsync");
            }
        }

        /// <summary>
        /// Generate secret key hash for backup verification
        /// </summary>
        private string GenerateSecretKeyHash(string identifier, DateTime firstTrialDate)
        {
            const string dateFormat = "yyyy-MM-dd";
            var secretKey = _configuration?["Security:Key"] ?? "DefaultSecretKey";
            var data = $"{identifier}-{firstTrialDate.ToString(dateFormat)}-{secretKey}";
            var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Helper method to log errors
        /// </summary>
        private async Task LogErrorAsync(string message, Exception? ex, string source)
        {
            await (_loggingService?.LogErrorAsync(message, ex, source: source, module: "License") ?? Task.CompletedTask);
        }

        /// <summary>
        /// Helper method to log warnings
        /// </summary>
        private async Task LogWarningAsync(string message, string source)
        {
            await (_loggingService?.LogWarningAsync(message, source: source, module: "License") ?? Task.CompletedTask);
        }

        /// <summary>
        /// Helper method to log information
        /// </summary>
        private async Task LogInformationAsync(string message, string source)
        {
            await (_loggingService?.LogInformationAsync(message, source: source, module: "License") ?? Task.CompletedTask);
        }

        /// <summary>
        /// Trial backup data model (location-based)
        /// </summary>
        private class TrialBackupData
        {
            public string Location { get; set; } = string.Empty;
            public DateTime FirstTrialDate { get; set; }
            public DateTime CreatedAt { get; set; }
            public string SecretKeyHash { get; set; } = string.Empty;
        }

        private async Task<LicenseInfo?> ParseLicenseKeyAsync(string licenseKey)
        {
            // Parse license key using LicenseKeyGeneratorService
            // Note: ParseLicenseKey only extracts basic info (type, features)
            // Expiry, OrganizationName, Location are validated during activation via ValidateLicenseKeyForActivation
            var generationKey = _configuration?["License:GenerationKey"];
            var keyInfo = _keyGenerator.ParseLicenseKey(licenseKey, generationKey);

            if (keyInfo == null)
                return null;

            // Load settings to get limits
            var settings = await LoadSettingsAsync();
            var limits = GetLimitsForLicenseType(settings, keyInfo.LicenseType);

            return new LicenseInfo
            {
                LicenseKey = keyInfo.LicenseKey,
                LicenseType = keyInfo.LicenseType,
                ExpiryDate = keyInfo.ExpiryDate, // Placeholder, will be validated during activation
                Features = keyInfo.Features,
                MaxUsers = limits.MaxUsers,
                MaxApiCallsPerDay = limits.MaxApiCallsPerDay
            };
        }

        private LicenseTypeLimits GetLimitsForLicenseType(LicenseSettings settings, LicenseType licenseType)
        {
            return licenseType switch
            {
                LicenseType.Trial => settings.Limits.Trial,
                LicenseType.Standard => settings.Limits.Standard,
                LicenseType.Professional => settings.Limits.Professional,
                LicenseType.Enterprise => settings.Limits.Enterprise,
                _ => settings.Limits.Trial
            };
        }

        /// <summary>
        /// Generate trial license key
        /// </summary>
        private async Task<string> GenerateTrialLicenseKeyAsync()
        {
            var settings = await LoadSettingsAsync();
            var expiryDate = DateTime.UtcNow.AddDays(settings.TrialDays);
            var features = new List<string> { "Basic" };
            var generationKey = _configuration?["License:GenerationKey"];
            var deploymentId = _deploymentIdService.GetOrCreateDeploymentId();
            const string trialOrganizationName = "Trial User";
            
            return _keyGenerator.GenerateLicenseKey(
                LicenseType.Trial, 
                expiryDate, 
                features, 
                trialOrganizationName, 
                deploymentId, 
                generationKey);
        }

        /// <summary>
        /// Get current location from environment variable, config, or machine name
        /// Used for trial tracking (location-based anti-abuse)
        /// </summary>
        private string GetCurrentLocation()
        {
            // Option 1: From environment variable
            var location = Environment.GetEnvironmentVariable("LICENSE_LOCATION");
            if (!string.IsNullOrEmpty(location))
                return location;

            // Option 2: From configuration
            var configLocation = _configuration?["License:Location"];
            if (!string.IsNullOrEmpty(configLocation))
                return configLocation;

            // Option 3: Default to machine name (fallback)
            return Environment.MachineName;
        }
    }
}

    