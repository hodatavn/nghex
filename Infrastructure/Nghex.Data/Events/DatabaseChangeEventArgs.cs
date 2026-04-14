using Nghex.Data.Enum;

namespace Nghex.Data.Events
{
    

    /// <summary>
    /// Event arguments for database change notification
    /// </summary>
    public class DatabaseChangeEventArgs : EventArgs
    {
        public string TableName { get; set; } = string.Empty;
        public DatabaseChangeAction Action { get; set; } = DatabaseChangeAction.Insert;
        public long? RowId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? AdditionalData { get; set; } = [];
    }
}