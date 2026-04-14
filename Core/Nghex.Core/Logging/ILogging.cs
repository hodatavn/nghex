using Nghex.Core.Enum;

namespace Nghex.Core.Logging
{
    /// <summary>
    /// Shared logging contract for cross-module usage.
    /// Modules should depend on this abstraction instead of concrete logging implementations.
    /// </summary>
    public interface ILogging
    {
        Task LogDebugAsync(string message, string? source = null, string? module = null, string? action = null,
            object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null);

        Task LogInformationAsync(string message, string? source = null, string? module = null, string? action = null,
            object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null);

        Task LogWarningAsync(string message, string? source = null, string? module = null, string? action = null,
            object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null);

        Task LogErrorAsync(string message, Exception? exception = null, string? source = null, string? module = null,
            string? action = null, object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null);

        Task LogCriticalAsync(string message, Exception? exception = null, string? source = null, string? module = null,
            string? action = null, object? details = null, long? userId = null, string? username = null,
            string? requestId = null, string? ipAddress = null, string? userAgent = null);

        Task LogAsync(LogLevel level, string message, Exception? exception = null, string? source = null,
            string? module = null, string? action = null, object? details = null, long? userId = null,
            string? username = null, string? requestId = null, string? ipAddress = null, string? userAgent = null);
    }
}
