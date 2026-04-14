namespace Nghex.Realtime.SignalR.Models
{
    /// <summary>
    /// Model cho UI update notification
    /// </summary>
    public class UIUpdateNotification
    {
        public string Component { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime UIUpdateAt { get; set; } = DateTime.UtcNow;
    }

}
