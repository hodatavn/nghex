using Microsoft.AspNetCore.SignalR;
using Nghex.Realtime.SignalR.Hubs;
using Nghex.Logging.Interfaces;

namespace Nghex.Realtime.SignalR.Services
{
    /// <summary>
    /// Interface cho notification service
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Gửi thông báo refresh đến tất cả client đang kết nối
        /// </summary>
        /// <param name="refreshType">Loại refresh</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        Task SendRefreshToAllAsync(string refreshType, object? data = null);

        /// <summary>
        /// Gửi thông báo refresh đến một nhóm cụ thể
        /// </summary>
        /// <param name="groupName">Tên nhóm đích</param>
        /// <param name="refreshType">Loại refresh</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        Task SendRefreshToGroupAsync(string groupName, string refreshType, object? data = null);

        /// <summary>
        /// Gửi thông báo refresh đến một user cụ thể
        /// </summary>
        /// <param name="userId">ID của user đích</param>
        /// <param name="refreshType">Loại refresh</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        Task SendRefreshToUserAsync(string userId, string refreshType, object? data = null);

        /// <summary>
        /// Gửi thông báo cập nhật dữ liệu
        /// </summary>
        /// <param name="entityType">Loại entity được cập nhật</param>
        /// <param name="entityId">ID của entity</param>
        /// <param name="action">Hành động thực hiện (create, update, delete)</param>
        Task SendDataUpdateAsync(string entityType, long entityId, string action);

        /// <summary>
        /// Gửi thông báo cập nhật UI
        /// </summary>
        /// <param name="component">Component cần refresh</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        Task SendUIUpdateAsync(string component, object? data = null);

        /// <summary>
        /// Gửi thông báo cập nhật cấu hình
        /// </summary>
        /// <param name="configType">Loại cấu hình</param>
        /// <param name="data">Dữ liệu tùy chọn</param>
        Task SendConfigUpdateAsync(string configType, object? data = null);

        /// <summary>
        /// Gửi tin nhắn đến tất cả client
        /// </summary>
        /// <param name="message">Tin nhắn</param>
        Task SendMessageToAllAsync(string message);

        /// <summary>
        /// Gửi tin nhắn đến một nhóm
        /// </summary>
        /// <param name="groupName">Tên nhóm</param>
        /// <param name="message">Tin nhắn</param>
        Task SendMessageToGroupAsync(string groupName, string message);
    }

    /// <summary>
    /// Implementation của notification service sử dụng SignalR
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILoggingService _loggingService;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILoggingService loggingService)
        {
            _hubContext = hubContext;
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public async Task SendRefreshToAllAsync(string refreshType, object? data = null)
        {
            try
            {
                var notification = new
                {
                    Type = "refresh",
                    RefreshType = refreshType,
                    Data = data,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveRefreshNotification", notification);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to send refresh notification to all clients",
                    ex,
                    source: "NotificationService.SendRefreshToAllAsync",
                    module: "SignalR",
                    action: "SendRefreshToAll"
                );
                throw;
            }
        }

        public async Task SendRefreshToGroupAsync(string groupName, string refreshType, object? data = null)
        {
            try
            {
                var notification = new
                {
                    Type = "refresh",
                    RefreshType = refreshType,
                    Data = data,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveRefreshNotification", notification);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to send refresh notification to group {groupName}",
                    ex,
                    source: "NotificationService.SendRefreshToGroupAsync",
                    module: "SignalR",
                    action: "SendRefreshToGroup",
                    details: new { GroupName = groupName }
                );
                throw;
            }
        }

        public async Task SendRefreshToUserAsync(string userId, string refreshType, object? data = null)
        {
            try
            {
                var notification = new
                {
                    Type = "refresh",
                    RefreshType = refreshType,
                    Data = data,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.User(userId).SendAsync("ReceiveRefreshNotification", notification);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to send refresh notification to user {userId}",
                    ex,
                    source: "NotificationService.SendRefreshToUserAsync",
                    module: "SignalR",
                    action: "SendRefreshToUser",
                    details: new { UserId = userId }
                );
                throw;
            }
        }

        public async Task SendDataUpdateAsync(string entityType, long entityId, string action)
        {
            try
            {
                var data = new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveDataUpdate", data);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to send data update notification",
                    ex,
                    source: "NotificationService.SendDataUpdateAsync",
                    module: "SignalR",
                    action: "SendDataUpdate"
                );
                throw;
            }
        }

        public async Task SendUIUpdateAsync(string component, object? data = null)
        {
            try
            {
                var uiData = new
                {
                    Component = component,
                    Data = data,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveUIUpdate", uiData);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to send UI update notification",
                    ex,
                    source: "NotificationService.SendUIUpdateAsync",
                    module: "SignalR",
                    action: "SendUIUpdate"
                );
                throw;
            }
        }

        public async Task SendConfigUpdateAsync(string configType, object? data = null)
        {
            try
            {
                var configData = new
                {
                    ConfigType = configType,
                    Data = data,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveConfigUpdate", configData);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to send config update notification",
                    ex,
                    source: "NotificationService.SendConfigUpdateAsync",
                    module: "SignalR",
                    action: "SendConfigUpdate"
                );
                throw;
            }
        }

        public async Task SendMessageToAllAsync(string message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Failed to send message to all clients",
                    ex,
                    source: "NotificationService.SendMessageToAllAsync",
                    module: "SignalR",
                    action: "SendMessageToAll"
                );
                throw;
            }
        }

        public async Task SendMessageToGroupAsync(string groupName, string message)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Failed to send message to group {groupName}",
                    ex,
                    source: "NotificationService.SendMessageToGroupAsync",
                    module: "SignalR",
                    action: "SendMessageToGroup",
                    details: new { GroupName = groupName }
                );
                throw;
            }
        }
    }
}
