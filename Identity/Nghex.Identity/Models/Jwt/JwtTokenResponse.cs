using System.Text.Json.Serialization;

namespace Nghex.Identity.Models
{
    /// <summary>
    /// JWT Token response model
    /// </summary>
    public class JwtTokenResponse
    {
        /// <summary>
        /// Access token
        /// </summary>
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type
        /// </summary>
        [JsonPropertyName("tokenType")]
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Expires in seconds
        /// </summary>
        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// User information
        /// </summary>
        [JsonPropertyName("user")]
        public UserInfo? User { get; set; }
    }

    /// <summary>
    /// User information model
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// User ID
        /// </summary>
        [JsonPropertyName("id")]
        public long Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// User roles
        /// </summary>
        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new List<string>();

    }
}
