using System.Text.Json;
using Nghex.Core.Enum;

namespace Nghex.Core.Logging
{
    /// <summary>
    /// File-based implementation of ILogging. Always available — no DB required.
    /// Writes JSON-per-line audit log with rolling and retention.
    /// </summary>
    public class FileLoggingService : ILogging
    {
        private static readonly object _sync = new();
        private readonly string _directory;
        private readonly string _fileName;
        private readonly int _maxFileSizeMb;
        private readonly int _retentionDays;

        public FileLoggingService(string directory = "logs", string fileName = "nghex.log",
            int maxFileSizeMb = 10, int retentionDays = 30)
        {
            _directory = directory;
            _fileName = fileName;
            _maxFileSizeMb = maxFileSizeMb;
            _retentionDays = retentionDays;
        }

        public Task LogDebugAsync(string message, string? source = null, string? module = null, string? action = null,
            object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null)
            => LogAsync(LogLevel.Debug, message, null, source, module, action, details, userId, username, requestId, ipAddress, userAgent);

        public Task LogInformationAsync(string message, string? source = null, string? module = null, string? action = null,
            object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null)
            => LogAsync(LogLevel.Information, message, null, source, module, action, details, userId, username, requestId, ipAddress, userAgent);

        public Task LogWarningAsync(string message, string? source = null, string? module = null, string? action = null,
            object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null)
            => LogAsync(LogLevel.Warning, message, null, source, module, action, details, userId, username, requestId, ipAddress, userAgent);

        public Task LogErrorAsync(string message, Exception? exception = null, string? source = null, string? module = null,
            string? action = null, object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null)
            => LogAsync(LogLevel.Error, message, exception, source, module, action, details, userId, username, requestId, ipAddress, userAgent);

        public Task LogCriticalAsync(string message, Exception? exception = null, string? source = null, string? module = null,
            string? action = null, object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null)
            => LogAsync(LogLevel.Critical, message, exception, source, module, action, details, userId, username, requestId, ipAddress, userAgent);

        public Task LogAsync(LogLevel level, string message, Exception? exception = null, string? source = null,
            string? module = null, string? action = null, object? details = null, long? userId = null,
            string? username = null, string? requestId = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                string? detailsJson = null;
                if (details != null)
                    try { detailsJson = JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }); }
                    catch { detailsJson = details.ToString(); }

                var line = JsonSerializer.Serialize(new
                {
                    timestampUtc = DateTime.UtcNow.ToString("o"),
                    logLevel = (int)level,
                    logLevelName = level.ToString(),
                    message,
                    source,
                    module,
                    action,
                    userId,
                    username,
                    requestId,
                    ipAddress,
                    userAgent,
                    exception = exception?.Message,
                    stackTrace = exception?.StackTrace,
                    details = detailsJson
                }, new JsonSerializerOptions { WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                WriteToFile(line);
            }
            catch
            {
                // Swallow to avoid cascading failures
            }

            return Task.CompletedTask;
        }

        private void WriteToFile(string line)
        {
            try
            {
                var baseDir = AppContext.BaseDirectory?.Trim() ?? string.Empty;
                var logsDir = Path.IsPathRooted(_directory) ? _directory : Path.Combine(baseDir, _directory);
                Directory.CreateDirectory(logsDir);
                var logPath = Path.Combine(logsDir, _fileName);

                lock (_sync)
                {
                    RollIfNeeded(logPath);
                    File.AppendAllText(logPath, line + Environment.NewLine);
                    CleanupOldFiles(logsDir);
                }
            }
            catch { }
        }

        private void RollIfNeeded(string logPath)
        {
            if (_maxFileSizeMb <= 0 || !File.Exists(logPath)) return;
            var info = new FileInfo(logPath);
            if (info.Length < (long)_maxFileSizeMb * 1024L * 1024L) return;

            var dir = info.DirectoryName ?? string.Empty;
            var baseName = Path.GetFileNameWithoutExtension(info.Name);
            var ext = Path.GetExtension(info.Name);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            File.Move(logPath, Path.Combine(dir, $"{baseName}-{stamp}{ext}"), true);
        }

        private void CleanupOldFiles(string logsDir)
        {
            if (_retentionDays <= 0 || !Directory.Exists(logsDir)) return;
            var baseName = Path.GetFileNameWithoutExtension(_fileName);
            var ext = Path.GetExtension(_fileName);
            foreach (var file in Directory.GetFiles(logsDir, $"{baseName}-*{ext}"))
                try
                {
                    if ((DateTime.UtcNow - new FileInfo(file).CreationTimeUtc).TotalDays > _retentionDays)
                        File.Delete(file);
                }
                catch { }
        }
    }
}
