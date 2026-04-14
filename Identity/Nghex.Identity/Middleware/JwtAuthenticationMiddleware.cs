using Nghex.Logging.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Nghex.Identity.Services.Interfaces;
using Nghex.Identity.Models;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace Nghex.Identity.Middleware
{
    /// <summary>
    /// JWT Authentication Middleware - handles token validation and automatic refresh
    /// </summary>
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        
        // Prevent "refresh storm" when many parallel requests hit with the same refresh token
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _refreshLocks = new();
        
        // Short-lived cache so parallel requests can reuse newly issued tokens
        private static readonly ConcurrentDictionary<string, (JwtTokenResponse Response, DateTimeOffset CreatedAt)> _refreshResultCache = new();
        private static readonly TimeSpan RefreshResultCacheTtl = TimeSpan.FromSeconds(5);

        public JwtAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService, AuthCookieConfiguration cookieConfig)
        {
            try
            {
                var endpoint = context.GetEndpoint();
                
                // Skip if no endpoint or endpoint doesn't require authorization
                // Use IAuthorizeData to support both controller attributes and minimal API .RequireAuthorization()
                var authorizeData = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>();
                if (endpoint == null || authorizeData == null || authorizeData.Count == 0)
                {
                    await _next(context);
                    return;
                }
                
                // Allow anonymous requests
                // Use IAllowAnonymous to support both controller attributes and minimal API .AllowAnonymous()
                if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
                {
                    await _next(context);
                    return;
                }

                // If JwtBearer already authenticated, verify token is still valid in DB
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var authenticatedToken = ExtractTokenFromHeaderOrCookie(context, cookieConfig);
                    if (!string.IsNullOrEmpty(authenticatedToken))
                    {
                        // Check if token is expired first
                        var isExpired = IsTokenExpired(authenticatedToken);
                        
                        // If expired, try to refresh immediately
                        if (isExpired)
                        {
                            var refreshToken = ExtractRefreshTokenFromCookie(context, cookieConfig);
                            if (!string.IsNullOrWhiteSpace(refreshToken) && 
                                await TryRefreshTokenAsync(context, jwtService, cookieConfig, refreshToken))
                            {
                                return;
                            }
                            
                            await UnauthorizedResponse(context, "Token expired");
                            return;
                        }
                        
                        // If not expired, validate token in DB
                        var isValidInDb = await SafeValidateTokenAsync(jwtService, authenticatedToken);
                        
                        if (!isValidInDb)
                        {
                            // Token revoked in DB - try to refresh
                            var refreshToken = ExtractRefreshTokenFromCookie(context, cookieConfig);
                            if (!string.IsNullOrWhiteSpace(refreshToken) && 
                                await TryRefreshTokenAsync(context, jwtService, cookieConfig, refreshToken))
                            {
                                return;
                            }
                            
                            await UnauthorizedResponse(context, "Token expired or revoked");
                            return;
                        }
                    }
                    
                    await _next(context);
                    return;
                }

                // No authentication yet - validate token manually
                var token = ExtractTokenFromHeaderOrCookie(context, cookieConfig);
                
                if (!string.IsNullOrEmpty(token))
                {
                    // Check if token is expired first (before full validation)
                    var isExpired = IsTokenExpired(token);
                    
                    // If token is expired, try to refresh immediately
                    if (isExpired)
                    {
                        var refreshToken = ExtractRefreshTokenFromCookie(context, cookieConfig);
                        if (!string.IsNullOrWhiteSpace(refreshToken) && 
                            await TryRefreshTokenAsync(context, jwtService, cookieConfig, refreshToken))
                        {
                            return;
                        }
                    }
                    
                    // If not expired, validate token normally
                    var isValid = await SafeValidateTokenAsync(jwtService, token);
                    
                    if (isValid)
                    {
                        var principal = await jwtService.GetClaimsFromTokenAsync(token);
                        if (principal != null)
                        {
                            context.User = principal;
                            await _next(context);
                            return;
                        }
                    }
                    
                    // Token invalid (not expired but invalid for other reasons) - try to refresh
                    var refreshTokenForInvalid = ExtractRefreshTokenFromCookie(context, cookieConfig);
                    if (!string.IsNullOrWhiteSpace(refreshTokenForInvalid) && 
                        await TryRefreshTokenAsync(context, jwtService, cookieConfig, refreshTokenForInvalid))
                    {
                        return;
                    }
                    
                    await UnauthorizedResponse(context);
                    return;
                }
                
                // No access token - try to refresh if refresh token exists
                var refreshTokenOnly = ExtractRefreshTokenFromCookie(context, cookieConfig);
                if (!string.IsNullOrWhiteSpace(refreshTokenOnly) && 
                    await TryRefreshTokenAsync(context, jwtService, cookieConfig, refreshTokenOnly))
                {
                    return;
                }
                
                await UnauthorizedResponse(context, "Missing authentication token");
            }
            catch (Exception ex)
            {
                await LogErrorAsync(context, "Error in JWT authentication middleware", ex);
                await UnauthorizedResponse(context);
            }
        }

        /// <summary>
        /// Check if token is expired without full validation
        /// </summary>
        private static bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                // Check if token has expired
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    return true;
                }
                
                return false;
            }
            catch
            {
                // If we can't read the token, assume it's invalid (not just expired)
                return false;
            }
        }

        /// <summary>
        /// Safely validate token, returning false on any exception
        /// </summary>
        private static async Task<bool> SafeValidateTokenAsync(IJwtService jwtService, string token)
        {
            try
            {
                return await jwtService.ValidateTokenAsync(token);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Try to refresh token with single-flight pattern to prevent refresh storms
        /// </summary>
        private async Task<bool> TryRefreshTokenAsync(
            HttpContext context, 
            IJwtService jwtService, 
            AuthCookieConfiguration cookieConfig, 
            string refreshToken)
        {
            var ipAddress = GetClientIpAddress(context);
            var userAgent = GetUserAgent(context);
            var cacheKey = $"{refreshToken}::{ipAddress}::{userAgent}";

            var gate = _refreshLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            
            try
            {
                // Check if another request already refreshed this token
                if (TryUseCachedRefreshResult(context, jwtService, cookieConfig, cacheKey))
                {
                    await _next(context);
                    return true;
                }

                // Perform the actual refresh
                var tokenResponse = await jwtService.RefreshTokenAsync(refreshToken, ipAddress, userAgent);
                
                if (tokenResponse != null)
                {
                    // Cache the result for parallel requests
                    _refreshResultCache[cacheKey] = (tokenResponse, DateTimeOffset.UtcNow);
                    
                    // Set new cookies and user principal
                    var jwtConfig = context.RequestServices.GetService(typeof(JwtConfiguration)) as JwtConfiguration;
                    SetAuthCookies(context, tokenResponse, cookieConfig, jwtConfig);
                    
                    var principal = await jwtService.GetClaimsFromTokenAsync(tokenResponse.AccessToken);
                    if (principal != null)
                    {
                        context.User = principal;
                        context.Items[CsrfProtectionMiddleware.AuthTokenSourceItemKey] = 
                            CsrfProtectionMiddleware.AuthTokenSourceCookie;
                        
                        await _next(context);
                        return true;
                    }
                }
                else
                {
                    // Refresh failed - check cache one more time (another request might have succeeded)
                    if (TryUseCachedRefreshResult(context, jwtService, cookieConfig, cacheKey))
                    {
                        await _next(context);
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                await LogErrorAsync(context, "Token refresh failed", ex);
                return false;
            }
            finally
            {
                gate.Release();
                CleanupExpiredCacheEntry(cacheKey);
            }
        }

        /// <summary>
        /// Try to use a cached refresh result from a parallel request
        /// </summary>
        private bool TryUseCachedRefreshResult(
            HttpContext context, 
            IJwtService jwtService, 
            AuthCookieConfiguration cookieConfig, 
            string cacheKey)
        {
            if (_refreshResultCache.TryGetValue(cacheKey, out var cached) &&
                DateTimeOffset.UtcNow - cached.CreatedAt <= RefreshResultCacheTtl)
            {
                var jwtConfig = context.RequestServices.GetService(typeof(JwtConfiguration)) as JwtConfiguration;
                SetAuthCookies(context, cached.Response, cookieConfig, jwtConfig);
                
                var principal = jwtService.GetClaimsFromTokenAsync(cached.Response.AccessToken).GetAwaiter().GetResult();
                if (principal != null)
                {
                    context.User = principal;
                    context.Items[CsrfProtectionMiddleware.AuthTokenSourceItemKey] = 
                        CsrfProtectionMiddleware.AuthTokenSourceCookie;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove expired cache entry
        /// </summary>
        private static void CleanupExpiredCacheEntry(string cacheKey)
        {
            if (_refreshResultCache.TryGetValue(cacheKey, out var cached) &&
                DateTimeOffset.UtcNow - cached.CreatedAt > RefreshResultCacheTtl)
            {
                _refreshResultCache.TryRemove(cacheKey, out _);
            }
        }

        private static async Task UnauthorizedResponse(HttpContext context, string message = "Unauthorized")
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { message }));
        }

        private static string? ExtractTokenFromHeaderOrCookie(HttpContext context, AuthCookieConfiguration cookieConfig)
        {
            // Check Authorization header first
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            // Fallback to cookie
            var cookieName = cookieConfig?.AccessTokenCookieName ?? "auth_at";
            if (context.Request.Cookies.TryGetValue(cookieName, out var cookieToken) &&
                !string.IsNullOrWhiteSpace(cookieToken))
            {
                return cookieToken;
            }

            return null;
        }

        private static string? ExtractRefreshTokenFromCookie(HttpContext context, AuthCookieConfiguration cookieConfig)
        {
            var cookieName = cookieConfig?.RefreshTokenCookieName ?? "auth_rt";
            if (context.Request.Cookies.TryGetValue(cookieName, out var refreshToken) &&
                !string.IsNullOrWhiteSpace(refreshToken))
            {
                return refreshToken;
            }
            return null;
        }

        private void SetAuthCookies(HttpContext context, JwtTokenResponse tokenResponse, AuthCookieConfiguration cookieConfig, JwtConfiguration? jwtConfig)
        {
            var now = DateTimeOffset.UtcNow;
            var sameSite = ParseSameSite(cookieConfig.SameSite);

            // Access token cookie
            context.Response.Cookies.Append(
                cookieConfig.AccessTokenCookieName,
                tokenResponse.AccessToken,
                CreateCookieOptions(true, sameSite, cookieConfig.AccessTokenPath, 
                    now.AddSeconds(Math.Max(1, tokenResponse.ExpiresIn)), cookieConfig)
            );

            // Delete legacy refresh cookie path
            context.Response.Cookies.Delete(
                cookieConfig.RefreshTokenCookieName,
                CreateCookieOptions(true, sameSite, "/api/Auth", now.AddYears(-1), cookieConfig)
            );
            
            // Refresh token cookie
            context.Response.Cookies.Append(
                cookieConfig.RefreshTokenCookieName,
                tokenResponse.RefreshToken,
                CreateCookieOptions(true, sameSite, cookieConfig.RefreshTokenPath,
                    now.AddDays(Math.Max(1, jwtConfig?.RefreshTokenExpirationDays ?? 30)), cookieConfig)
            );
        }

        private static CookieOptions CreateCookieOptions(
            bool httpOnly, 
            SameSiteMode sameSite, 
            string path, 
            DateTimeOffset expires, 
            AuthCookieConfiguration cookieConfig)
        {
            var options = new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = cookieConfig.Secure,
                SameSite = sameSite,
                Path = string.IsNullOrWhiteSpace(path) ? "/" : path,
                Expires = expires,
                IsEssential = true
            };

            if (!string.IsNullOrWhiteSpace(cookieConfig.Domain))
                options.Domain = cookieConfig.Domain;

            return options;
        }

        private static SameSiteMode ParseSameSite(string? sameSite)
        {
            return sameSite?.Trim().ToLowerInvariant() switch
            {
                "none" => SameSiteMode.None,
                "strict" => SameSiteMode.Strict,
                "lax" => SameSiteMode.Lax,
                _ => SameSiteMode.Lax
            };
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
                return forwardedFor.Split(',')[0].Trim();

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private static string GetUserAgent(HttpContext context)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            return !string.IsNullOrWhiteSpace(userAgent) ? userAgent.Trim() : "Unknown";
        }

        private static async Task LogErrorAsync(HttpContext context, string message, Exception ex)
        {
            try
            {
                var loggingService = context.RequestServices.GetService(typeof(ILoggingService)) as ILoggingService;
                if (loggingService != null)
                {
                    await loggingService.LogErrorAsync(
                        message,
                        ex,
                        source: "JwtAuthenticationMiddleware",
                        module: "JWT",
                        action: "Authentication"
                    );
                }
            }
            catch { /* Ignore logging failures */ }
        }
    }
}
