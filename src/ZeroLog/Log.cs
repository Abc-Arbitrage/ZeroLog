using System.Collections.Generic;
using ZeroLog.Appenders;

namespace ZeroLog
{
    internal partial class Log : ILog
    {
        private readonly IInternalLogManager _logManager;
        private readonly ForwardingLogEvent _specialLogMessage;

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
            LogLevel = _logManager?.ResolveLevel(Name) ?? Level.Fatal;
        }

        internal string Name { get; }

        public IList<IAppender> Appenders { get; private set; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; private set; }
        private Level LogLevel { get; set; }

        public bool IsLevelEnabled(Level level) => level >= LogLevel;

        public ILogEvent ForLevel(Level level)
        {
            return IsLevelEnabled(level)
                ? GetLogEventFor(level)
                : NoopLogEvent.Instance;
        }

        private IInternalLogEvent GetLogEventFor(Level level)
        {
            return _logManager.AllocateLogEvent(LogEventPoolExhaustionStrategy, _specialLogMessage, level, this);
        }

        internal void Enqueue(IInternalLogEvent logEvent)
        {
            _logManager.Enqueue(logEvent);
        }
    }
}
