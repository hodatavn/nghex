using Nghex.Core.Enum;
using Mapster;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Nghex.Identity.Models;
using Nghex.Base.Repositories;
using Nghex.Identity.Repositories.Interfaces;
using Nghex.Identity.Repositories.Accounts.Interfaces;
using Nghex.Logging.Interfaces;
using Nghex.Utilities;
using Nghex.Core.Extension;
using Nghex.Identity.Enum;
using Nghex.Identity.DTOs.Accounts;
using Nghex.Identity.DTOs.Roles;
using Nghex.Identity.Services.Interfaces;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;

namespace Nghex.Identity.Services
{
    /// <summary>
    /// JWT Service implementation với thuật toán HS256 (HMAC-SHA256)
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IJwtTokenRepository _jwtTokenRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IAccountRoleRepository _accountRoleRepository;
        private readonly ILoggingService _loggingService;
        private readonly JwtConfiguration _jwtConfig;
        private readonly object _sync = new();

        public JwtService(
            IJwtTokenRepository jwtTokenRepository,
            IAccountRepository accountRepository,
            IAccountRoleRepository accountRoleRepository,
            ILoggingService loggingService,
            JwtConfiguration jwtConfig)            
        {
            _jwtTokenRepository = jwtTokenRepository;
            _accountRepository = accountRepository;
            _accountRoleRepository = accountRoleRepository;
            _loggingService = loggingService;
            _jwtConfig = jwtConfig;
            EnsureSymmetricSecretKey();
        }

        public async Task<JwtTokenResponse> GenerateTokensAsync(AccountDto account, List<RoleDto> roles, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var tokenId = Guid.NewGuid().ToString();
                var refreshToken = SecretKeyGenerator.CreateRandomSecretKey();
                var now = DateTime.UtcNow;
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes);
                var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays);
                // Tạo claims
                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                    new(JwtRegisteredClaimNames.Jti, tokenId),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new(JwtRegisteredClaimNames.Email, account.Email),
                    new("username", account.Username),
                    new("email", account.Email),
                    new("display_name", account.DisplayName ?? account.Username ?? string.Empty),
                    new("account_id", account.Id.ToString()),
                    new("expired_at", expiresAt.ToLocalTime().Format(DateFormat.DayMonthYearHour24MinuteSecond)),
                };

                // Add roles and role levels
                var roleCodes = new List<string>();
                var roleLevels = new HashSet<int>();

                foreach (var role in roles)
                {
                    if (!string.IsNullOrWhiteSpace(role.Code))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Code));
                        roleCodes.Add(role.Code);
                    }
                    var roleLevel = role.RoleLevel.GetLevel();
                    roleLevels.Add(roleLevel);
                }

                foreach(var level in roleLevels)
                    claims.Add(new Claim("role_level", level.ToString(), ClaimValueTypes.Integer32));

                // Tạo access token
                var accessToken = CreateAccessToken(claims);

                // Lưu token vào database
                var jwtToken = new JwtTokenEntity
                {
                    AccountId = account.Id,
                    TokenId = tokenId,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    RefreshExpiresAt = refreshExpiresAt,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    CreatedBy = account.Username
                };

                await _jwtTokenRepository.AddAsync(jwtToken);

                // Tạo user info                
                var userInfo = new UserInfo
                {
                    Id = account.Id,
                    Username = account.Username ?? string.Empty,
                    Email = account.Email ?? string.Empty,
                    DisplayName = account.DisplayName ?? account.Username ?? string.Empty,
                    Roles = roleCodes
                };

                return new JwtTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = _jwtConfig.AccessTokenExpirationMinutes * 60,
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error generating JWT tokens for account: {account.Id}",
                    ex,
                    source: "JwtService.GenerateTokensAsync",
                    module: "JWT",
                    action: "GenerateTokens"
                );
                throw;
            }
        }

        public async Task<JwtTokenResponse?> RefreshTokenAsync(string refreshToken, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var tokenRecord = await _jwtTokenRepository.GetByRefreshTokenAsync(refreshToken);
                if (tokenRecord == null || tokenRecord.IsRevoked || tokenRecord.RefreshExpiresAt < DateTime.UtcNow)
                    return null;
                
                var accountEntity = await _accountRepository.GetByIdAsync(tokenRecord.AccountId);
                if (accountEntity == null || !accountEntity.IsActive || accountEntity.IsDeleted)
                    return null;
                
                // Revoke old token
                await _jwtTokenRepository.RevokeTokenAsync(tokenRecord.TokenId);

                // Get account roles from DB
                var roleEntities = await _accountRoleRepository.GetRolesByAccountIdAsync(accountEntity.Id);
                var roles = roleEntities
                    .Where(r => !string.IsNullOrWhiteSpace(r.Code))
                    .Select(r => r.Adapt<RoleDto>())
                    .ToList();

                if (roles.Count == 0)
                {
                    // Fallback: keep at least one role so downstream code doesn't break
                    roles.Add(new RoleDto { Code = RoleLevel.User.GetCode(), RoleLevel = RoleLevel.User });
                }

                // Convert entity to DTO and generate new tokens
                var accountDto = accountEntity.Adapt<AccountDto>();
                return await GenerateTokensAsync(accountDto, roles, ipAddress, userAgent);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error refreshing JWT token",
                    ex,
                    source: "JwtService.RefreshTokenAsync",
                    module: "JWT",
                    action: "RefreshToken"
                );
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string tokenId)
        {
            try
            {
                return await _jwtTokenRepository.RevokeTokenAsync(tokenId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error revoking JWT token: {tokenId}",
                    ex,
                    source: "JwtService.RevokeTokenAsync",
                    module: "JWT",
                    action: "RevokeToken"
                );
                throw;
            }
        }

        public async Task<bool> RevokeAllTokensForAccountAsync(long accountId)
        {
            try
            {
                return await _jwtTokenRepository.RevokeAllTokensForAccountAsync(accountId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error revoking all JWT tokens for account: {accountId}",
                    ex,
                    source: "JwtService.RevokeAllTokensForAccountAsync",
                    module: "JWT",
                    action: "RevokeAllTokens"
                );
                throw;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler
                {
                    MapInboundClaims = false
                };
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // Setup token: stateless, không lưu DB, chỉ cần validate chữ ký + thời gian
                var isSetupToken = jwtToken.Claims.Any(c =>
                    string.Equals(c.Type, "setup_token", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(c.Value, "true", StringComparison.OrdinalIgnoreCase));

                if (isSetupToken)
                {
                    var validationParameters = GetTokenValidationParameters();
                    try
                    {
                        tokenHandler.ValidateToken(token, validationParameters, out SecurityToken _);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                // Token thường: phải tồn tại và còn hiệu lực trong DB
                var isValid = await _jwtTokenRepository.IsTokenValidAsync(jwtToken.Id);
                if (!isValid)
                    return false;

                // Validate token signature và expiration
                var validationParams = GetTokenValidationParameters();
                try
                {
                    tokenHandler.ValidateToken(token, validationParams, out SecurityToken _);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error validating JWT token",
                    ex,
                    source: "JwtService.ValidateTokenAsync",
                    module: "JWT",
                    action: "ValidateToken"
                );
                return false;
            }
        }

        public Task<ClaimsPrincipal?> GetClaimsFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler
                {
                    MapInboundClaims = false  // Keep original JWT claim types (email/sub/role/etc.)
                };
                var validationParameters = GetTokenValidationParameters();
                
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return Task.FromResult<ClaimsPrincipal?>(principal);
            }
            catch (Exception ex)
            {
                _ = _loggingService.LogErrorAsync(
                    "Error getting claims from JWT token",
                    ex,
                    source: "JwtService.GetClaimsFromTokenAsync",
                    module: "JWT",
                    action: "GetClaims"
                );
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        public Task<bool> RotateSecretKeyAsync()
        {
            try
            {
                // Kiểm tra xem có cần rotate key không
                if (DateTime.UtcNow.Subtract(_jwtConfig.SecretKeyLastRotatedAt).TotalDays < _jwtConfig.SecretKeyRotationDays)
                    return Task.FromResult(false);
                
                // Lưu key cũ
                // _jwtConfig.PreviousSecretKey = _jwtConfig.SecretKey;
                
                // Tạo key mới
                _jwtConfig.SecretKey = SecretKeyGenerator.CreateRandomSecretKey(64);
                // _jwtConfig.CurrentSecretKeyVersion++;
                _jwtConfig.SecretKeyLastRotatedAt = DateTime.UtcNow;
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _loggingService.LogErrorAsync(
                    "Error rotating JWT secret key",
                    ex,
                    source: "JwtService.RotateSecretKeyAsync",
                    module: "JWT",
                    action: "RotateKey"
                );
                throw;
            }
        }

        public async Task<int> CleanupExpiredTokensAsync()
        {
            try
            {
                return await _jwtTokenRepository.CleanupExpiredTokensAsync();                
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error cleaning up expired JWT tokens",
                    ex,
                    source: "JwtService.CleanupExpiredTokensAsync",
                    module: "JWT",
                    action: "CleanupTokens"
                );
                throw;
            }
        }

        private string CreateAccessToken(List<Claim> claims)
        {
            var keyBytes = GetSymmetricKeyBytes();
            var securityKey = new SymmetricSecurityKey(keyBytes)
            {
                KeyId = GetKeyId()
            };
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes),
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private void EnsureSymmetricSecretKey()
        {
            lock (_sync)
            {
                if (string.IsNullOrWhiteSpace(_jwtConfig.SecretKey) || _jwtConfig.SecretKey.Length < 32)
                {
                    _jwtConfig.SecretKey = SecretKeyGenerator.CreateRandomSecretKey(64);
                    _jwtConfig.SecretKeyLastRotatedAt = DateTime.UtcNow;
                    if (_jwtConfig.SecretKeyRotationDays <= 0)
                        _jwtConfig.SecretKeyRotationDays = 90;
                }
            }
        }

        private byte[] GetSymmetricKeyBytes()
        {
            if (string.IsNullOrWhiteSpace(_jwtConfig.SecretKey) || _jwtConfig.SecretKey.Length < 32)
                throw new InvalidOperationException("JWT SecretKey is missing or too short.");
            return System.Text.Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            var keyBytes = GetSymmetricKeyBytes();
            var securityKey = new SymmetricSecurityKey(keyBytes)
            {
                KeyId = GetKeyId()
            };
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero
            };
        }

        private string GetKeyId()
        {
            // Generate a consistent KeyId based on the secret key
            // This ensures the same KeyId is used for both token creation and validation
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var keyBytes = GetSymmetricKeyBytes();
                var hash = sha256.ComputeHash(keyBytes);
                return Convert.ToBase64String(hash).Substring(0, 16).Replace("+", "-").Replace("/", "_");
            }
        }
    }
}
