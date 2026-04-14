using Microsoft.AspNetCore.SignalR;
using Nghex.Logging.Interfaces;

namespace Nghex.Realtime.SignalR.Hubs
{
    /// <summary>
    /// SignalR Hub cho thông báo real-time
    /// </summary>
    public class NotificationHub : Hub
    {
        // private readonly ILoggingService _loggingService;

        // public NotificationHub() //(ILoggingService loggingService)
        // {
        //     // _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        // }

        /// <summary>
        /// Tham gia vào một nhóm cụ thể để nhận thông báo có mục tiêu
        /// </summary>
        /// <param name="groupName">Tên nhóm</param>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoined", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Rời khỏi một nhóm cụ thể
        /// </summary>
        /// <param name="groupName">Tên nhóm</param>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeft", Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Gửi tin nhắn đến tất cả client đang kết nối
        /// </summary>
        /// <param name="message">Tin nhắn cần gửi</param>
        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        /// <summary>
        /// Gửi tin nhắn đến một nhóm cụ thể
        /// </summary>
        /// <param name="groupName">Nhóm đích</param>
        /// <param name="message">Tin nhắn cần gửi</param>
        public async Task SendMessageToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }

        /// <summary>
        /// Gửi thông báo refresh đến tất cả client
        /// </summary>
        /// <param name="refreshType">Loại refresh (ví dụ: "data", "ui", "config")</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        public async Task SendRefreshNotification(string refreshType, object? data = null)
        {
            var notification = new
            {
                Type = "refresh",
                RefreshType = refreshType,
                Data = data,
                Timestamp = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId
            };
            await Clients.All.SendAsync("ReceiveRefreshNotification", notification);
        }

        /// <summary>
        /// Gửi thông báo refresh đến một nhóm cụ thể
        /// </summary>
        /// <param name="groupName">Nhóm đích</param>
        /// <param name="refreshType">Loại refresh</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        public async Task SendRefreshNotificationToGroup(string groupName, string refreshType, object? data = null)
        {
            var notification = new
            {
                Type = "refresh",
                RefreshType = refreshType,
                Data = data,
                Timestamp = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId
            };

            await Clients.Group(groupName).SendAsync("ReceiveRefreshNotification", notification);
        }

        /// <summary>
        /// Gửi thông báo refresh đến một user cụ thể
        /// </summary>
        /// <param name="userId">ID của user đích</param>
        /// <param name="refreshType">Loại refresh</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        public async Task SendRefreshNotificationToUser(string userId, string refreshType, object? data = null)
        {
            var notification = new
            {
                Type = "refresh",
                RefreshType = refreshType,
                Data = data,
                Timestamp = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId
            };
            await Clients.User(userId).SendAsync("ReceiveRefreshNotification", notification);
        }

        /// <summary>
        /// Gửi thông báo cập nhật dữ liệu
        /// </summary>
        /// <param name="entityType">Loại entity được cập nhật</param>
        /// <param name="entityId">ID của entity</param>
        /// <param name="action">Hành động thực hiện (create, update, delete)</param>
        public async Task SendDataUpdate(string entityType, long entityId, string action)
        {
            var data = new
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("ReceiveDataUpdate", data);
        }

        /// <summary>
        /// Gửi thông báo cập nhật UI
        /// </summary>
        /// <param name="component">Component cần refresh</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        public async Task SendUIUpdate(string component, object? data = null)
        {
            var uiData = new
            {
                Component = component,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("ReceiveUIUpdate", uiData);
        }

        /// <summary>
        /// Được gọi khi client kết nối
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Được gọi khi client ngắt kết nối
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
