using Nghex.Core.Setting;

namespace Nghex.Licensing.Models
{
    /// <summary>
    /// License settings model for JSON file
    /// </summary>
    public class LicenseSettings
    {
        /// <summary>
        /// Default trial days when creating trial license
        /// </summary>
        public int TrialDays { get; set; } = 45;

        /// <summary>
        /// License file path (relative to application root)
        /// </summary>
        public Nghex.Core.Setting.LicenseFile? LicenseFile { get; set; } = null;

        /// <summary>
        /// Enable location-based binding
        /// </summary>
        public bool EnableLocationBinding { get; set; } = true;

        /// <summary>
        /// Enable organization validation
        /// </summary>
        public bool EnableOrganizationValidation { get; set; } = false;

        /// <summary>
        /// Allowed locations (whitelist - optional, empty means all locations allowed)
        /// </summary>
        public List<string> AllowedLocations { get; set; } = [];

        /// <summary>
        /// Allowed environments
        /// </summary>
        public List<string> AllowedEnvironments { get; set; } = ["Production", "UAT", "Development"];

        /// <summary>
        /// Validation interval in minutes
        /// </summary>
        public int ValidationIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// Grace period days after license expiry (allow system to continue working)
        /// </summary>
        public int GracePeriodDays { get; set; } = 7;

        /// <summary>
        /// Days before expiry to show warning (should be same as GracePeriodDays)
        /// </summary>
        public int WarningDaysBeforeExpiry { get; set; } = 7;

        /// <summary>
        /// Location-based trial tracking: Dictionary of Location -> FirstTrialDate
        /// </summary>
        public Dictionary<string, DateTime> LocationTrialDates { get; set; } = new();

        /// <summary>
        /// License limits configuration by license type
        /// </summary>
        public LicenseLimits Limits { get; set; } = new();
    }
}

