using Nghex.Licensing.Enum;

namespace Nghex.Licensing.Models
{
    /// <summary>
    /// License key information
    /// </summary>
    public class LicenseKeyInfo
    {
        public string LicenseKey { get; set; } = string.Empty;
        public LicenseType LicenseType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public List<string> Features { get; set; } = new();
        public string Checksum { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Organization name embedded in license key (for validation)
        /// </summary>
        public string? OrganizationName { get; set; }
        
        /// <summary>
        /// Location embedded in license key (for validation)
        /// </summary>
        public string? Location { get; set; }
    }
}