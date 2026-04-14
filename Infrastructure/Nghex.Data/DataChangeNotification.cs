using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Nghex.Data.Enum;
using Nghex.Data.Events;
using Nghex.Data.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace Nghex.Data
{
    public class DataChangeNotification : BackgroundService, IDatabaseChangeNotification
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly ILogger<DataChangeNotification> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, OracleDependency> _dependencies = [];
        private readonly ConcurrentDictionary<string, OnChangeEventHandler> _eventHandlers = [];
        private readonly Channel<string> _reRegisterQueue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        private OracleConnection? _connection;
        private bool _isMonitoring = false;
        private readonly string[] _tablesMonitoring = [];

        /// <summary>
        /// Event handler for database change notification
        /// </summary>
        public event EventHandler<DatabaseChangeEventArgs>? OnDatabaseChange = null!;

        public DataChangeNotification(
            IDatabaseConnection databaseConnection, 
            ILogger<DataChangeNotification> logger, 
            IConfiguration configuration)
        {
            _databaseConnection = databaseConnection;
            _logger = logger;
            _configuration = configuration;
            _tablesMonitoring =_configuration.GetSection("DatabaseChangeNotification:Tables")
                .Get<string[]>() ?? [];
        }

        public async Task RegisterTableAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if(_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _logger.LogWarning("Cannot register table for change notification. Connection is not open.");
                return;
            }
            string commandText = string.Empty;
            try
            {
                var dependency = new OracleDependency
                {
                    QueryBasedNotification = true
                };

                // Create and store the event handler delegate
                void handler(object sender, OracleNotificationEventArgs args) =>
                    OnDatabaseChangeHandler(sender, args, tableName);
                dependency.OnChange += handler;
                _eventHandlers.TryAdd(tableName, handler);

                // Oracle DCN requires a deterministic SELECT statement per table
                commandText = $"SELECT ROWID FROM {tableName}";
                _logger.LogInformation("Registering OracleDependency | Table: {Table} | Command: {Command}", tableName, commandText);
                using var command = new OracleCommand(commandText, _connection)
                {
                    AddRowid = true
                };

                dependency.AddCommandDependency(command);
                var notificationRequest = command.Notification;
                if (notificationRequest != null)
                {
                    notificationRequest.IsNotifiedOnce = false; // keep listening
                    notificationRequest.Timeout = 0; // never expire
                }

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                await reader.DisposeAsync();

                _dependencies.TryAdd(tableName, dependency);
                _logger.LogInformation($"Table {tableName.ToUpper()} registered for change notification.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to register table {tableName} for change notification. Command text: {commandText}");
                throw;
            }
        }

        // IHostedService.StartAsync (from BackgroundService) should only start the worker loop.
        public override Task StartAsync(CancellationToken cancellationToken)
            => base.StartAsync(cancellationToken);

        // Explicit interface implementation to start monitoring on-demand (used internally).
        Task IDatabaseChangeNotification.StartAsync(CancellationToken cancellationToken)
            => StartMonitoringAsync(cancellationToken);

        private async Task StartMonitoringAsync(CancellationToken cancellationToken = default)
        {
            if(_isMonitoring)
            {
                _logger.LogInformation("Database change notification is already started");
                return;
            }
            try
            {
                _connection = new OracleConnection(_databaseConnection.GetConnectionString());
                await _connection.OpenAsync(cancellationToken);

                foreach (var table in _tablesMonitoring)
                    await RegisterTableAsync(table, cancellationToken);

                _isMonitoring = true;
                _logger.LogInformation($"Monitoring {_tablesMonitoring.Length} tables for changes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start database change notification.");
                _isMonitoring = false;
                throw;
            }
        }

        // IHostedService.StopAsync (from BackgroundService) should stop worker loop, then cleanup resources.
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await StopMonitoringAsync(cancellationToken);
        }

        // Explicit interface implementation to stop monitoring on-demand.
        Task IDatabaseChangeNotification.StopAsync(CancellationToken cancellationToken)
            => StopMonitoringAsync(cancellationToken);

        private async Task StopMonitoringAsync(CancellationToken cancellationToken = default)
        {
            if(!_isMonitoring)
            {
                _logger.LogInformation("Database change notification is not started");
                return;
            }
            try
            {
                foreach (var kvp in _dependencies)
                {
                    var tableName = kvp.Key;
                    var dependency = kvp.Value;
                    
                    // Unsubscribe using the stored handler
                    if (_eventHandlers.TryRemove(tableName, out var handler))
                        dependency.OnChange -= handler;

                    // Ensure dependency is disposed to release underlying resources/threads
                    if (dependency is IDisposable disposable)
                        disposable.Dispose();
                }
                _dependencies.Clear();

                if(_connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    await _connection.CloseAsync();
                    await _connection.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop database change notification.");
                _isMonitoring = false;
                throw;
            }
            finally
            {
                _isMonitoring = false;
                _reRegisterQueue.Writer.TryComplete();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = _configuration.GetValue<bool>("DatabaseChangeNotification:Enabled", false);
            if (!enabled)
            {
                _logger.LogInformation("Database change notification is not enabled");
                return;
            }
            if (_tablesMonitoring.Length == 0)
            {
                _logger.LogWarning("No tables configured for change notification");
                return;
            }
            await Task.Delay(5000, stoppingToken); //delay for 5 seconds for application startup
            _logger.LogInformation("Database change notification started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if(!_isMonitoring)
                        await StartMonitoringAsync(stoppingToken);

                    // Drain queued re-registrations (serialized) to avoid spawning many tasks/threads
                    while (_reRegisterQueue.Reader.TryRead(out var tableName))
                    {
                        await Task.Delay(1000, stoppingToken); // small debounce
                        await ReRegisterTableAsync(tableName, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while watching database changes");
                    _isMonitoring = false;
                    await Task.Delay(10000, stoppingToken); //delay for 10 seconds before retrying
                }
                await Task.Delay(500, stoppingToken); 
            }
        }

        private void OnDatabaseChangeHandler(object sender, OracleNotificationEventArgs args, string tableName)
        {
            try
            {
                _logger.LogInformation(
                    "Oracle DCN received | Table: {Table} | Info: {Info} | Source: {Source} | Type: {Type}",
                    tableName,
                    args.Info,
                    args.Source,
                    args.Type);
                var action = DetermineAction(args);
                var eventArgs = new DatabaseChangeEventArgs
                {
                    TableName = tableName,
                    Action = action,
                    Timestamp = DateTime.UtcNow.ToLocalTime(),
                    AdditionalData = new Dictionary<string, object> {
                        { "Info", args.Info.ToString()},
                        { "SourceType", args.Source.ToString() ?? string.Empty },
                        { "ResourceNames", args.ResourceNames },
                        { "OracleSource", "Oracle DCN"}
                    }
                };
                _logger.LogInformation(
                    $"Invoking OnDatabaseChange event | Table: {tableName} | Action: {action} | Timestamp: {eventArgs.Timestamp}"
                    );
                OnDatabaseChange?.Invoke(this, eventArgs);
                _logger.LogInformation("OnDatabaseChange event dispatched | Table: {Table}", tableName);

                _logger.LogInformation("Queueing table re-registration | Table: {Table}", tableName);
                _reRegisterQueue.Writer.TryWrite(tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling database change notification");
                throw;
            }
        }

        private DatabaseChangeAction DetermineAction(OracleNotificationEventArgs args)
        {
            return args.Info switch
            {
                OracleNotificationInfo.Insert => DatabaseChangeAction.Insert,
                OracleNotificationInfo.Update => DatabaseChangeAction.Update,
                OracleNotificationInfo.Delete => DatabaseChangeAction.Delete,
                _ => DatabaseChangeAction.Unknown
            };
        }

        private async Task ReRegisterTableAsync(string tableName, CancellationToken cancellationToken = default)
        {
            try
            {
                if(_dependencies.TryRemove(tableName, out var oldDependency))
                {
                    // Unsubscribe using the stored handler
                    if (_eventHandlers.TryRemove(tableName, out var oldHandler))
                    {
                        oldDependency.OnChange -= oldHandler;
                    }

                    if (oldDependency is IDisposable disposable)
                        disposable.Dispose();
                }
                await RegisterTableAsync(tableName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to re-register table {tableName}");
                throw;
            }
        }
    }
}