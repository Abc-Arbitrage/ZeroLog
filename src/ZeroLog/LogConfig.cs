using ZeroLog.Appenders;

namespace ZeroLog
{
    public struct LogConfig
    {
        public IAppender[]? Appenders { get; set; }
        public Level Level { get; set; }
        public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; set; }
    }
}
