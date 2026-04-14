namespace Nghex.Configuration.Api.Models
{
    /// <summary>
    /// Request model cho ImportConfigurations
    /// </summary>
    public class ImportConfigurationsRequest
    {
        public string JsonData { get; set; } = string.Empty;
        public string? Module { get; set; }
    }

}