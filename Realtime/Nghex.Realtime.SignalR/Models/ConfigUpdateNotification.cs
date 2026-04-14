namespace Nghex.Realtime.SignalR.Models
{
    /// <summary>
    /// Model cho config update notification
    /// </summary>
    public class ConfigUpdateNotification
    {
        public string ConfigType { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime ConfigUpdateAt { get; set; } = DateTime.UtcNow;
    }
}
