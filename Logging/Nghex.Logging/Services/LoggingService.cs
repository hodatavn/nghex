using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Nghex.Logging.Interfaces;
using Nghex.Logging.Models;
using NghexLogLevel = Nghex.Core.Enum.LogLevel;
using Nghex.Logging.Providers;

namespace Nghex.Logging.Services
{
    /// <summary>
    /// Logging Service implementation
    /// </summary>
    public class LoggingService(ILogRepository logRepository) : ILoggingService, ILogQueryService
    {
        private readonly ILogRepository _logRepository = logRepository;
        private readonly FileLoggerOptions _fileOptions = FileLoggerOptions.DefaultOptions();

        public async Task LogDebugAsync(string message, string? source = null, string? module = null, string? action = null, 
                                        object? details = null, long? userId = null, string? username = null, 
                                        string? requestId = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync(NghexLogLevel.Debug, message, null, source, module, action, details, userId, username, requestId, ipAddress, userAgent);
        }

        public async Task LogInformationAsync(string message, string? source = null, string? module = null, string? action = null, 
                                             object? details = null, long? userId = null, string? username = null, 
                                             string? requestId = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync(NghexLogLevel.Information, message, null, source, module, action, details, userId, username, requestId, ipAddress, userAgent);
        }

        public async Task LogWarningAsync(string message, string? source = null, string? module = null, string? action = null, 
                                         object? details = null, long? userId = null, string? username = null, 
                                         string? requestId = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync(NghexLogLevel.Warning, message, null, source, module, action, details, userId, username, requestId, ipAddress, userAgent);
        }

        public async Task LogErrorAsync(string message, Exception? exception = null, string? source = null, string? module = null, 
                                       string? action = null, object? details = null, long? userId = null, string? username = null, 
                                       string? requestId = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync(NghexLogLevel.Error, message, exception, source, module, action, details, userId, username, requestId, ipAddress, userAgent);
        }

        public async Task LogCriticalAsync(string message, Exception? exception = null, string? source = null, string? module = null, 
                                          string? action = null, object? details = null, long? userId = null, string? username = null, 
                                          string? requestId = null, string? ipAddress = null, string? userAgent = null)
        {
            await LogAsync(NghexLogLevel.Critical, message, exception, source, module, action, details, userId, username, requestId, ipAddress, userAgent);
        }

        public async Task LogAsync(NghexLogLevel level, string message, Exception? exception = null, string? source = null, 
                                  string? module = null, string? action = null, object? details = null, long? userId = null, 
                                  string? username = null, string? requestId = null, string? ipAddress = null, string? userAgent = null)
        {
            LogEntry? log = null;
            try
            {
                log = new LogEntry
                {
                    LogLevel = (int)level,
                    Message = message,
                    Source = source ?? GetCallerInfo(),
                    Module = module,
                    Action = action,
                    UserId = userId,
                    Username = username,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    RequestId = requestId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = username ?? "system"
                };

                // Serialize details to JSON if provided
                if (details != null)
                {
                    try
                    {
                        log.Details = JsonSerializer.Serialize(details, new JsonSerializerOptions
                        {
                            WriteIndented = false,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    }
                    catch (Exception ex)
                    {
                        log.Details = $"Error serializing details: {ex.Message}";
                    }
                }

                // Add exception information if provided
                if (exception != null)
                {
                    log.Exception = exception.Message;
                    log.StackTrace = exception.StackTrace;
                }
                await _logRepository.AddAsync(log);
            }
            catch (Exception ex)
            {
                try
                {
                    log ??= new LogEntry
                        {
                            LogLevel = (int)level,
                            Message = message,
                            Source = source,
                            Module = module,
                            Action = action,
                            UserId = userId,
                            Username = username,
                            IpAddress = ipAddress,
                            UserAgent = userAgent,
                            RequestId = requestId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = username ?? "system",
                            Exception = exception?.Message ?? ex.Message,
                            StackTrace = exception?.StackTrace ?? ex.StackTrace
                        };
                    if (_fileOptions.Enabled && MeetsFileLevel(level))
                        FileLogWriterService.Write(log, _fileOptions);
                }
                catch(Exception fEx)
                {
                    Console.WriteLine($"LogEntry: {log}");
                    Console.WriteLine($"Cannot write log to file: {fEx.Message}\nStackTrace: {fEx.StackTrace}");
                }
            }
        }

        private bool MeetsFileLevel(NghexLogLevel level)
        {
            // Map to Microsoft LogLevel for comparison
            var mapped = level switch
            {
                NghexLogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
                NghexLogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
                NghexLogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
                NghexLogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
                NghexLogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
                _ => Microsoft.Extensions.Logging.LogLevel.Information
            };
            return mapped >= _fileOptions.MinimumLogLevel;
        }

        public async Task<IEnumerable<LogEntry>> GetLogsByLevelAsync(NghexLogLevel level, int offset = 0, int limit = 100)
        {
            return await _logRepository.GetByLevelAsync(level, offset, limit);
        }

        public async Task<IEnumerable<LogEntry>> GetLogsByUserAsync(string username, int offset = 0, int limit = 100)
        {
            return await _logRepository.GetByUserAsync(username, offset, limit);
        }

        public async Task<IEnumerable<LogEntry>> GetLogsByModuleAsync(string module, int offset = 0, int limit = 100)
        {
            return await _logRepository.GetByModuleAsync(module, offset, limit);
        }

        public async Task<IEnumerable<LogEntry>> GetLogsByDateRangeAsync(DateTime fromDate, DateTime toDate, int offset = 0, int limit = 100)
        {
            return await _logRepository.GetByDateRangeAsync(fromDate, toDate, offset, limit);
        }

        public async Task<IEnumerable<LogEntry>> SearchLogsAsync(string keyword, int offset = 0, int limit = 100)
        {
            return await _logRepository.SearchAsync(keyword, offset, limit);
        }

        public async Task<IEnumerable<LogEntry>> GetLogsByRequestIdAsync(string requestId)
        {
            return await _logRepository.GetByRequestIdAsync(requestId);
        }

        public async Task<int> CleanupOldLogsAsync(int daysToKeep = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            return await _logRepository.DeleteOldLogsAsync(cutoffDate);
        }

        public async Task<bool> DeleteLogAsync(long id)
        {
            return await _logRepository.DeleteAsync(id);
        }

        public async Task<long> CountLogsByLevelAsync(NghexLogLevel level)
        {
            return await _logRepository.CountByLevelAsync(level);
        }

        public async Task<long> CountLogsByUserAsync(string username)
        {
            return await _logRepository.CountByUserAsync(username);
        }

        public async Task<long> CountLogsByModuleAsync(string module)
        {
            return await _logRepository.CountByModuleAsync(module);
        }

        public async Task<long> CountLogsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _logRepository.CountByDateRangeAsync(fromDate, toDate);
        }

        public async Task<long> CountSearchLogsAsync(string keyword)
        {
            return await _logRepository.CountBySearchAsync(keyword);
        }

        /// <summary>
        /// Lấy thông tin về caller (class và method) từ application code
        /// </summary>
        private string GetCallerInfo()
        {
            try
            {
                var stackTrace = new System.Diagnostics.StackTrace(true);
                
                // Skip các framework methods và tìm application code
                for (int i = 2; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    if (frame == null) continue;

                    var method = frame.GetMethod();
                    if (method == null) continue;

                    var declaringType = method.DeclaringType;
                    if (declaringType == null) continue;

                    var typeName = declaringType.FullName ?? declaringType.Name;
                    
                    // Skip các framework và internal methods
                    if (typeName.Contains("Microsoft.Extensions.Logging") ||
                        typeName.Contains("Microsoft.AspNetCore") ||
                        typeName.Contains("Microsoft.Extensions.Hosting") ||
                        typeName.Contains("Microsoft.Extensions.DependencyInjection") ||
                        typeName.Contains("System.") ||
                        typeName.Contains("Nghex.Logging.Services") ||
                        typeName.Contains("Nghex.Logging.Providers") ||
                        typeName.Contains("ConsoleLifetime") ||
                        typeName.Contains("ApplicationLifetime") ||
                        typeName.Contains("HttpsRedirectionMiddleware") ||
                        typeName.Contains("Main$") ||
                        typeName.Contains("<") && typeName.Contains(">d__"))
                    {
                        continue;
                    }

                    // Tìm thấy application code
                    var className = declaringType.Name;
                    var methodName = method.Name;
                    
                    // Clean up method name (remove async state machine names)
                    if (methodName.Contains("<") && methodName.Contains(">d__"))
                    {
                        methodName = methodName.Split('<')[0] + "Async";
                    }
                    
                    return $"{className}.{methodName}";
                }
            }
            catch
            {
                // Ignore errors in getting caller info
            }

            return "Unknown";
        }

    }
}
