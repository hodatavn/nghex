using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Nghex.Identity.Models;
using Nghex.Data.Setup;
using Nghex.Identity.Services.Interfaces;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Api.Services;
using Nghex.Identity.Api.Models.Account;

namespace Nghex.Identity.Api.Controllers
{
    /// <summary>
    /// Authentication Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController(
        IAccountService accountService,
        IAuthManagementService authManagementService,
        IJwtService jwtService,
        ISetupMenuService setupMenuService,
        IMenuService menuService,
        ILoggingService loggingService,
        IHttpContextService httpContextService,
        AuthCookieConfiguration cookieConfig,
        JwtConfiguration jwtConfig,
        ISystemInitializationState initState) : ControllerBase
    {
        private readonly IAccountService _accountService = accountService;
        private readonly IAuthManagementService _authManagementService = authManagementService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly ILoggingService _loggingService = loggingService;
        private readonly IHttpContextService _httpContextService = httpContextService;
        private readonly AuthCookieConfiguration _cookieConfig = cookieConfig;
        private readonly JwtConfiguration _jwtConfig = jwtConfig;
        private readonly ISystemInitializationState _initState = initState;
        private readonly ISetupMenuService _setupMenuService = setupMenuService;
        private readonly IMenuService _menuService = menuService;

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }
                if(string.IsNullOrEmpty(request.IpAddress))
                    request.IpAddress = _httpContextService.GetClientIpAddress();

                var initialized = _initState.IsInitialized;

                // Setup token login: allow user "system" ONLY when DB/schema is NOT initialized.
                // This enables accessing setup UI without requiring SYS_* tables.
                if (!initialized)
                {
                    var isSetupTokenValid = await _accountService.ValidateSetupCredentialsAsync(request.Username, request.Password);
                    if (isSetupTokenValid)
                    {
                        var userAgentSetup = _httpContextService.GetUserAgent();
                        var setupTokenResponse = CreateSetupTokenResponse(
                            username: request.Username,
                            roles: ["SETUP_SYSTEM"],
                            ipAddress: request.IpAddress,
                            userAgent: userAgentSetup);

                        SetAuthCookies(setupTokenResponse);

                        if (_cookieConfig.ReturnTokensInBody)
                            return Ok(setupTokenResponse);

                        return Ok(new
                        {
                            tokenType = setupTokenResponse.TokenType,
                            expiresIn = setupTokenResponse.ExpiresIn,
                            user = setupTokenResponse.User
                        });
                    }
                    // Block any non-setup token login before DB is initialized.
                    return Unauthorized(new { message = "System is not initialized. Use setup account to setup database." });
                }

                // After initialization, disallow setup token account login.
                if (string.Equals(request.Username, "setup", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { message = "Setup account is disabled after system initialization." });

                // Authenticate user - License được kiểm tra trong AccountService.AuthenticateAsync
                try
                {
                    var account = await _accountService.AuthenticateAsync(request.Username, request.Password, request.IpAddress);
                    var userAgent = _httpContextService.GetUserAgent();
                    if (account == null)
                        return Unauthorized(new { message = "Invalid username or password" });
                    var rolesOfAccount = await _authManagementService.GetRolesOfAccountAsync(account.Id);
                    
                    // Generate JWT tokens
                    var tokenResponse = await _jwtService.GenerateTokensAsync(account, [.. rolesOfAccount], request.IpAddress, userAgent);

                    SetAuthCookies(tokenResponse);

                    if (_cookieConfig.ReturnTokensInBody)
                        return Ok(tokenResponse);

                    return Ok(new
                    {
                        tokenType = tokenResponse.TokenType,
                        expiresIn = tokenResponse.ExpiresIn,
                        user = tokenResponse.User
                    });

                }
                catch (Exception ex)
                {
                    return Unauthorized(new {message = ex.Message});
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Login error for username: {request.Username}",
                    ex,
                    source: "AuthController.Login",
                    module: "Authentication",
                    action: "Login",
                    details: new { Username = request.Username }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        /// <summary>
        /// Refresh token
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestModel request)
        {
            try
            {
                // Accept refresh token from body (legacy) OR from HttpOnly cookie (recommended)
                var refreshToken = request.RefreshToken;
                if (string.IsNullOrWhiteSpace(refreshToken))
                    refreshToken = GetCookie(_cookieConfig.RefreshTokenCookieName);

                if (string.IsNullOrWhiteSpace(refreshToken))
                    return BadRequest(new { message = "Refresh token is required" });

                var ipAddress = _httpContextService.GetClientIpAddress();
                var userAgent = _httpContextService.GetUserAgent();
                var tokenResponse = await _jwtService.RefreshTokenAsync(refreshToken, ipAddress, userAgent);

                if (tokenResponse == null)
                {
                    await _loggingService.LogWarningAsync(
                        $"Token refresh failed",
                        source: "AuthController.RefreshToken",
                        module: "Authentication",
                        action: "RefreshToken",
                        details: new { IPAddress = ipAddress }
                    );
                    return Unauthorized(new { message = "Invalid refresh token" });
                }

                SetAuthCookies(tokenResponse);

                if (_cookieConfig.ReturnTokensInBody)
                    return Ok(tokenResponse);

                return Ok(new
                {
                    tokenType = tokenResponse.TokenType,
                    expiresIn = tokenResponse.ExpiresIn,
                    user = tokenResponse.User
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Token refresh error",
                    ex,
                    source: "AuthController.RefreshToken",
                    module: "Authentication",
                    action: "RefreshToken"
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // If we still have a valid principal, revoke by access token jti.
                var tokenId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrWhiteSpace(tokenId))
                    await _jwtService.RevokeTokenAsync(tokenId);
                else
                {
                    // If access token is missing/expired, revoke by refresh token cookie.
                    var refreshToken = GetCookie(_cookieConfig.RefreshTokenCookieName);
                    if (!string.IsNullOrWhiteSpace(refreshToken))
                        await _jwtService.RevokeTokenAsync(refreshToken);
                }

                ClearAuthCookies();
                return Ok(new { message = "Logout successful" });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Logout error",
                    ex,
                    source: "AuthController.Logout",
                    module: "Authentication",
                    action: "Logout",
                    details: new { AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Đăng xuất tất cả thiết bị
        /// </summary>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            try
            {
                var accountId = User.GetUserId();
                if (accountId.HasValue)
                {
                    await _jwtService.RevokeAllTokensForAccountAsync(accountId.Value);
                }

                ClearAuthCookies();
                return Ok(new { message = "Logout from all devices successful" });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Logout all devices error",
                    ex,
                    source: "AuthController.LogoutAll",
                    module: "Authentication",
                    action: "LogoutAll",
                    details: new { AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        /// <summary>
        /// Get filtered menu tree for current user.
        /// </summary>
        [HttpGet("me/menu")]
        [Authorize]
        public async Task<IActionResult> GetMenu()
        {
            var accountId = User.GetUserId();
            if (!accountId.HasValue || accountId.Value <= 0)
            {
                // Allow setup user (account_id=0) when claim setup_token=true
                var setupToken = User.FindFirst("setup_token")?.Value;
                if (string.Equals(setupToken, "true", StringComparison.OrdinalIgnoreCase))
                {
                    var setupMenu = await _setupMenuService.GetSetupMenuAsync();
                    return Ok(setupMenu);
                }

                return Unauthorized(new { message = "User not authenticated" });
            }
            var permissionCodes = (await _authManagementService.GetPermissionsOfAccountAsync(accountId.Value))
                                    .Select(p => p.Code);

            var menu = await _menuService.GetMenuTreeFromPermissionsAsync(permissionCodes);
            return Ok(menu);
        }


        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        [HttpGet("me")]
        // [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userInfo = new UserInfo
                {
                    Id = User.GetUserId() ?? 0,
                    Username = User.GetUsername() ?? "",
                    Email = User.GetEmail() ?? "",
                    DisplayName = User.GetDisplayName(),
                    Roles = User.GetRoles()
                };

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error getting current user info",
                    ex,
                    source: "AuthController.GetCurrentUser",
                    module: "Authentication",
                    action: "GetCurrentUser",
                    details: new { AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }


        /// <summary>
        /// Lấy thông tin login tracking của user hiện tại
        /// </summary>
        [HttpGet("login-tracking")]
        [Authorize]
        public async Task<IActionResult> GetLoginTrackingInfo()
        {
            try
            {
                var username = User.GetUsername();
                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "User not authenticated" });

                var trackingInfo = await _accountService.GetLoginTrackingInfoAsync(username);
                if (string.IsNullOrEmpty(trackingInfo.IpAddress))
                    return NotFound(new { message = "Login tracking info not found" });

                return Ok(trackingInfo);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting login tracking info",
                    ex,
                    source: "AuthController.GetLoginTrackingInfo",
                    module: "Authentication",
                    action: "GetLoginTrackingInfo",
                    details: new { AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private void SetAuthCookies(JwtTokenResponse tokenResponse)
        {
            var now = DateTimeOffset.UtcNow;
            var sameSite = ParseSameSite(_cookieConfig.SameSite);

            // Access token (HttpOnly)
            Response.Cookies.Append(
                _cookieConfig.AccessTokenCookieName,
                tokenResponse.AccessToken,
                CreateCookieOptions(
                    httpOnly: true,
                    sameSite: sameSite,
                    path: _cookieConfig.AccessTokenPath,
                    expires: now.AddSeconds(Math.Max(1, tokenResponse.ExpiresIn))
                )
            );

            // Refresh token (HttpOnly)
            // IMPORTANT: cleanup legacy refresh cookie path to avoid having two cookies with same name but different paths.
            // If both exist, the browser may send an unexpected one depending on request path, causing multiple refresh chains.
            Response.Cookies.Delete(
                _cookieConfig.RefreshTokenCookieName,
                CreateCookieOptions(httpOnly: true, sameSite: sameSite, path: "/api/Auth", expires: now.AddYears(-1))
            );
            Response.Cookies.Append(
                _cookieConfig.RefreshTokenCookieName,
                tokenResponse.RefreshToken,
                CreateCookieOptions(
                    httpOnly: true,
                    sameSite: sameSite,
                    path: _cookieConfig.RefreshTokenPath,
                    expires: now.AddDays(Math.Max(1, _jwtConfig.RefreshTokenExpirationDays))
                )
            );

            // CSRF token (readable by client)
            var csrfToken = Guid.NewGuid().ToString("N");
            Response.Cookies.Append(
                _cookieConfig.CsrfCookieName,
                csrfToken,
                CreateCookieOptions(
                    httpOnly: false,
                    sameSite: sameSite,
                    path: _cookieConfig.CsrfPath,
                    expires: now.AddDays(Math.Max(1, _jwtConfig.RefreshTokenExpirationDays))
                )
            );
        }

        private void ClearAuthCookies()
        {
            var sameSite = ParseSameSite(_cookieConfig.SameSite);
            var expired = DateTimeOffset.UtcNow.AddYears(-1);

            Response.Cookies.Delete(_cookieConfig.AccessTokenCookieName,
                CreateCookieOptions(httpOnly: true, sameSite: sameSite, path: _cookieConfig.AccessTokenPath, expires: expired));

            Response.Cookies.Delete(_cookieConfig.RefreshTokenCookieName,
                CreateCookieOptions(httpOnly: true, sameSite: sameSite, path: _cookieConfig.RefreshTokenPath, expires: expired));

            Response.Cookies.Delete(_cookieConfig.CsrfCookieName,
                CreateCookieOptions(httpOnly: false, sameSite: sameSite, path: _cookieConfig.CsrfPath, expires: expired));
        }

        private string? GetCookie(string name)
        {
            return Request.Cookies.TryGetValue(name, out var val) ? val : null;
        }

        private CookieOptions CreateCookieOptions(bool httpOnly, SameSiteMode sameSite, string path, DateTimeOffset expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = httpOnly,
                Secure = _cookieConfig.Secure,
                SameSite = sameSite,
                Path = string.IsNullOrWhiteSpace(path) ? "/" : path,
                Expires = expires,
                IsEssential = true
            };

            if (!string.IsNullOrWhiteSpace(_cookieConfig.Domain))
                options.Domain = _cookieConfig.Domain;

            return options;
        }

        private static SameSiteMode ParseSameSite(string? sameSite)
        {
            if (string.IsNullOrWhiteSpace(sameSite))
                return SameSiteMode.Lax;

            return sameSite.Trim().ToLowerInvariant() switch
            {
                "none" => SameSiteMode.None,
                "strict" => SameSiteMode.Strict,
                "lax" => SameSiteMode.Lax,
                _ => SameSiteMode.Lax
            };
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

        private JwtTokenResponse CreateSetupTokenResponse(string username, List<string> roles, string? ipAddress, string? userAgent)
        {
            // Create a stateless JWT token (no DB writes) for setup token only
            // Setup workflow thường cần nhiều thời gian hơn, nên đảm bảo thời gian sống tối thiểu đủ dài.
            // Nếu cấu hình AccessTokenExpirationMinutes đang quá nhỏ, ta ép tối thiểu 180 phút cho setup.
            var tokenId = Guid.NewGuid().ToString();
            var refreshToken = Guid.NewGuid().ToString("N");
            var setupExpiresMinutes = _jwtConfig.AccessTokenExpirationMinutes;
            if (setupExpiresMinutes < 180)
                setupExpiresMinutes = 180;
            var expiresAt = DateTime.Now.AddMinutes(setupExpiresMinutes);

            if (string.IsNullOrWhiteSpace(_jwtConfig.SecretKey) || _jwtConfig.SecretKey.Length < 32)
                throw new InvalidOperationException("JWT SecretKey is missing or too short for setup account login.");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, "0"),
                new(JwtRegisteredClaimNames.Jti, tokenId),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new("username", username),
                new("email", ""),
                new("display_name", "System"),
                new("account_id", "0"),
                new("setup_token", "true"),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var keyBytes = Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);
            var securityKey = new SymmetricSecurityKey(keyBytes)
            {
                KeyId = GetKeyId(_jwtConfig.SecretKey)
            };
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return new JwtTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = setupExpiresMinutes * 60,
                User = new UserInfo
                {
                    Id = 0,
                    Username = username,
                    Email = "",
                    DisplayName = "System",
                    Roles = roles
                }
            };
        }
    }

}
