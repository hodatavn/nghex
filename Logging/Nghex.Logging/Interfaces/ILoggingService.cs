using Nghex.Core.Logging;

namespace Nghex.Logging.Interfaces
{
    /// <summary>
    /// DB-backed logging interface — extends ILogging (file) with database write capability.
    /// Inject this when you need guaranteed DB audit trail.
    /// Inject ILogging when file logging is sufficient.
    /// </summary>
    public interface ILoggingService : ILogging
    {
    }
}
