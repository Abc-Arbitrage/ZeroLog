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
            _logManager = logManager;
            Name = name;
            Appenders = _logManager?.ResolveAppenders(name);
            LogEventPoolExhaustionStrategy = _logManager?.ResolveLogEventPoolExhaustionStrategy(name) ?? LogEventPoolExhaustionStrategy.Default;
            _specialLogMessage = new ForwardingLogEvent(new SpecialLogEvents.SpecialLogEvent(Level.Fatal, "Log message skipped due to LogEvent pool exhaustion.", this).LogEvent);
        }

        internal string Name { get; }

        public IList<IAppender> Appenders { get; }
        public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; }

        public bool IsLevelEnabled(Level level) => level >= _logManager.Level;

        public ILogEvent ForLevel(Level level)
        {
            return IsLevelEnabled(level)
                ? GetLogEventFor(level)
                : NoopLogEvent.Instance;
        }

        private IInternalLogEvent GetLogEventFor(Level level)
        {
            var logEvent = _logManager.AllocateLogEvent(LogEventPoolExhaustionStrategy, _specialLogMessage);
            logEvent.Initialize(level, this);
            return logEvent;
        }

        internal void Enqueue(IInternalLogEvent logEvent)
        {
            _logManager.Enqueue(logEvent);
        }
    }
}
