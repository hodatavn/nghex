using Nghex.Licensing.Services;
using Nghex.Licensing.Models;
using Microsoft.AspNetCore.Http;
using Nghex.Licensing.Interfaces;

namespace Nghex.Licensing.Middleware
{
    /// <summary>
    /// Middleware to check license validation cache on each request
    /// License chỉ được kiểm tra 1 lần khi login (trong AccountService.AuthenticateAsync)
    /// Kết quả được cache trong LicenseService, middleware chỉ check cache, không validate lại
    /// </summary>
    public class LicenseValidationMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private static DateTime _lastValidation = DateTime.MinValue;
        private static LicenseValidationResult? _cachedValidationResult;
        // License được cấp theo năm, cache validation result trong 1 ngày
        // Khi activate license mới, InvalidateCache() được gọi tự động từ LicenseController
        private const int ValidationIntervalDays = 1; // 1 day

        /// <summary>
        /// Invalidate the license validation cache (call after license activation/creation)
        /// This is called automatically when:
        /// - License is activated via /api/license/activate
        /// - Trial license is created via /api/license/create-trial
        /// - License status is checked via /api/license/status
        /// - License is revalidated via /api/license/revalidate
        /// </summary>
        public static void InvalidateCache()
        {
            _lastValidation = DateTime.MinValue;
            _cachedValidationResult = null;
        }

        public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
        {
            // Skip validation for certain paths
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/swagger") || 
                path.Contains("/health") || 
                path.Contains("/license/activate") ||
                path.Contains("/license/status") ||
                path.Contains("/license/features") ||
                path.Contains("/license/create-trial") ||
                path.Contains("/auth/login"))
            {
                await _next(context);
                return;
            }

            // Chỉ check cache, không validate lại
            // License đã được validate khi login, kết quả được cache trong LicenseService
            bool shouldRevalidate = _lastValidation == DateTime.MinValue || 
                                    DateTime.UtcNow.Subtract(_lastValidation).TotalDays > ValidationIntervalDays;

            if (shouldRevalidate)
            {
                // Lấy kết quả từ cache của LicenseService (không validate lại)
                // LicenseService.ValidateLicenseAsync() sẽ sử dụng cache nếu còn hiệu lực
                var result = await licenseService.ValidateLicenseAsync();
                _cachedValidationResult = result;
                _lastValidation = DateTime.UtcNow;

                // Add warning headers if needed
                if (result.ShouldShowWarning && !string.IsNullOrEmpty(result.WarningMessage))
                {
                    context.Response.Headers.Append("X-License-Warning", result.WarningMessage);
                    context.Response.Headers.Append("X-License-Days-Remaining", result.DaysRemaining.ToString());
                }

                // Add grace period headers if in grace period
                if (result.IsInGracePeriod)
                {
                    context.Response.Headers.Append("X-License-Grace-Period", "true");
                    context.Response.Headers.Append("X-License-Grace-Period-Days-Remaining", result.GracePeriodDaysRemaining.ToString());
                }

                if (!result.IsValid)
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "License validation failed",
                        message = result.Message,
                        daysRemaining = result.DaysRemaining,
                        isInGracePeriod = result.IsInGracePeriod,
                        gracePeriodDaysRemaining = result.GracePeriodDaysRemaining
                    }));
                    return;
                }
            }
            else if (_cachedValidationResult != null && !_cachedValidationResult.IsValid)
            {
                // Use cached validation result with proper error message
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = "License validation failed",
                    message = _cachedValidationResult.Message,
                    daysRemaining = _cachedValidationResult.DaysRemaining,
                    isInGracePeriod = _cachedValidationResult.IsInGracePeriod,
                    gracePeriodDaysRemaining = _cachedValidationResult.GracePeriodDaysRemaining
                }));
                return;
            }

            await _next(context);
        }
    }
}
