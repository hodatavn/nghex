using System.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Nghex.Logging.Models;
using Nghex.Logging.Providers;

namespace Nghex.Logging
{
    /// <summary>
    /// Simple file writer for log fallback with basic rolling and retention
    /// </summary>
    public static class FileLogWriterService
    {
        private static readonly object _sync = new();

        /// <summary>
        /// Write log entry to file
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="fileLoggerOptions"></param>
        public static void Write(LogEntry logEntry, FileLoggerOptions? fileLoggerOptions = null)
        {
            fileLoggerOptions ??= FileLoggerOptions.DefaultOptions();
            Write(logEntry, fileLoggerOptions.Directory, fileLoggerOptions.FileName, fileLoggerOptions.MaxFileSizeMb, fileLoggerOptions.RetentionDays);
        }

        /// <summary>
        /// Write log entry to file
        /// </summary>
        /// <param name="logEntry"></param>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <param name="maxFileSizeMb"></param>
        /// <param name="retentionDays"></param>
        public static void Write(LogEntry logEntry, string directory, string fileName, int maxFileSizeMb, int retentionDays)
        {
            try
            {
                var baseDirectory = AppContext.BaseDirectory?.Trim() ?? string.Empty;
                var logsDirectory = Path.IsPathRooted(directory)
                    ? directory
                    : Path.Combine(baseDirectory, directory);

                Directory.CreateDirectory(logsDirectory);
                var logFilePath = Path.Combine(logsDirectory, string.IsNullOrWhiteSpace(fileName) ? "omed.log" : fileName);
                lock (_sync)
                {
                    try
                    {
                        RollIfNeeded(logFilePath, maxFileSizeMb);
                        var line = SerializeLogEntry(logEntry);
                        File.AppendAllText(logFilePath, line + Environment.NewLine);
                        CleanupOldFiles(logsDirectory, fileName, retentionDays);
                    }
                    catch
                    {
                        // Swallow exceptions to avoid secondary failures during logging
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot write log entry to file: {ex.Message}");
            }
        }

        private static string SerializeLogEntry(LogEntry logEntry)
        {
            var payload = new
            {
                timestampUtc = DateTime.UtcNow.ToString("o"),
                logLevel = logEntry.LogLevel,
                message = logEntry.Message,
                source = logEntry.Source,
                module = logEntry.Module,
                action = logEntry.Action,
                userId = logEntry.UserId,
                username = logEntry.Username,
                ipAddress = logEntry.IpAddress,
                userAgent = logEntry.UserAgent,
                requestId = logEntry.RequestId,
                exception = logEntry.Exception,
                stackTrace = logEntry.StackTrace,
                details = logEntry.Details
            };

            return JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }

        private static void RollIfNeeded(string logFilePath, int maxFileSizeMb)
        {
            try
            {
                if (maxFileSizeMb <= 0) return;
                if (!File.Exists(logFilePath)) return;

                var fileInfo = new FileInfo(logFilePath);
                var maxBytes = (long)maxFileSizeMb * 1024L * 1024L;
                if (fileInfo.Length < maxBytes) return;

                var directory = fileInfo.DirectoryName ?? string.Empty;
                var baseName = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var ext = Path.GetExtension(fileInfo.Name);
                var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var rolledName = $"{baseName}-{stamp}{ext}";
                var rolledPath = Path.Combine(directory, rolledName);

                File.Move(logFilePath, rolledPath, true);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot roll log file: {logFilePath}\nError: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private static void CleanupOldFiles(string directory, string fileName, int retentionDays)
        {
            try
            {
                if (retentionDays <= 0) return;
                if (!Directory.Exists(directory)) return;

                var baseName = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);
                var now = DateTime.UtcNow;

                var files = Directory.GetFiles(directory, $"{baseName}-*{ext}");
                foreach (var file in files)
                {
                    try
                    {
                        var info = new FileInfo(file);
                        if ((now - info.CreationTimeUtc).TotalDays > retentionDays)
                            File.Delete(file);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"File {file} not found or cannot delete log file.\nError: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot cleanup old log files: {ex.Message}");
            }
        }
    }
}




























