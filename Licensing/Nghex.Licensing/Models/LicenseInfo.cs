
using Nghex.Licensing.Enum;

namespace Nghex.Licensing.Models
{
    /// <summary>
    /// License information model - Based on Who, Where, When, What
    /// </summary>
    public class LicenseInfo
    {
        public string LicenseKey { get; set; } = string.Empty;
        public LicenseType LicenseType { get; set; }
        
        public string OrganizationName { get; set; } = string.Empty;

        public string? ServerIdentifier { get; set; } 
        
        public DateTime IssuedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        
        public List<string> Features { get; set; } = []; // Features list, "All" means all features
        public int MaxUsers { get; set; }
        public int MaxApiCallsPerDay { get; set; }
        
        // Status
        public bool IsActive { get; set; }
        public DateTime LastValidated { get; set; }
        public int ValidationCount { get; set; }
        
        /// <summary>
        /// Check if license has "All" feature (means all features are enabled)
        /// </summary>
        public bool HasAllFeatures()
        {
            return Features.Contains("All", StringComparer.OrdinalIgnoreCase);
        }
    }
}


