using Nghex.Identity.DTOs.Accounts;
using Nghex.Identity.DTOs.Permissions;
using Nghex.Identity.DTOs.Roles;

namespace Nghex.Identity.Services.Interfaces
{
    /// <summary>
    /// Account Service interface với business logic cho account management
    /// </summary>
    public interface IAccountService
    {
        #region Basic CRUD Operations

        /// <summary>
        /// Get account by ID
        /// </summary>
        /// <param name="id">The ID of the account to get</param>
        /// <returns>The account DTO</returns>
        Task<AccountDto?> GetByIdAsync(long id);

        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <param name="isDeleted">Include deleted accounts</param>
        /// <returns>The account DTOs</returns>
        Task<IEnumerable<AccountDto>> GetAllAsync(bool isDeleted);

        /// <summary>
        /// Create a new account
        /// </summary>
        /// <param name="createDto">The account data to create</param>
        /// <returns>The created account DTO with ID</returns>
        Task<AccountDto> CreateAsync(CreateAccountDto createDto);

        /// <summary>
        /// Update an account
        /// </summary>
        /// <param name="updateDto">The account data to update</param>
        /// <returns>True if the account was updated, false otherwise</returns>
        Task<bool> UpdateAsync(UpdateAccountDto updateDto);

        /// <summary>
        /// Delete an account
        /// </summary>
        /// <param name="id">The ID of the account to delete</param>
        /// <param name="deletedBy">The user who deleted the account</param>
        /// <returns>True if the account was deleted, false otherwise</returns>
        Task<bool> DeleteAsync(long id, string deletedBy);

        /// <summary>
        /// Restore an account
        /// </summary>
        /// <param name="id">The ID of the account to restore</param>
        /// <param name="restoredBy">The user who restored the account</param>
        /// <returns>True if the account was restored, false otherwise</returns>
        Task<bool> RestoreAsync(long id, string restoredBy);

        #endregion

        #region Account-Specific Operations

        /// <summary>
        /// Get account by username
        /// </summary>
        /// <param name="username">The username of the account to get</param>
        /// <returns>The account DTO</returns>
        Task<AccountDto?> GetByUsernameAsync(string username);

        /// <summary>
        /// Get account by email
        /// </summary>
        /// <param name="email">The email of the account to get</param>
        /// <returns>The account DTO</returns>
        Task<AccountDto?> GetByEmailAsync(string email);

        /// <summary>
        /// Lock an account
        /// </summary>
        /// <param name="username">The username of the account to lock</param>
        /// <param name="lockedUntil">The date and time the account will be locked until</param>
        /// <param name="lockedBy">The user who locked the account</param>
        Task LockAccountAsync(string username, DateTime lockedUntil, string lockedBy);
        
        /// <summary>
        /// Unlock an account
        /// </summary>
        /// <param name="username">The username of the account to unlock</param>
        /// <param name="unlockedBy">The user who unlocked the account</param>
        Task UnlockAccountAsync(string username, string unlockedBy);

        #endregion

        #region Authentication Operations

        /// <summary>
        /// Authenticate an account
        /// </summary>
        /// <param name="username">The username of the account to authenticate</param>
        /// <param name="password">The password of the account to authenticate</param>
        /// <param name="ipAddress">The IP address of the authentication request</param>
        /// <returns>The authenticated account DTO</returns>
        Task<AccountDto?> AuthenticateAsync(string username, string password, string ipAddress);

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="username">The username of the account to change the password</param>
        /// <param name="currentPassword">The current password of the account</param>
        /// <param name="newPassword">The new password of the account</param>
        /// <returns>True if the password was changed, false otherwise</returns>
        Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="username">The username of the account to reset the password</param>
        /// <returns>True if the password was reset, false otherwise</returns>
        Task<bool> ResetPasswordAsync(string username);

        /// <summary>
        /// Get login tracking information
        /// </summary>
        /// <param name="username">The username of the account to get the login tracking information</param>
        /// <returns>The login tracking information</returns>
        Task<(string IpAddress, DateTime? LastLoginAt, int FailedAttempts)> GetLoginTrackingInfoAsync(string username);

        /// <summary>
        /// Validate setup credentials
        /// </summary>
        /// <param name="username">The username to validate</param>
        /// <param name="password">The password to validate</param>
        /// <returns>True if the credentials are valid, false otherwise</returns>
        Task<bool> ValidateSetupCredentialsAsync(string username, string password);
        
        #endregion

    }
}
