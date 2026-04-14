namespace Nghex.Licensing.Models
{
    /// <summary>
    /// License validation result
    /// </summary>
    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public LicenseInfo? License { get; set; }
        public int DaysRemaining { get; set; }
        
        /// <summary>
        /// Is license in grace period (expired but still allowed)
        /// </summary>
        public bool IsInGracePeriod { get; set; }
        
        /// <summary>
        /// Days remaining in grace period
        /// </summary>
        public int GracePeriodDaysRemaining { get; set; }
        
        /// <summary>
        /// Should show warning (approaching expiry)
        /// </summary>
        public bool ShouldShowWarning { get; set; }
        
        /// <summary>
        /// Warning message
        /// </summary>
        public string? WarningMessage { get; set; }
    }
}

