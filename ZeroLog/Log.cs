namespace ZeroLog
{
    public partial class Log
    {
        private readonly LogManager _logManager;

        internal Log(LogManager logManager, string name)
        {
            _logManager = logManager;
            Name = name;
        }

        internal string Name { get; }

        private LogEvent GetLogEventFor(Level level)
        {
            var logEvent = _logManager.AllocateLogEvent();
            logEvent.Initialize(level, this);
            return logEvent;
        }

        internal void Enqueue(LogEvent logEvent)
        {
            _logManager.Enqueue(logEvent);
        }
    }
}
