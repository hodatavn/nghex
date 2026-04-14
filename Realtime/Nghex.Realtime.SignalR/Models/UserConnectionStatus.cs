using Nghex.Realtime.SignalR.Enum;

namespace Nghex.Realtime.SignalR.Models
{
    /// <summary>
    /// Model cho user connection status
    /// </summary>
    public class UserConnectionStatus
    {
        public string ConnectionId { get; set; } = string.Empty;
        public UserConnectionState Status { get; set; } = UserConnectionState.Disconnected;
        public DateTime UserConnectAt { get; set; } = DateTime.UtcNow;
    }

}
