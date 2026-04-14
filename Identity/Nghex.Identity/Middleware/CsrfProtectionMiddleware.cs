using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Nghex.Identity.Models;

namespace Nghex.Identity.Middleware
{
    /// <summary>
    /// Minimal CSRF protection for cookie-authenticated requests.
    /// Enforces a double-submit token: a readable CSRF cookie + matching request header.
    ///
    /// Only enforced when the access token was sourced from cookies (not from Authorization header).
    /// </summary>
    public class CsrfProtectionMiddleware
    {
        public const string AuthTokenSourceItemKey = "Nghex.AuthTokenSource";
        public const string AuthTokenSourceCookie = "cookie";

        private readonly RequestDelegate _next;

        public CsrfProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AuthCookieConfiguration cookieConfig)
        {
            if (!cookieConfig.EnableCsrfProtection)
            {
                await _next(context);
                return;
            }

            // Only enforce for unsafe methods
            if (HttpMethods.IsGet(context.Request.Method) ||
                HttpMethods.IsHead(context.Request.Method) ||
                HttpMethods.IsOptions(context.Request.Method) ||
                HttpMethods.IsTrace(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // If routing hasn't selected an endpoint yet, do nothing.
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }

            // Skip endpoints that allow anonymous access (e.g. /api/Auth/login).
            if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            // Skip paths explicitly excluded from CSRF enforcement (e.g. logout).
            var requestPath = context.Request.Path.Value ?? string.Empty;
            if (cookieConfig.CsrfExcludedPaths != null && cookieConfig.CsrfExcludedPaths.Count > 0)
            {
                foreach (var p in cookieConfig.CsrfExcludedPaths)
                {
                    if (string.IsNullOrWhiteSpace(p))
                        continue;
                    if (requestPath.Equals(p, StringComparison.OrdinalIgnoreCase))
                    {
                        await _next(context);
                        return;
                    }
                }
            }

            // Only enforce CSRF for endpoints that require authorization.
            // This prevents public endpoints from being blocked just because stale auth cookies exist.
            var authorizeData = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            if (authorizeData == null || authorizeData.Count == 0)
            {
                await _next(context);
                return;
            }

            // Only enforce CSRF for authenticated requests.
            // If the cookie token is missing/expired/invalid, authentication won't succeed and CSRF shouldn't be the blocker.
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // Only enforce when authentication came from cookies
            var tokenSource = context.Items.TryGetValue(AuthTokenSourceItemKey, out var v) ? v?.ToString() : null;
            if (!string.Equals(tokenSource, AuthTokenSourceCookie, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Require CSRF cookie + header match
            context.Request.Cookies.TryGetValue(cookieConfig.CsrfCookieName, out var csrfCookie);
            var csrfHeader = context.Request.Headers[cookieConfig.CsrfHeaderName].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(csrfCookie) || string.IsNullOrWhiteSpace(csrfHeader) ||
                !string.Equals(csrfCookie, csrfHeader, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
                {
                    message = "CSRF validation failed"
                }));
                return;
            }

            await _next(context);
        }
    }
}
