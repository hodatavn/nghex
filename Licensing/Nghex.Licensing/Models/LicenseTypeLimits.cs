namespace Nghex.Licensing.Models
{
    /// <summary>
    /// License type limits
    /// </summary>
    public class LicenseTypeLimits
    {
        /// <summary>
        /// Maximum number of users
        /// </summary>
        public int MaxUsers { get; set; }

        /// <summary>
        /// Maximum API calls per day
        /// </summary>
        public int MaxApiCallsPerDay { get; set; }
    }
}

