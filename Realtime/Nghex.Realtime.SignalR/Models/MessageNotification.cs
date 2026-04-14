namespace Nghex.Realtime.SignalR.Models
{
    /// <summary>
    /// Model cho message notification
    /// </summary>
    public class MessageNotification
    {
        public string Message { get; set; } = string.Empty;
        public DateTime SendMessageAt { get; set; } = DateTime.UtcNow;
        public string? SenderId { get; set; }
    }
}
