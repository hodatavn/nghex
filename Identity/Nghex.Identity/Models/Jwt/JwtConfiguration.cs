namespace Nghex.Identity.Models
{
    /// <summary>
    /// JWT Configuration model
    /// </summary>
    public class JwtConfiguration
    {
        /// <summary>
        /// Issuer (iss)
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Audience (aud)
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Secret key for signing
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time in minutes
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Refresh token expiration time in days
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 30;

        /// <summary>
        /// Secret key rotation period in days
        /// </summary>
        public int SecretKeyRotationDays { get; set; } = 30;

        // /// <summary>
        // /// Current secret key version
        // /// </summary>
        // public int CurrentSecretKeyVersion { get; set; } = 1;

        // /// <summary>
        // /// Previous secret key for validation during rotation
        // /// </summary>
        // public string? PreviousSecretKey { get; set; }

        /// <summary>
        /// Secret key last rotated at
        /// </summary>
        public DateTime SecretKeyLastRotatedAt { get; set; } = DateTime.UtcNow;
    }
}
