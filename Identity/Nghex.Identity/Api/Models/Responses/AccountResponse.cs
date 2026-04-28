namespace Nghex.Identity.Api.Models.Responses
{
    /// <summary>
    /// Response model for Account
    /// </summary>
    public class AccountResponse
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public long AccountId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is locked
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Last login at
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Failed login attempts
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// Locked until
        /// </summary>
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// Additional data
        /// </summary>
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
