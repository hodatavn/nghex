namespace Nghex.Realtime.SignalR.Models
{
    /// <summary>
    /// Model cho refresh notification
    /// </summary>
    public class RefreshNotification
    {
        public string Type { get; set; } = "refresh";
        public string RefreshType { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime RefreshAt { get; set; } = DateTime.UtcNow;
        public string? ConnectionId { get; set; }
    }
}
