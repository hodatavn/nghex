namespace Nghex.Data.Setup
{
    /// <summary>
    /// Result of database setup operation.
    /// </summary>
    public class DatabaseSetupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string DataProvider { get; set; } = string.Empty;
        public List<string> ExecutedCommands { get; set; } = [];
        public List<DatabaseSetupErrorResult> Errors { get; set; } = [];
    }

    public struct DatabaseSetupErrorResult
    {
        public DatabaseSetupErrorResult(string error, string? command = null, string? errorStackTrace = null)
        {
            Error = error;
            Command = command ?? string.Empty;
            ErrorStackTrace = errorStackTrace ?? string.Empty;
        }

        public string Error { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string ErrorStackTrace { get; set; } = string.Empty;
    }
}
