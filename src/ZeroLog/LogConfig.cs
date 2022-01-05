using ZeroLog.Appenders;

namespace ZeroLog
{
    public struct LogConfig
    {
        public IAppender[]? Appenders { get; set; }
        public Level Level { get; set; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; set; }
        public LogEventArgumentExhaustionStrategy LogEventArgumentExhaustionStrategy { get; set; } // TODO Implement this
    }
}
