
using System.Text;
using System.Text.Json;
using Oracle.ManagedDataAccess.Client;
using Nghex.Data;
using Nghex.Logging;
using Nghex.Logging.Models;
using System.Text.Json.Serialization;

namespace Nghex.Realtime.SignalR.Services
{
    /// <summary>
    /// Background service listening to Oracle Advanced Queuing (AQ)
    /// with "One Queue - Many Tables" model to send notifications to SignalR clients.
    /// 
    /// DB side will push message into a single queue, payload in JSON format:
    /// {
    ///   "TableName": "TABLE_NAME",
    ///   "Action": "INSERT|UPDATE|DELETE",
    ///   "RowId": ROW_ID,
    ///   "AdditionalData": { ... optional ... }
    /// }
    /// </summary>
    public class OracleAQNotificationService(IDatabaseConnection databaseConnection, IServiceScopeFactory scopeFactory, IConfiguration configuration) : BackgroundService
    {
        private readonly IDatabaseConnection _databaseConnection = databaseConnection;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IConfiguration _configuration = configuration;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = _configuration.GetValue<bool>("SignalR:Enabled", false);
            if (!enabled) 
            {
                FileLogWriterService.Write(new LogEntry {
                    Message = "SignalR is disabled via configuration (SignalR:Enabled = false)",
                    LogLevel = (int)LogLevel.Information,
                    Source = "OracleAQNotificationService",
                    Module = "Nghex.Realtime.SignalR",
                    Action = "ExecuteAsync"
                });
                return;
            }

            var queueName = _configuration.GetValue<string>("SignalR:AQ:QueueName");
            if (string.IsNullOrWhiteSpace(queueName))
            {
                FileLogWriterService.Write(new LogEntry {
                    Message = "SignalR:AQ:QueueName is not configured. Service will not start.",
                    LogLevel = (int)LogLevel.Warning,
                    Source = "OracleAQNotificationService",
                    Module = "Nghex.Realtime.SignalR",
                    Action = "ExecuteAsync"
                });
                return;
            }

            var waitSeconds = _configuration.GetValue<int?>("SignalR:AQ:DequeueWaitSeconds") ?? 5;
            try
            {
                // Sử dụng connection string từ Nghex.Data (DatabaseConnection)
                var connectionString = _databaseConnection.GetConnectionString();

                await using var connection = new OracleConnection(connectionString);
                await connection.OpenAsync(stoppingToken);

                using var queue = new OracleAQQueue(queueName, connection)
                {
                    MessageType = OracleAQMessageType.Raw
                };

                queue.DequeueOptions = new OracleAQDequeueOptions
                {
                    DequeueMode = OracleAQDequeueMode.Remove,
                    Visibility = OracleAQVisibilityMode.OnCommit,
                    Wait = waitSeconds
                };

                FileLogWriterService.Write(new LogEntry {
                    Message = $"OracleAQNotificationService started. Listening on queue {queueName} with wait {waitSeconds}s",
                    LogLevel = (int)LogLevel.Information,
                    Source = "OracleAQNotificationService",
                    Module = "Nghex.Realtime.SignalR",
                    Action = "ExecuteAsync"
                });

                while (!stoppingToken.IsCancellationRequested)
                {
                    OracleAQMessage? message = null;
                    try
                    {
                        message = queue.Dequeue();
                    }
                    catch (OracleException ex) when (ex.Number == 25228)
                    {
                        continue;
                    }
                    catch (Exception ex)
                    {
                        FileLogWriterService.Write(new LogEntry {
                            Message = $"OracleAQNotificationService: error while dequeuing message. {ex.Message}",
                            LogLevel = (int)LogLevel.Error,
                            Source = "OracleAQNotificationService",
                            Module = "Nghex.Realtime.SignalR",
                            Action = "ExecuteAsync"
                        });
                        await Task.Delay(TimeSpan.FromSeconds(waitSeconds), stoppingToken);
                        continue;
                    }

                    if (message == null || message.Payload == null)
                        continue;

                    try
                    {
                        // Payload dạng RAW -> byte[]
                        if (message.Payload is not byte[] rawBytes)
                        {
                            FileLogWriterService.Write(new LogEntry {
                                Message = $"OracleAQNotificationService: unexpected payload type {message.Payload.GetType().FullName}",
                                LogLevel = (int)LogLevel.Warning,
                                Source = "OracleAQNotificationService",
                                Module = "Nghex.Realtime.SignalR",
                                Action = "ExecuteAsync"
                            });
                            continue;
                        }

                        var json = Encoding.UTF8.GetString(rawBytes);
                        var payload = JsonSerializer.Deserialize<OracleAQNotificationPayload>(json);
                        if (payload == null || string.IsNullOrWhiteSpace(payload.TableName))
                        {
                            FileLogWriterService.Write(new LogEntry {
                                Message = $"OracleAQNotificationService: invalid payload JSON: {json}",
                                LogLevel = (int)LogLevel.Warning,
                                Source = "OracleAQNotificationService",
                                Module = "Nghex.Realtime.SignalR",
                                Action = "ExecuteAsync"
                            });
                            continue;
                        }

                        var action = string.IsNullOrWhiteSpace(payload.Action)
                            ? "Unknown"
                            : payload.Action;

                        using var scope = _scopeFactory.CreateScope();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        // Gửi DataUpdate
                        await notificationService.SendDataUpdateAsync(
                            entityType: payload.TableName,
                            entityId: payload.RowId,
                            action: action);

                        // Gửi Refresh đến tất cả clients
                        await notificationService.SendRefreshToAllAsync(
                            refreshType: "data",
                            data: new
                            {
                                TableName = payload.TableName,
                                Action = action,
                                RowId = payload.RowId,
                                Source = "OracleAQ",
                                AdditionalData = payload.AdditionalData
                            });

                        FileLogWriterService.Write(new LogEntry {
                            Message = $"OracleAQNotificationService: processed message | Table: {payload.TableName} | Action: {action} | RowId: {payload.RowId}",
                            LogLevel = (int)LogLevel.Information,
                            Source = "OracleAQNotificationService",
                            Module = "Nghex.Realtime.SignalR",
                            Action = "ExecuteAsync"
                        });
                    }
                    catch (Exception ex)
                    {
                        FileLogWriterService.Write(new LogEntry {
                            Message = $"OracleAQNotificationService: error while processing dequeued message.",
                            LogLevel = (int)LogLevel.Error,
                            Source = "OracleAQNotificationService",
                            Module = "Nghex.Realtime.SignalR",
                            Action = "ExecuteAsync",
                            Exception = ex.Message,
                            StackTrace = ex.StackTrace
                        });
                    }
                }

                FileLogWriterService.Write(new LogEntry {
                    Message = "OracleAQNotificationService is stopping due to cancellation.",
                    LogLevel = (int)LogLevel.Information,
                    Source = "OracleAQNotificationService",
                    Module = "Nghex.Realtime.SignalR",
                    Action = "ExecuteAsync"
                });
            }
            catch (Exception ex)
            {
                FileLogWriterService.Write(new LogEntry {
                    Message = $"OracleAQNotificationService failed to start",
                    LogLevel = (int)LogLevel.Error,
                    Source = "OracleAQNotificationService",
                    Module = "Nghex.Realtime.SignalR",
                    Action = "ExecuteAsync",
                    Exception = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Payload standard for One Queue - Many Tables model
        /// </summary>
        private sealed class OracleAQNotificationPayload
        {
            [JsonPropertyName("table_name")]
            public string TableName { get; set; } = string.Empty;
            
            [JsonPropertyName("action")]
            public string Action { get; set; } = "Unknown";
            
            [JsonPropertyName("row_id")]
            public long RowId { get; set; } = 0;
            
            [JsonExtensionData]
            public Dictionary<string, JsonElement>? AdditionalData { get; set; }
        }
    }
}