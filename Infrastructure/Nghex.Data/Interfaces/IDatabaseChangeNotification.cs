using Nghex.Data.Events;

namespace Nghex.Data.Interfaces
{
    public interface IDatabaseChangeNotification
    {
        /// <summary>
        /// Event handler for database change notification
        /// </summary>
        event EventHandler<DatabaseChangeEventArgs>? OnDatabaseChange;

        /// <summary>
        /// Start the database change notification
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop the database change notification
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Register a table for change notification
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RegisterTableAsync(string tableName, CancellationToken cancellationToken = default);

    }
}