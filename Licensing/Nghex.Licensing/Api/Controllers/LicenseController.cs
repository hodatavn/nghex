using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nghex.Licensing.Services;
using Nghex.Licensing.Middleware;
using Nghex.Licensing.Api.Models;
using Nghex.Licensing.Interfaces;

namespace Nghex.Licensing.Api.Controllers
{
    /// <summary>
    /// License management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController(
        ILicenseService licenseService,
        LicenseSettingsService settingsService) : ControllerBase
    {
        private readonly ILicenseService _licenseService = licenseService;
        private readonly LicenseSettingsService _settingsService = settingsService;

        /// <summary>
        /// Activate license with license key - Based on Who, Where, When, What
        /// </summary>
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicenseKey))
                return BadRequest(new { message = "License key is required" });

            if (string.IsNullOrWhiteSpace(request.OrganizationName))
                return BadRequest(new { message = "Organization name is required" });

            var success = await _licenseService.ActivateLicenseAsync(
                request.LicenseKey,
                request.OrganizationName
            );

            if (!success)
                return BadRequest(new { message = "Invalid license key" });

            // Invalidate middleware cache to force revalidation on next request
            LicenseValidationMiddleware.InvalidateCache();
            
            return Ok(new 
            { 
                message = "License activated successfully",
                organization = request.OrganizationName,
            });
        }

        /// <summary>
        /// Get license status
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            // Invalidate cache to get fresh status
            LicenseValidationMiddleware.InvalidateCache();
            
            var result = await _licenseService.ValidateLicenseAsync();
            return Ok(result);
        }

        /// <summary>
        /// Force revalidate license (clears cache and revalidates)
        /// </summary>
        [HttpPost("revalidate")]
        public async Task<IActionResult> Revalidate()
        {
            // Invalidate cache
            LicenseValidationMiddleware.InvalidateCache();
            
            // Revalidate
            var result = await _licenseService.ValidateLicenseAsync();
            return Ok(new
            {
                message = "License revalidated",
                validationResult = result
            });
        }

        /// <summary>
        /// Get license features
        /// </summary>
        [HttpGet("features")]
        public async Task<IActionResult> GetFeatures()
        {
            var license = await _licenseService.LoadLicenseAsync();
            var hasAllFeatures = license?.HasAllFeatures() ?? false;
            return Ok(new
            {
                features = license?.Features ?? [],
                hasAllFeatures = hasAllFeatures,
                licenseType = license?.LicenseType.ToString() ?? "None",
                maxUsers = license?.MaxUsers ?? 0,
                maxApiCallsPerDay = license?.MaxApiCallsPerDay ?? 0,
                organization = license?.OrganizationName ?? "N/A",
                serverIdentifier = license?.ServerIdentifier ?? "N/A",
            });
        }

        /// <summary>
        /// Create trial license (for testing/development)
        /// </summary>
        [HttpPost("create-trial")]
        public async Task<IActionResult> CreateTrial([FromBody] CreateTrialRequest? request = null)
        {
            // Load settings to get default trial days if not provided
            var settings = await _settingsService.LoadSettingsAsync();
            var trialDays = request?.TrialDays ?? 0; // 0 means use default from settings
            var license = await _licenseService.CreateTrialLicenseAsync(trialDays);
            
            // Invalidate middleware cache to force revalidation on next request
            LicenseValidationMiddleware.InvalidateCache();
            
            return Ok(new
            {
                message = "Trial license created successfully",
                licenseKey = license.LicenseKey,
                expiryDate = license.ExpiryDate,
                daysRemaining = (license.ExpiryDate - DateTime.UtcNow).Days
            });
        }

        /// <summary>
        /// Helper method to get current location
        /// </summary>
        private string GetCurrentLocation()
        {
            // Option 1: From environment variable
            var location = Environment.GetEnvironmentVariable("LICENSE_LOCATION");
            if (!string.IsNullOrEmpty(location))
                return location;

            // Option 2: Default to machine name
            return Environment.MachineName;
        }
    }
}

