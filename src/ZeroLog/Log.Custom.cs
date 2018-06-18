using ZeroLog.Appenders;

namespace ZeroLog
{
    internal partial class Log : ILog
    {
        private readonly IInternalLogManager _logManager;
        private readonly ForwardingLogEvent _specialLogMessage;
        private Level _logLevel;

        internal string Name { get; }

        public IAppender[] Appenders { get; private set; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; private set; }

        internal Log(IInternalLogManager logManager, string name)
        {
            Name = name;
            _logManager = logManager;

            var logEvent = CreateUnpooledLogEvent();
            _specialLogMessage = new ForwardingLogEvent(logEvent);
            _specialLogMessage.Initialize(Level.Fatal, this);

            ResetConfiguration();
        }

        private IInternalLogEvent CreateUnpooledLogEvent()
        {
            var bufferSegment = _logManager.GetBufferSegment();
            var logEvent = new UnpooledLogEvent(bufferSegment);
            logEvent.Initialize(Level.Fatal, this);
            logEvent.Append("Log message skipped due to LogEvent pool exhaustion.");
            return logEvent;
        }

        internal void ResetConfiguration()
        {
            Appenders = _logManager?.ResolveAppenders(Name);
            LogEventPoolExhaustionStrategy = _logManager?.ResolveLogEventPoolExhaustionStrategy(Name) ?? LogEventPoolExhaustionStrategy.Default;
            _logLevel = _logManager?.ResolveLevel(Name) ?? Level.Fatal;
        }

        public bool IsLevelEnabled(Level level) => level >= _logLevel;

        public ILogEvent ForLevel(Level level) => IsLevelEnabled(level)
            ? GetLogEventFor(level)
            : NoopLogEvent.Instance;

        private IInternalLogEvent GetLogEventFor(Level level) => _logManager.AllocateLogEvent(LogEventPoolExhaustionStrategy, _specialLogMessage, level, this);

        internal void Enqueue(IInternalLogEvent logEvent)
        {
            _logManager.Enqueue(logEvent);
        }
    }
}
