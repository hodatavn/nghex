using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;

namespace Nghex.Identity.Services.Interfaces
{
    public interface IAccountService
    {
        #region Basic CRUD Operations

        Task<AccountResponse?> GetByIdAsync(long id);
        Task<IEnumerable<AccountResponse>> GetAllAsync(bool isDeleted);
        Task<AccountResponse> CreateAsync(CreateAccountRequest request);
        Task<bool> UpdateAsync(UpdateAccountRequest request);
        Task<bool> DeleteAsync(long id, string deletedBy);
        Task<bool> RestoreAsync(long id, string restoredBy);

        #endregion

        #region Account-Specific Operations

        Task<AccountResponse?> GetByUsernameAsync(string username);
        Task<AccountResponse?> GetByEmailAsync(string email);
        Task LockAccountAsync(string username, DateTime lockedUntil, string lockedBy);
        Task UnlockAccountAsync(string username, string unlockedBy);

        #endregion

        #region Authentication Operations

        Task<AccountResponse?> AuthenticateAsync(string username, string password, string ipAddress);
        Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string username);
        Task<(string IpAddress, DateTime? LastLoginAt, int FailedAttempts)> GetLoginTrackingInfoAsync(string username);
        Task<bool> ValidateSetupCredentialsAsync(string username, string password);

        #endregion
    }
}
