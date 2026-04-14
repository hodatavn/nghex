using Nghex.Realtime.SignalR.Enum;

namespace Nghex.Realtime.SignalR.Models
{
    /// <summary>
    /// Model cho group membership
    /// </summary>
    public class GroupMembership
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public GroupMemberAction Action { get; set; } = GroupMemberAction.Left;
        public DateTime ActionAt { get; set; } = DateTime.UtcNow;
    }
}
