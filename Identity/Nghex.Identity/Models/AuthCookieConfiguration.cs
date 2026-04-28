namespace Nghex.Identity.Models
{
    /// <summary>
    /// Configuration for storing JWT tokens in HttpOnly cookies.
    /// </summary>
    public class AuthCookieConfiguration
    {
        /// <summary>
        /// Cookie name for access token.
        /// </summary>
        public string AccessTokenCookieName { get; set; } = "auth_at";

        /// <summary>
        /// Cookie name for refresh token.
        /// </summary>
        public string RefreshTokenCookieName { get; set; } = "auth_rt";

        /// <summary>
        /// Cookie name for CSRF token (NOT HttpOnly; client reads it and sends via header).
        /// </summary>
        public string CsrfCookieName { get; set; } = "auth_csrf";

        /// <summary>
        /// Header name to submit CSRF token.
        /// </summary>
        public string CsrfHeaderName { get; set; } = "X-CSRF-TOKEN";

        /// <summary>
        /// Enable CSRF protection for unsafe HTTP methods when authentication comes from cookies.
        /// </summary>
        public bool EnableCsrfProtection { get; set; } = true;

        /// <summary>
        /// API paths that are excluded from CSRF enforcement (case-insensitive, path-only; query string ignored).
        /// Useful for endpoints like logout where CSRF protection is often not necessary.
        /// </summary>
        public List<string> CsrfExcludedPaths { get; set; } = new()
        {
            "/api/Auth/logout",
            "/api/Auth/logout-all"
        };

        /// <summary>
        /// Optional cookie domain (leave empty to use host-only cookies).
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// Path for access token cookie (default "/").
        /// </summary>
        public string AccessTokenPath { get; set; } = "/";

        /// <summary>
        /// Path for refresh token cookie (default "/api/auth").
        /// </summary>
        // IMPORTANT:
        // If you want backend auto-refresh to work for ALL API endpoints, the browser must send the refresh token
        // cookie on those requests. Therefore RefreshTokenPath should be "/" (or at least "/api"), not "/api/Auth".
        public string RefreshTokenPath { get; set; } = "/";

        /// <summary>
        /// Path for CSRF cookie (default "/").
        /// </summary>
        public string CsrfPath { get; set; } = "/";

        /// <summary>
        /// SameSite policy: "None", "Lax", "Strict".
        /// For cross-site SPA auth you typically need "None" (and Secure=true).
        /// </summary>
        public string SameSite { get; set; } = "None";

        /// <summary>
        /// Mark cookies as Secure. If true, cookies only sent over HTTPS.
        /// NOTE: SameSite=None requires Secure in modern browsers.
        /// </summary>
        public bool Secure { get; set; } = true;

        /// <summary>
        /// Whether to still return access/refresh tokens in JSON response body (backward compatibility).
        /// If false, API will only return user info + expiry.
        /// </summary>
        public bool ReturnTokensInBody { get; set; } = false;
    }
}
