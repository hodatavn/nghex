namespace Nghex.Identity.Models
{
    /// <summary>
    /// Login tracking information model
    /// </summary>
    public class LoginTrackingInfo
    {
        /// <summary>
        /// Last login date
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Last login IP address
        /// </summary>
        public string? LastLoginIp { get; set; }

        /// <summary>
        /// Last login user agent
        /// </summary>
        public string? LastLoginUserAgent { get; set; }

        /// <summary>
        /// Failed login attempts count
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// Last failed login attempt IP
        /// </summary>
        public string? LastFailedLoginIp { get; set; }

        /// <summary>
        /// Last failed login attempt date
        /// </summary>
        public DateTime? LastFailedLoginAt { get; set; }

        /// <summary>
        /// Account locked until
        /// </summary>
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// Is account currently locked
        /// </summary>
        public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
    }
}
