using System.Security.Claims;
using Nghex.Identity.DTOs.Accounts;
using Nghex.Identity.DTOs.Roles;
using Nghex.Identity.Models;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IJwtService
    {
        /// <summary>
        /// Tạo access token và refresh token
        /// </summary>
        Task<JwtTokenResponse> GenerateTokensAsync(AccountDto account, List<RoleDto> roles, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Refresh access token
        /// </summary>
        Task<JwtTokenResponse?> RefreshTokenAsync(string refreshToken, string? ipAddress = null, string? userAgent = null);

        /// <summary>
        /// Revoke token
        /// </summary>
        Task<bool> RevokeTokenAsync(string tokenId);

        /// <summary>
        /// Revoke tất cả tokens của account
        /// </summary>
        Task<bool> RevokeAllTokensForAccountAsync(long accountId);

        /// <summary>
        /// Validate token
        /// </summary>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Get claims từ token
        /// </summary>
        Task<ClaimsPrincipal?> GetClaimsFromTokenAsync(string token);

        /// <summary>
        /// Rotate secret key
        /// </summary>
        Task<bool> RotateSecretKeyAsync();

        /// <summary>
        /// Cleanup expired tokens
        /// </summary>
        Task<int> CleanupExpiredTokensAsync();
    }
}
