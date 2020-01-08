using System;
using ZeroLog.Appenders;

namespace ZeroLog
{
    internal partial class NoopLogManager : IInternalLogManager
    {
        public Level Level { get; } = Level.Finest;

        public IInternalLogEvent AcquireLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy)
            => throw new NotSupportedException();

        public void Enqueue(IInternalLogEvent logEvent)
            => throw new NotSupportedException();

        public ILog GetLog(string name)
            => NoopLog.Instance;

        public LogConfig ResolveLogConfig(string name)
        {
            return new LogConfig
            {
                Appenders = NoopLog.Instance.Appenders,
                Level = Level.Fatal,
                LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.Default,
                LogEventArgumentExhaustionStrategy = LogEventArgumentExhaustionStrategy.Default
            };
        }

        public BufferSegment GetBufferSegment() => throw new NotSupportedException();

        public void Dispose()
        {
        }

        private partial class NoopLog : ILog
        {
            public static NoopLog Instance { get; } = new NoopLog();

            public IAppender[] Appenders { get; } = Array.Empty<IAppender>();

            public bool IsLevelEnabled(Level level) => false;

            public ILogEvent ForLevel(Level level) => NoopLogEvent.Instance;

            private NoopLog()
            {
            }
        }
    }
}
