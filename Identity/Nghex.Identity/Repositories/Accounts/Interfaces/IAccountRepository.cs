using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Base.Repositories;

namespace Nghex.Identity.Repositories.Accounts.Interfaces
{
    public interface IAccountRepository : IRepository<AccountEntity>
    {
        /// <summary>
        /// Get account by ID and is deleted
        /// </summary>
        /// <param name="id">The ID of the account</param>
        /// <returns>The account</returns>
        Task<AccountEntity?> GetByIdAndIsDeletedAsync(long id);
        /// <summary>
        /// Restore account
        /// </summary>
        /// <param name="id">The ID of the account to restore</param>
        /// <param name="restoredBy">The user who restored the account</param>
        /// <returns>True if the account was restored, false otherwise</returns>
        Task<bool> RestoreAsync(long id, string restoredBy);

        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <param name="isDeleted">Whether to include deleted accounts</param>
        /// <returns>The accounts</returns>
        Task<IEnumerable<AccountEntity>> GetAllAsync(bool isDeleted);

        /// <summary>
        /// Get account by username
        /// </summary>
        /// <param name="username">The username to check</param>
        Task<AccountEntity?> GetByUsernameAsync(string username);

        /// <summary>
        /// Get account by email
        /// </summary>
        /// <param name="email">The email to check</param>
        Task<AccountEntity?> GetByEmailAsync(string email);

        /// <summary>
        /// Check if username exists
        /// </summary>
        /// <param name="username">The username to check</param>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Check if email exists
        /// </summary>
        /// <param name="email">The email to check</param>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Reset failed login attempts
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="resetBy">The user who reset the failed login attempts</param>
        Task ResetFailedLoginAttemptsAsync(long accountId, string resetBy);

        /// <summary>
        /// Lock account
        /// </summary>
        /// <param name="username">The username of the account</param>
        /// <param name="lockedUntil">The date and time the account will be locked until</param>
        /// <param name="lockedBy">The user who locked the account</param>
        Task LockAccountAsync(string username, DateTime lockedUntil, string lockedBy);

        /// <summary>
        /// Unlock account
        /// </summary>
        /// <param name="username">The username of the account</param>
        /// <param name="unlockedBy">The user who unlocked the account</param>  
        Task UnlockAccountAsync(string username, string unlockedBy);

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>True if the password was changed, false otherwise</returns>
        Task<bool> ChangePasswordAsync(long accountId, string newPassword);

        /// <summary>
        /// Update last login
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="ipAddress">The IP address</param>
        Task UpdateLastLoginAsync(long accountId, string ipAddress);

        /// <summary>
        /// Increment failed login attempts
        /// </summary>
        /// <param name="accountId">The account ID</param>
        /// <param name="ipAddress">The IP address</param>
        Task IncrementFailedLoginAttemptsAsync(long accountId, string ipAddress);

    }
}
