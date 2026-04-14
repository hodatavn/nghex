namespace Nghex.Licensing.Api.Models
{
    /// <summary>
    /// Create trial license request model
    /// </summary>
    public class CreateTrialRequest
    {
        public int TrialDays { get; set; } = 45;
    }
}