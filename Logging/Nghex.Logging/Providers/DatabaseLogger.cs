using Microsoft.Extensions.Logging;
using Nghex.Logging.Interfaces;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;
using OurLogLevel = Nghex.Core.Enum.LogLevel;

namespace Nghex.Logging.Providers
{
    /// <summary>
    /// Database Logger để ghi logs vào database
    /// </summary>
    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly ILoggingService _loggingService;
        private readonly DatabaseLoggerOptions _options;

        public DatabaseLogger(string categoryName, ILoggingService loggingService, DatabaseLoggerOptions options)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(MicrosoftLogLevel logLevel)
        {
            // Chỉ ghi vào database nếu log level >= configured level
            return logLevel >= _options.MinimumLogLevel;
        }

        public void Log<TState>(MicrosoftLogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            try
            {
                var message = formatter(state, exception);
                if (string.IsNullOrEmpty(message))
                    return;

                // Convert Microsoft.Extensions.Logging.LogLevel to our LogLevel enum
                var ourLogLevel = ConvertToOurLogLevel(logLevel);

                // Extract additional information from state
                var details = ExtractDetails(state, exception);

                // Extract debug information
                var debugInfo = ExtractDebugInfo();

                // Log to database asynchronously (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _loggingService.LogAsync(
                            ourLogLevel,
                            message,
                            exception,
                            source: _categoryName,
                            module: ExtractModuleFromCategory(_categoryName),
                            action: ExtractActionFromCategory(_categoryName),
                            details: details,
                            requestId: ExtractRequestId(state),
                            ipAddress: ExtractIpAddress(state),
                            userAgent: ExtractUserAgent(state)
                        );
                    }
                    catch (Exception ex)
                    {
                        // Don't throw exceptions from logging to avoid infinite loops
                        Console.WriteLine($"Failed to write log to database: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                // Don't throw exceptions from logging to avoid infinite loops
                Console.WriteLine($"Failed to process log: {ex.Message}");
            }
        }

        private OurLogLevel ConvertToOurLogLevel(MicrosoftLogLevel logLevel)
        {
            return logLevel switch
            {
                MicrosoftLogLevel.Trace => OurLogLevel.Debug,
                MicrosoftLogLevel.Debug => OurLogLevel.Debug,
                MicrosoftLogLevel.Information => OurLogLevel.Information,
                MicrosoftLogLevel.Warning => OurLogLevel.Warning,
                MicrosoftLogLevel.Error => OurLogLevel.Error,
                MicrosoftLogLevel.Critical => OurLogLevel.Critical,
                MicrosoftLogLevel.None => OurLogLevel.Debug,
                _ => OurLogLevel.Information
            };
        }

        private object? ExtractDetails<TState>(TState state, Exception? exception)
        {
            try
            {
                var details = new Dictionary<string, object?>();

                // Add exception details if present
                if (exception != null)
                {
                    details["Exception"] = new
                    {
                        Type = exception.GetType().Name,
                        Message = exception.Message,
                        StackTrace = exception.StackTrace
                    };
                }

                // Add state details if it's a structured logging state
                if (state is IEnumerable<KeyValuePair<string, object?>> structuredState)
                {
                    foreach (var kvp in structuredState)
                    {
                        if (kvp.Key != "{OriginalFormat}" && kvp.Key != "Message")
                        {
                            details[kvp.Key] = kvp.Value;
                        }
                    }
                }

                return details.Count > 0 ? details : null;
            }
            catch
            {
                return null;
            }
        }

        private string? ExtractModuleFromCategory(string categoryName)
        {
            // Extract module from category name (e.g., "MyApp.Controllers.AccountController" -> "Controllers")
            var parts = categoryName.Split('.');
            if (parts.Length > 1)
            {
                return parts[^2]; // Second to last part
            }
            return "Application";
        }

        private string? ExtractActionFromCategory(string categoryName)
        {
            // Extract action from category name (e.g., "MyApp.Controllers.AccountController" -> "AccountController")
            var parts = categoryName.Split('.');
            return parts[^1]; // Last part
        }

        private string? ExtractRequestId<TState>(TState state)
        {
            if (state is IEnumerable<KeyValuePair<string, object?>> structuredState)
            {
                return structuredState.FirstOrDefault(kvp => 
                    kvp.Key.Equals("RequestId", StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Equals("CorrelationId", StringComparison.OrdinalIgnoreCase)
                ).Value?.ToString();
            }
            return null;
        }

        private string? ExtractIpAddress<TState>(TState state)
        {
            if (state is IEnumerable<KeyValuePair<string, object?>> structuredState)
            {
                return structuredState.FirstOrDefault(kvp => 
                    kvp.Key.Equals("IpAddress", StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Equals("RemoteIpAddress", StringComparison.OrdinalIgnoreCase)
                ).Value?.ToString();
            }
            return null;
        }

        private string? ExtractUserAgent<TState>(TState state)
        {
            if (state is IEnumerable<KeyValuePair<string, object?>> structuredState)
            {
                return structuredState.FirstOrDefault(kvp => 
                    kvp.Key.Equals("UserAgent", StringComparison.OrdinalIgnoreCase)
                ).Value?.ToString();
            }
            return null;
        }

        private (string? ClassName, int? LineNumber) ExtractDebugInfo()
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
                    var lineNumber = frame.GetFileLineNumber();
                    
                    return (className, lineNumber > 0 ? lineNumber : null);
                }
            }
            catch
            {
                // If stack trace extraction fails, return null values
            }

            return (null, null);
        }
    }
}
