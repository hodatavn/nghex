using Nghex.Plugins.DTOs;

namespace Nghex.Plugins.Models
{
    /// <summary>
    /// Plugin installation result
    /// </summary>
    public class PluginInstallResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<PluginDto> Plugins { get; set; } = [];
    }
}
