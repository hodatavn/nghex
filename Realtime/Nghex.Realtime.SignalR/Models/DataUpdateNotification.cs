namespace Nghex.Realtime.SignalR.Models
{
    /// <summary>
    /// Model cho data update notification
    /// </summary>
    public class DataUpdateNotification
    {
        public string EntityType { get; set; } = string.Empty;
        public long EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime DataUpdateAt { get; set; } = DateTime.UtcNow;
    }

}
