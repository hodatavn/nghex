using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nghex.Logging.Interfaces;
using Nghex.Identity.Middleware.Extensions;
using Nghex.Identity.Api.Services;
using Nghex.Identity.Services.Interfaces;

namespace Nghex.Identity.Api.Controllers
{
    /// <summary>
    /// JWT Management Controller (Admin only)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class JwtController(
        IJwtService jwtService,
        ILoggingService loggingService) : ControllerBase
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly ILoggingService _loggingService = loggingService;

        /// <summary>
        /// Rotate JWT secret key manually
        /// </summary>
        [HttpPost("rotate-key")]
        public async Task<IActionResult> RotateSecretKey()
        {
            try
            {
                var result = await _jwtService.RotateSecretKeyAsync();                
                if (result)
                    return Ok(new { message = "Secret key rotated successfully" });
                return BadRequest(new { message = "Secret key rotation not needed at this time" });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error rotating JWT secret key",
                    ex,
                    source: "JwtController.RotateSecretKey",
                    module: "JWT",
                    action: "RotateKey",
                    details: new { AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cleanup expired tokens manually
        /// </summary>
        [HttpPost("cleanup-tokens")]
        public async Task<IActionResult> CleanupExpiredTokens()
        {
            try
            {
                var cleanedCount = await _jwtService.CleanupExpiredTokensAsync();                
                return Ok(new { message = $"Cleaned up {cleanedCount} expired tokens" });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error cleaning up expired JWT tokens",
                    ex,
                    source: "JwtController.CleanupExpiredTokens",
                    module: "JWT",
                    action: "CleanupTokens",
                    details: new { AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Revoke specific token
        /// </summary>
        [HttpPost("revoke-token/{tokenId}")]
        public async Task<IActionResult> RevokeToken(string tokenId)
        {
            try
            {
                var result = await _jwtService.RevokeTokenAsync(tokenId);
                if (result)                                    
                    return Ok(new { message = "Token revoked successfully" });
                return NotFound(new { message = "Token not found" });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error revoking JWT token: {tokenId}",
                    ex,
                    source: "JwtController.RevokeToken",
                    module: "JWT",
                    action: "RevokeToken",
                    details: new { TokenId = tokenId, AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Revoke all tokens for specific account
        /// </summary>
        [HttpPost("revoke-all-tokens/{accountId}")]
        public async Task<IActionResult> RevokeAllTokensForAccount(long accountId)
        {
            try
            {
                var result = await _jwtService.RevokeAllTokensForAccountAsync(accountId);
                if (result)                                    
                    return Ok(new { message = "All tokens revoked successfully" });
                return NotFound(new { message = "Account not found" });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error revoking all JWT tokens for account: {accountId}",
                    ex,
                    source: "JwtController.RevokeAllTokensForAccount",
                    module: "JWT",
                    action: "RevokeAllTokens",
                    details: new { TargetAccountId = accountId, AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate token
        /// </summary>
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                    return BadRequest(new { message = "Token is required" });
                var isValid = await _jwtService.ValidateTokenAsync(request.Token);
                
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error validating JWT token",
                    ex,
                    source: "JwtController.ValidateToken",
                    module: "JWT",
                    action: "ValidateToken",
                    details: new { AccountId = User.GetUserId() }
                );
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Validate Token Request Model
    /// </summary>
    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
