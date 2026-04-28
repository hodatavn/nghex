using System.Security.Claims;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Identity.Models;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IJwtService
    {
        Task<JwtTokenResponse> GenerateTokensAsync(AccountResponse account, List<RoleResponse> roles, string? ipAddress = null, string? userAgent = null);
        Task<JwtTokenResponse?> RefreshTokenAsync(string refreshToken, string? ipAddress = null, string? userAgent = null);
        Task<bool> RevokeTokenAsync(string tokenId);
        Task<bool> RevokeAllTokensForAccountAsync(long accountId);
        Task<bool> ValidateTokenAsync(string token);
        Task<ClaimsPrincipal?> GetClaimsFromTokenAsync(string token);
        Task<bool> RotateSecretKeyAsync();
        Task<int> CleanupExpiredTokensAsync();
    }
}
