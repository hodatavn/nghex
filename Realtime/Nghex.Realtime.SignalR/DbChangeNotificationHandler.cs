using Nghex.Data.Enum;
using Nghex.Data.Events;
using Nghex.Data.Interfaces;
using Nghex.Realtime.SignalR.Services;
using System.Threading.Channels;

namespace Nghex.Realtime.SignalR {
    public class DbChangeNotificationHandler : BackgroundService
    {
        private readonly IDatabaseChangeNotification _databaseChangeNotification;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DbChangeNotificationHandler> _logger;
        private readonly Channel<DatabaseChangeEventArgs> _queue = Channel.CreateUnbounded<DatabaseChangeEventArgs>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        public DbChangeNotificationHandler(
            IDatabaseChangeNotification databaseChangeNotification,
            IServiceScopeFactory scopeFactory,
            ILogger<DbChangeNotificationHandler> logger)
        {
            _databaseChangeNotification = databaseChangeNotification;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _databaseChangeNotification.OnDatabaseChange += HandleDatabaseChange;
            _logger.LogInformation("Database change notification handler started and subscribed to IDatabaseChangeNotification");

            try
            {
                while (await _queue.Reader.WaitToReadAsync(stoppingToken))
                {
                    while (_queue.Reader.TryRead(out var e))
                    {
                        await ProcessDatabaseChangeAsync(e, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // expected on shutdown
            }
        }

        private void HandleDatabaseChange(object? sender, DatabaseChangeEventArgs e)
        {
            // Never do async work in the event callback; queue it to the background loop.
            if (!_queue.Writer.TryWrite(e))
            {
                _logger.LogWarning("DbChangeNotificationHandler: dropping change event because queue is not available. Table={Table} Action={Action}", e.TableName, e.Action);
            }
        }

        private async Task ProcessDatabaseChangeAsync(DatabaseChangeEventArgs e, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                _logger.LogInformation(
                    "DbChangeNotificationHandler received event | Table: {Table} | Action: {Action} | RowId: {RowId} | Timestamp: {Timestamp} | AdditionalData: {AdditionalData}",
                    e.TableName, e.Action, e.RowId, e.Timestamp, e.AdditionalData
                );

                await notificationService.SendDataUpdateAsync(
                    entityType: e.TableName,
                    entityId: e.RowId ?? 0,
                    action: e.Action.GetActionName()
                );

                // send refresh notification to all clients
                await notificationService.SendRefreshToAllAsync(
                    refreshType: "data",
                    data: new {
                        TableName = e.TableName,
                        Action = e.Action.GetActionName(),
                        RowId = e.RowId,
                        Timestamp = e.Timestamp,
                        Source = "DatabaseChangeNotification",
                        AdditionalData = e.AdditionalData
                    }
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // ignore on shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling database change notification | Table: {Table} | Action: {Action}", e.TableName, e.Action);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Database change notification");
            if(_databaseChangeNotification != null)
                _databaseChangeNotification.OnDatabaseChange -= HandleDatabaseChange;

            _queue.Writer.TryComplete();
            
            await base.StopAsync(cancellationToken);
        }
    }
}