namespace ZeroLog.Benchmarks.Logging
{
    internal class BenchmarkLogManager : IInternalLogManager
    {
        private readonly IInternalLogManager _innerLogManager;
        private readonly LogEvent _logEvent;

        public BenchmarkLogManager(IInternalLogManager innerLogManager, ZeroLogInitializationConfig config)
        {
            _innerLogManager = innerLogManager;
            _logEvent = new LogEvent(innerLogManager.GetBufferSegment(), config.LogEventArgumentCapacity);
        }

        public Level Level => _innerLogManager.Level;

        public void Dispose()
        {
            _innerLogManager.Dispose();
        }

        public IInternalLogEvent AcquireLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy) => _logEvent;

        public void Enqueue(IInternalLogEvent logEvent)
        {
        }

        public ILog GetLog(string name) => _innerLogManager.GetLog(name);

        public LogConfig ResolveLogConfig(string name) => _innerLogManager.ResolveLogConfig(name);

        public BufferSegment GetBufferSegment() => _innerLogManager.GetBufferSegment();
    }
}
