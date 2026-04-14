using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using System.Text;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Models;
using Nghex.Identity.Services.Interfaces;
using Nghex.Identity.Enum;
using System.Threading;

namespace Nghex.Identity.Middleware.Extensions
{
    /// <summary>
    /// Extension methods for JWT Authentication
    /// </summary>
    public static class JwtAuthenticationExtensions
    {
        private static long _headerTokenHits;
        private static long _cookieTokenHits;
        private static long _queryTokenHits;

        /// <summary>
        /// Configure JWT Authentication services
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtConfig = services.BuildServiceProvider().GetRequiredService<JwtConfiguration>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Keep JWT claim types as-is (e.g. "email", "sub") instead of mapping to WS-Fed/ClaimTypes URIs
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig.Issuer,
                        ValidAudience = jwtConfig.Audience,
                        IssuerSigningKey = BuildSymmetricKey(jwtConfig.SecretKey),
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = "role"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = async context =>
                        {
                            // Token validation failed - middleware will handle refresh
                            // Only log actual errors (not expired tokens which are expected)
                            var isExpired = context.Exception is SecurityTokenExpiredException;
                            if (!isExpired)
                            {
                                try
                                {
                                    var loggingService = context.HttpContext.RequestServices.GetService<ILoggingService>();
                                    if (loggingService != null)
                                    {
                                        await loggingService.LogErrorAsync(
                                            "JWT Authentication failed",
                                            context.Exception,
                                            source: "JwtBearerEvents.OnAuthenticationFailed",
                                            module: "JWT",
                                            action: "Authentication"
                                        );
                                    }
                                }
                                catch { /* Ignore logging failures */ }
                            }
                        },
                        
                        OnTokenValidated = async context =>
                        {
                            // After JWT signature validation passes, verify token is still valid in DB
                            try
                            {
                                var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
                                var token = context.SecurityToken as JwtSecurityToken;
                                
                                if (token != null)
                                {
                                    var isValidInDb = await jwtService.ValidateTokenAsync(token.RawData);
                                    
                                    if (!isValidInDb)
                                    {
                                        // Token revoked/expired in DB - let middleware handle refresh
                                        context.Fail("Token is no longer valid.");
                                    }
                                }
                            }
                            catch
                            {
                                // On error, fail auth and let middleware attempt refresh
                                context.Fail("Token validation error.");
                            }
                        },
                        
                        OnMessageReceived = context =>
                        {
                            var path = context.HttpContext.Request.Path;

                            // SignalR typically sends token via query string.
                            // HTTP APIs should prioritize Authorization header, then cookie.
                            if (path.StartsWithSegments("/hubs") || path.StartsWithSegments("/notificationHub"))
                            {
                                var accessToken = context.Request.Query["access_token"].ToString();
                                if (!string.IsNullOrWhiteSpace(accessToken))
                                    SetToken(context, accessToken, "query");
                            }

                            if (string.IsNullOrEmpty(context.Token))
                                TryReadFromAuthorizationHeader(context);

                            if (string.IsNullOrEmpty(context.Token))
                                TryReadFromCookie(context);
                            
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .Build();

                // Register dynamic policies for RoleLevel authorization
                // Generate all possible combinations of role levels to ensure any order works
                var allRoleLevels = new[] { 0, 1, 99 }; // SuperAdmin, Admin, User
                var levelPolicies = new List<int[]>();
                
                // Generate all non-empty combinations (2^3 - 1 = 7 combinations)
                for (int i = 1; i < (1 << allRoleLevels.Length); i++)
                {
                    var combination = new List<int>();
                    for (int j = 0; j < allRoleLevels.Length; j++)
                    {
                        if ((i & (1 << j)) != 0)
                            combination.Add(allRoleLevels[j]);
                    }
                    if (combination.Count > 0)
                    {
                        // Sort to ensure consistent policy names regardless of order
                        combination.Sort();
                        levelPolicies.Add(combination.ToArray());
                    }
                }
                
                foreach (var policy in levelPolicies)
                {
                    var policyName = $"RoleLevelPolicy_{string.Join("_", policy.Select(p => p.ToString()))}";
                    var roleLevelArray = policy.Select(level => level.FromLevel()).ToArray();
                    options.AddPolicy(policyName, policyBuilder =>
                    {
                        policyBuilder.Requirements.Add(new RoleLevelRequirement(roleLevelArray));
                    });
                }
            });

            return services;
        }

        /// <summary>
        /// Use JWT Authentication Middleware pipeline
        /// </summary>
        public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            
            // JwtAuthenticationMiddleware handles token refresh when access token is expired/missing
            app.UseMiddleware<JwtAuthenticationMiddleware>();
            
            // CSRF protection for cookie-authenticated requests
            app.UseMiddleware<CsrfProtectionMiddleware>();
            
            app.UseAuthorization();
            
            return app;
        }

        private static SecurityKey BuildSymmetricKey(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("JWT SecretKey is missing. Configure Jwt:SecretKey with a strong random string (>=32 chars).");
            if (secret.Length < 32)
                throw new InvalidOperationException("JWT SecretKey too short. Must be at least 32 characters.");
            
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var securityKey = new SymmetricSecurityKey(keyBytes)
            {
                KeyId = GetKeyId(secret)
            };
            return securityKey;
        }

        private static string GetKeyId(string secret)
        {
            // Generate a consistent KeyId based on the secret key
            // This ensures the same KeyId is used for both token creation and validation
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(secret);
                var hash = sha256.ComputeHash(keyBytes);
                return Convert.ToBase64String(hash).Substring(0, 16).Replace("+", "-").Replace("/", "_");
            }
        }

        private static void TryReadFromAuthorizationHeader(MessageReceivedContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrWhiteSpace(token))
                    SetToken(context, token, "header");
            }
        }

        private static void TryReadFromCookie(MessageReceivedContext context)
        {
            var cookieConfig = context.HttpContext.RequestServices.GetService<AuthCookieConfiguration>();
            var accessCookieName = cookieConfig?.AccessTokenCookieName ?? "auth_at";

            if (context.Request.Cookies.TryGetValue(accessCookieName, out var cookieToken) &&
                !string.IsNullOrWhiteSpace(cookieToken))
            {
                SetToken(context, cookieToken, "cookie");
                context.HttpContext.Items[CsrfProtectionMiddleware.AuthTokenSourceItemKey] =
                    CsrfProtectionMiddleware.AuthTokenSourceCookie;
            }
        }

        private static void SetToken(MessageReceivedContext context, string token, string source)
        {
            context.Token = token;

            var hitCount = source switch
            {
                "header" => Interlocked.Increment(ref _headerTokenHits),
                "cookie" => Interlocked.Increment(ref _cookieTokenHits),
                "query" => Interlocked.Increment(ref _queryTokenHits),
                _ => 0
            };

            if (hitCount > 0 && hitCount % 200 == 0)
            {
                try
                {
                    var loggingService = context.HttpContext.RequestServices.GetService<ILoggingService>();
                    loggingService?.LogInformationAsync(
                        $"JWT source counters => header:{_headerTokenHits}, cookie:{_cookieTokenHits}, query:{_queryTokenHits}",
                        source: "JwtBearerEvents.OnMessageReceived",
                        module: "JWT",
                        action: "TokenSourceMetrics");
                }
                catch
                {
                    // Ignore logging failures, never block auth pipeline
                }
            }
        }
    }
}
