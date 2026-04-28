using Mapster;
using Nghex.Identity.Api.Models.Requests;
using Nghex.Identity.Api.Models.Responses;
using Nghex.Core.Helper;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Persistence.Entities;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Identity.Services.Interfaces;
using Nghex.Core.Configuration;
using Nghex.Licensing.Interfaces;
using Nghex.Utilities;

namespace Nghex.Identity.Services
{
    public class AccountService(
        IAccountRepository accountRepository,
        IAccountRoleRepository accountRoleRepository,
        IRoleService roleService,
        IPasswordService passwordService,
        IAppConfigurationReader appConfiguration,
        ILoggingService loggingService,
        ILicenseService licenseService) : IAccountService
    {
        private readonly IAccountRepository _accountRepository = accountRepository;
        private readonly IAccountRoleRepository _accountRoleRepository = accountRoleRepository;
        private readonly IRoleService _roleService = roleService;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IAppConfigurationReader _appConfiguration = appConfiguration;
        private readonly ILoggingService _loggingService = loggingService;
        private readonly ILicenseService _licenseService = licenseService;
        private readonly string DefaultPassword = appConfiguration.GetAppSettingValueByKey("Security:DefaultPassword") ?? "123456";
        private readonly int PasswordMinLength = int.TryParse(appConfiguration.GetAppSettingValueByKey("Security:PasswordMinLength"), out var passwordMinLength) ? passwordMinLength : 6;
        private readonly int LoginFailedAttempts = int.TryParse(appConfiguration.GetAppSettingValueByKey("Security:LoginFailedAttempts"), out var loginFailedAttempts) ? loginFailedAttempts : 5;
        private readonly int LoginFailedAttemptsLockoutDuration = int.TryParse(appConfiguration.GetAppSettingValueByKey("Security:LoginFailedAttemptsLockoutDuration"), out var loginFailedAttemptsLockoutDuration) ? loginFailedAttemptsLockoutDuration : 30;

        #region Basic CRUD Operations

        public async Task<AccountResponse?> GetByIdAsync(long id)
        {
            if (id <= 0) return null;
            var entity = await _accountRepository.GetByIdAsync(id);
            return entity?.Adapt<AccountResponse>();
        }

        public async Task<IEnumerable<AccountResponse>> GetAllAsync(bool isDeleted)
        {
            var entities = await _accountRepository.GetAllAsync(isDeleted);
            return entities.Select(e => e.Adapt<AccountResponse>());
        }

        public async Task<AccountResponse> CreateAsync(CreateAccountRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            await ValidateNewAccountAsync(request);

            var entity = request.Adapt<AccountEntity>();
            entity.Password = _passwordService.HashPassword(request.Password);

            var id = await _accountRepository.AddAsync(entity);
            entity.Id = id;

            return entity.Adapt<AccountResponse>();
        }

        public async Task<bool> UpdateAsync(UpdateAccountRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existingEntity = await _accountRepository.GetByUsernameAsync(request.Username);
            if (existingEntity == null || existingEntity.IsDeleted)
                throw new InvalidOperationException("Account not found or not allowed to update");

            await ValidateUpdateAccountAsync(request, existingEntity.Id);

            existingEntity.DisplayName = request.DisplayName;
            existingEntity.Email = request.Email!;
            existingEntity.IsActive = request.IsActive;
            existingEntity.UpdatedBy = request.UpdatedBy;

            return await _accountRepository.UpdateAsync(existingEntity);
        }

        public async Task<bool> DeleteAsync(long id, string deletedBy)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            if (account == null) return false;
            return await _accountRepository.DeleteAsync(id, deletedBy);
        }

        public async Task<bool> RestoreAsync(long id, string restoredBy)
        {
            var account = await _accountRepository.GetByIdAndIsDeletedAsync(id);
            if (account == null) return false;
            return await _accountRepository.RestoreAsync(id, restoredBy);
        }

        #endregion

        #region Account-Specific Operations

        public async Task<AccountResponse?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            var entity = await _accountRepository.GetByUsernameAsync(username);
            return entity?.Adapt<AccountResponse>();
        }

        public async Task<AccountResponse?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var entity = await _accountRepository.GetByEmailAsync(email);
            return entity?.Adapt<AccountResponse>();
        }

        public async Task LockAccountAsync(string username, DateTime lockedUntil, string lockedBy)
        {
            var account = await GetAccountEntityOrThrowAsync(username);
            await _accountRepository.LockAccountAsync(account.Username, lockedUntil, lockedBy);
        }

        public async Task UnlockAccountAsync(string username, string unlockedBy)
        {
            var account = await GetAccountEntityOrThrowAsync(username);
            await _accountRepository.UnlockAccountAsync(account.Username, unlockedBy);
        }

        #endregion

        #region Authentication Operations

        public async Task<AccountResponse?> AuthenticateAsync(string username, string password, string ipAddress)
        {
            var account = await _accountRepository.GetByUsernameAsync(username);
            if (account == null || !account.IsActive || account.IsDeleted)
                throw new InvalidOperationException("Account not found or not allowed to access system");

            if (!_passwordService.VerifyPassword(password, account.Password))
            {
                await UpdateFailedLoginAsync(username, ipAddress);
                await _loggingService.LogWarningAsync(
                    $"Login failed for username: {username}",
                    source: "AccountService.AuthenticateAsync",
                    module: "Authentication",
                    action: "Login",
                    details: new { Username = username, IPAddress = ipAddress }
                );
                throw new InvalidOperationException("Invalid username or password");
            }

            if (account.IsLocked && account.LockedUntil.HasValue && account.LockedUntil > DateTime.UtcNow)
                throw new InvalidOperationException("Account is locked until " + account.LockedUntil?.ToString("dd/MM/yyyy HH:mm:ss"));

            if (account.IsLocked && !account.LockedUntil.HasValue)
                throw new InvalidOperationException("Account is locked");

            var licenseValidation = await _licenseService.ValidateLicenseAsync();
            if (!licenseValidation.IsValid)
                throw new InvalidOperationException($"License validation failed: {licenseValidation.Message}");

            await UpdateSuccessfulLoginAsync(username, ipAddress);
            return account.Adapt<AccountResponse>();
        }

        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var account = await GetAccountEntityOrThrowAsync(username);

            if (!_passwordService.VerifyPassword(currentPassword, account.Password))
                throw new InvalidOperationException("Invalid current password");

            var minLength = await GetPasswordMinLengthAsync();
            if (newPassword.Length < minLength)
                throw new ArgumentException($"Password must be at least {minLength} characters");

            account.Password = _passwordService.HashPassword(newPassword);
            return await _accountRepository.ChangePasswordAsync(account.Id, account.Password);
        }

        public async Task<bool> ResetPasswordAsync(string username)
        {
            var account = await GetAccountEntityOrThrowAsync(username);
            account.Password = _passwordService.HashPassword(DefaultPassword);
            return await _accountRepository.ChangePasswordAsync(account.Id, account.Password);
        }

        public async Task<(string IpAddress, DateTime? LastLoginAt, int FailedAttempts)> GetLoginTrackingInfoAsync(string username)
        {
            var account = await GetAccountEntityOrThrowAsync(username);
            return (account.IpAddress ?? string.Empty, account.LastLoginAt, account.FailedLoginAttempts);
        }

        #endregion

        #region Setup Credentials

        public Task<bool> ValidateSetupCredentialsAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return Task.FromResult(false);

            var setupUsername = _appConfiguration.GetAppSettingValueByKey("SetupAccount:Username");
            if (string.IsNullOrWhiteSpace(setupUsername))
                setupUsername = _appConfiguration.GetAppSettingValueByKey("SetupSettings:SetupAccount:Username");

            if (!string.Equals(username, setupUsername, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(false);

            var setupPassword = _appConfiguration.GetAppSettingValueByKey("SetupAccount:Password");
            if (string.IsNullOrWhiteSpace(setupPassword))
                setupPassword = _appConfiguration.GetAppSettingValueByKey("SetupSettings:SetupAccount:Password");

            if (string.IsNullOrWhiteSpace(setupPassword))
                return Task.FromResult(false);

            if (string.Equals(password, setupPassword, StringComparison.Ordinal))
                return Task.FromResult(true);

            return Task.FromResult(_passwordService.VerifyPassword(password, setupPassword));
        }

        #endregion

        #region Private Helpers

        private async Task ValidateNewAccountAsync(CreateAccountRequest request)
        {
            if (!ModelHelper.IsValidCode(request.Username))
                throw new ArgumentException("Username can only contain letters, numbers, and underscores");

            if (await _accountRepository.UsernameExistsAsync(request.Username))
                throw new ArgumentException("Username already exists");

            var minLength = await GetPasswordMinLengthAsync();
            if (request.Password.Length < minLength)
                throw new ArgumentException($"Password must be at least {minLength} characters");
        }

        private async Task ValidateUpdateAccountAsync(UpdateAccountRequest request, long currentAccountId)
        {
            if (!string.IsNullOrWhiteSpace(request.Email) && !ModelHelper.IsValidEmail(request.Email))
                throw new ArgumentException("Invalid email format");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existingByEmail = await _accountRepository.GetByEmailAsync(request.Email);
                if (existingByEmail != null && existingByEmail.Id != currentAccountId)
                    throw new ArgumentException("Email already exists");
            }
        }

        private async Task<AccountEntity> GetAccountEntityOrThrowAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required");

            var account = await _accountRepository.GetByUsernameAsync(username);
            if (account == null || !account.IsActive || account.IsDeleted)
                throw new InvalidOperationException("Account not found or not allowed to access system");
            return account;
        }

        private async Task<int> GetPasswordMinLengthAsync()
        {
            var configValue = await _appConfiguration.GetIntValueAsync("PASSWORD_MIN_LENGTH");
            return Math.Max(configValue, PasswordMinLength);
        }

        private async Task<int> GetLoginFailedAttemptsAsync()
        {
            var configValue = await _appConfiguration.GetIntValueAsync("LOGIN_FAILED_ATTEMPTS");
            return Math.Max(configValue, LoginFailedAttempts);
        }

        private async Task<int> GetLoginFailedAttemptsLockoutDurationAsync()
        {
            var configValue = await _appConfiguration.GetIntValueAsync("LOGIN_FAILED_ATTEMPTS_LOCKOUT_DURATION");
            return Math.Max(configValue, LoginFailedAttemptsLockoutDuration);
        }

        private async Task UpdateSuccessfulLoginAsync(string username, string ipAddress)
        {
            var account = await GetAccountEntityOrThrowAsync(username);
            account.LastLoginAt = DateTime.UtcNow;
            account.FailedLoginAttempts = 0;
            account.IsLocked = false;
            account.LockedUntil = null;
            account.IpAddress = ipAddress;
            account.UpdatedBy = account.Username;
            await _accountRepository.UpdateAsync(account);
        }

        private async Task UpdateFailedLoginAsync(string username, string ipAddress)
        {
            var account = await GetAccountEntityOrThrowAsync(username);
            account.FailedLoginAttempts++;
            account.IpAddress = ipAddress;
            account.LastLoginAt = DateTime.UtcNow;
            account.UpdatedBy = username;

            if (account.FailedLoginAttempts >= await GetLoginFailedAttemptsAsync())
            {
                account.IsLocked = true;
                account.LockedUntil = DateTime.UtcNow.AddMinutes(await GetLoginFailedAttemptsLockoutDurationAsync());
            }

            await _accountRepository.UpdateAsync(account);
        }

        #endregion
    }
}
