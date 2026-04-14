namespace Nghex.Licensing.Models
{
    /// <summary>
    /// License limits configuration
    /// </summary>
    public class LicenseLimits
    {
        /// <summary>
        /// Trial license limits
        /// </summary>
        public LicenseTypeLimits Trial { get; set; } = new() { MaxUsers = 5, MaxApiCallsPerDay = 100000 };

        /// <summary>
        /// Standard license limits
        /// </summary>
        public LicenseTypeLimits Standard { get; set; } = new() { MaxUsers = 50, MaxApiCallsPerDay = 1000000 };

        /// <summary>
        /// Professional license limits
        /// </summary>
        public LicenseTypeLimits Professional { get; set; } = new() { MaxUsers = 200, MaxApiCallsPerDay = 10000000 };

        /// <summary>
        /// Enterprise license limits
        /// </summary>
        public LicenseTypeLimits Enterprise { get; set; } = new() { MaxUsers = int.MaxValue, MaxApiCallsPerDay = int.MaxValue };
    }

}

