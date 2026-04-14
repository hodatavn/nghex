using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nghex.Logging.Interfaces;

namespace Nghex.Logging.Providers
{
    /// <summary>
    /// Database Logger Provider để ghi logs vào database
    /// </summary>
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly ILoggingService _loggingService;
        private readonly IDisposable? _onChangeToken;
        private readonly DatabaseLoggerOptions _options;

        public DatabaseLoggerProvider(ILoggingService loggingService, IOptionsMonitor<DatabaseLoggerOptions> options)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _options = options.CurrentValue;
            _onChangeToken = options.OnChange(updatedOptions => {
                Console.WriteLine($"DatabaseLoggerOptions changed: MinimumLogLevel={updatedOptions.MinimumLogLevel}");
            });
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(categoryName, _loggingService, _options);
        }

        public void Dispose()
        {
            _onChangeToken?.Dispose();
        }
    }
}
