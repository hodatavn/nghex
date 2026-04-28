namespace Nghex.Plugins.Models
{
    public class PluginInstallResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<PluginResponseModel> Plugins { get; set; } = [];
    }
}
