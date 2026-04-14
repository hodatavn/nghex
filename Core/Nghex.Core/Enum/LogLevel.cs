namespace Nghex.Core.Enum
{
    public enum LogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    public static class LogLevelExtensions
    {
        public static LogLevel FromString(string level) => System.Enum.Parse<LogLevel>(level);
    }
}
