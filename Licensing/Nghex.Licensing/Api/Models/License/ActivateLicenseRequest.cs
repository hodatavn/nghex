namespace Nghex.Licensing.Api.Models
{
    /// <summary>
    /// Activate license request model
    /// </summary>
    public class ActivateLicenseRequest
    {
        /// <summary>
        /// License key
        /// </summary>
        public string LicenseKey { get; set; } = string.Empty;
        
        /// <summary>
        /// Organization name (required)
        /// </summary>
        public string OrganizationName { get; set; } = string.Empty;
        
    }
}