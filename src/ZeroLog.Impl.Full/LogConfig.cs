using ZeroLog.Appenders;

namespace ZeroLog
{
    public struct LogConfig
    {
        public Appender[]? Appenders { get; set; }
        public Level Level { get; set; }
        public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; set; }
    }
}
