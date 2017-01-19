namespace ZeroLog
{
    public partial class Log : ILog
    {
        private readonly IInternalLogManager _logManager;

        internal Log(IInternalLogManager logManager, string name)
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
