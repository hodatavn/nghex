using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Nghex.Identity.Enum;

namespace Nghex.Identity.Middleware.Extensions
{
    /// <summary>
    /// JWT Claims Extensions
    /// </summary>
    public static class JwtClaimsExtensions
    {
        /// <summary>
        /// Get user ID from claims
        /// </summary>
        public static long? GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null) return null;

            // Prefer explicit account_id claim (we set this in JwtService)
            // Fall back to common JWT/ASP.NET mappings:
            // - "sub" (JwtRegisteredClaimNames.Sub)
            // - ClaimTypes.NameIdentifier (mapped from "sub" when inbound claim mapping is enabled)
            // - "user_id" (legacy/custom)
            var claim =
                principal.FindFirst("account_id") ??
                // principal.FindFirst("user_id") ??
                principal.FindFirst(JwtRegisteredClaimNames.Sub) ??
                principal.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null) return null;
            return long.TryParse(claim.Value, out var userId) ? userId : null;
        }

        /// <summary>
        /// Get username from claims
        /// </summary>
        public static string? GetUsername(this ClaimsPrincipal principal)
        {
            if (principal == null) return null;
            try
            {
                return principal.FindFirst("username")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get email from claims
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            if (principal == null) return null;

            return principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? 
                principal.FindFirst(ClaimTypes.Email)?.Value ??
                principal.FindFirst("Email")?.Value;
        }

        /// <summary>
        /// Get display name from claims
        /// </summary>
        public static string? GetDisplayName(this ClaimsPrincipal principal)
        {
            if (principal == null) return null;
            try
            {
                return principal.FindFirst("display_name")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get roles from claims.
        /// Supports both mapped (ClaimTypes.Role) and unmapped ("role") claim types.
        /// </summary>
        public static List<string> GetRoles(this ClaimsPrincipal principal)
        {
            // When MapInboundClaims = false, JWT "role" claim stays as "role"
            // When MapInboundClaims = true (default), it's mapped to ClaimTypes.Role
            var roles = principal.FindAll(ClaimTypes.Role)
                .Concat(principal.FindAll("role"))
                .Select(c => c.Value)
                .Distinct()
                .ToList();
            return roles;
        }

        /// <summary>
        /// Get role levels from claims
        /// </summary>
        public static List<RoleLevel> GetRoleLevels(this ClaimsPrincipal principal)
        {
            if (principal == null) return [];
            try
            {
                return principal.FindAll("role_level")
                .Select(c => 
                {
                    if (int.TryParse(c.Value, out var level))
                        return level.FromLevel();
                    return RoleLevel.User;
                })
                .Distinct()
                .ToList();
            }
            catch
            {
                return [];
            }
        }


        /// <summary>
        /// Get permissions from claims
        /// </summary>
        public static List<string> GetPermissions(this ClaimsPrincipal principal)
        {
            return principal.FindAll("permission").Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Check if user has specific role level
        /// </summary>
        public static bool HasRoleLevel(this ClaimsPrincipal principal, RoleLevel roleLevel)
        {
            if (principal == null) return false;
            var userLevels = principal.GetRoleLevels();
            return userLevels.Contains(roleLevel);
        }

        /// <summary>
        /// Check if user has any of the specified role levels
        /// </summary>
        public static bool HasAnyRoleLevel(this ClaimsPrincipal principal, params RoleLevel[] roleLevels)
        {
            if (principal == null || roleLevels == null || roleLevels.Length == 0) 
                return false;
            
            var userLevels = principal.GetRoleLevels();
            return userLevels.Any(level => roleLevels.Contains(level));
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        public static bool HasRole(this ClaimsPrincipal principal, string role)
        {
            return principal.IsInRole(role);
        }

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            return principal.FindAll("permission").Any(c => c.Value == permission);
        }
    }
}
