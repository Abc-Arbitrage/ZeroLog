using ZeroLog.Configuration;

namespace ZeroLog.Benchmarks.Logging;

internal class BenchmarkLogMessageProvider : ILogMessageProvider
{
    private readonly LogMessage _logMessage;

    public BenchmarkLogMessageProvider(int logMessageBufferSize = 128, int logMessageStringCapacity = 32)
    {
        _logMessage = LogMessage.CreateTestMessage(LogLevel.Trace, logMessageBufferSize, logMessageStringCapacity);
    }

    public LogMessage AcquireLogMessage(LogMessagePoolExhaustionStrategy poolExhaustionStrategy)
        => _logMessage;

    public void Submit(LogMessage message)
    {
    }
}
