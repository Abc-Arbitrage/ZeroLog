using ZeroLog.Appenders;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal partial class Log : ILog
    {
        private readonly IInternalLogManager _logManager;
        private readonly ForwardingLogEvent _skippedMessageLogEvent;
        private Level _logLevel;

        internal string Name { get; }

        public IAppender[] Appenders { get; private set; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; private set; }
        public LogEventArgumentExhaustionStrategy LogEventArgumentExhaustionStrategy { get; private set; }

        internal Log(IInternalLogManager logManager, string name)
        {
            Name = name;
            _logManager = logManager;

            var logEvent = CreateUnpooledLogEvent();
            _skippedMessageLogEvent = new ForwardingLogEvent(logEvent);
            _skippedMessageLogEvent.Initialize(Level.Fatal, this, LogEventArgumentExhaustionStrategy.Allocate);

            ResetConfiguration();
        }

        private IInternalLogEvent CreateUnpooledLogEvent()
        {
            var bufferSegment = _logManager.GetBufferSegment();
            var logEvent = new UnpooledLogEvent(bufferSegment, 1);
            logEvent.Initialize(Level.Fatal, this, LogEventArgumentExhaustionStrategy.Allocate);
            logEvent.Append("Log message skipped due to LogEvent pool exhaustion.");
            return logEvent;
        }

        internal void ResetConfiguration()
        {
            var config = _logManager?.ResolveLogConfig(Name);

            Appenders = config?.Appenders ?? ArrayUtil.Empty<IAppender>();
            LogEventPoolExhaustionStrategy = config?.LogEventPoolExhaustionStrategy ?? default;
            LogEventArgumentExhaustionStrategy = config?.LogEventArgumentExhaustionStrategy ?? default;
            _logLevel = config?.Level ?? Level.Fatal;
        }

        public bool IsLevelEnabled(Level level) => level >= _logLevel;

        public ILogEvent ForLevel(Level level) => IsLevelEnabled(level)
            ? GetLogEventFor(level)
            : NoopLogEvent.Instance;

        private IInternalLogEvent GetLogEventFor(Level level)
        {
            var logEvent = _logManager.AcquireLogEvent(LogEventPoolExhaustionStrategy);

            if (logEvent != null)
                logEvent.Initialize(level, this, LogEventArgumentExhaustionStrategy);
            else
                logEvent = _skippedMessageLogEvent;

            return logEvent;
        }

        internal void Enqueue(IInternalLogEvent logEvent)
        {
            _logManager.Enqueue(logEvent);
        }
    }
}
