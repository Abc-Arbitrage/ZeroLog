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
            _specialLogMessage = new ForwardingLogEvent(new SpecialLogEvents.SpecialLogEvent(Level.Fatal, "Log message skipped due to LogEvent pool exhaustion.", this).LogEvent);

            ResetConfiguration();
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
