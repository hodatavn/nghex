using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Repositories.Interfaces
{
    /// <summary>
    /// Interface cho JWT Token Repository
    /// </summary>
    public interface IJwtTokenRepository
    {
        /// <summary>
        /// Thêm JWT token mới
        /// </summary>
        Task<long> AddAsync(JwtTokenEntity jwtToken);

        /// <summary>
        /// Lấy JWT token theo token ID
        /// </summary>
        Task<JwtTokenEntity?> GetByTokenIdAsync(string tokenId);

        /// <summary>
        /// Lấy JWT token theo refresh token
        /// </summary>
        Task<JwtTokenEntity?> GetByRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Lấy tất cả JWT tokens của một account
        /// </summary>
        Task<List<JwtTokenEntity>> GetByAccountIdAsync(long accountId);

        /// <summary>
        /// Cập nhật JWT token
        /// </summary>
        Task<bool> UpdateAsync(JwtTokenEntity jwtToken);

        /// <summary>
        /// Revoke JWT token
        /// </summary>
        Task<bool> RevokeTokenAsync(string tokenId);

        /// <summary>
        /// Revoke JWT token by refresh token
        /// </summary>
        Task<bool> RevokeByRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Try revoke JWT token (concurrency-safe). Returns false if it was already revoked.
        /// </summary>
        Task<bool> TryRevokeTokenAsync(string tokenId);

        /// <summary>
        /// Revoke tất cả tokens của một account
        /// </summary>
        Task<bool> RevokeAllTokensForAccountAsync(long accountId);

        /// <summary>
        /// Xóa các token đã hết hạn
        /// </summary>
        Task<int> CleanupExpiredTokensAsync();

        /// <summary>
        /// Kiểm tra token có tồn tại và hợp lệ không
        /// </summary>
        Task<bool> IsTokenValidAsync(string tokenId);
    }
}
