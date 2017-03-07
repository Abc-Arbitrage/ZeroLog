namespace ZeroLog
{
    public class LogManagerConfiguration
    {
        public int LogEventQueueSize { get; set; }
        public int LogEventBufferSize { get; set; }
        public Level Level { get; set; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; set; }
    }
}