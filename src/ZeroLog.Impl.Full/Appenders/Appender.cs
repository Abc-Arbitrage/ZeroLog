using System;
using System.Diagnostics;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public abstract class Appender : IDisposable
{
    private readonly Stopwatch _quarantineStopwatch = new();

    public LogLevel Level { get; init; }

    public abstract void WriteMessage(FormattedLogMessage message);

    public virtual void Flush()
    {
    }

    public virtual void Dispose()
    {
    }

    internal void InternalWriteMessage(FormattedLogMessage message, ZeroLogConfiguration config)
    {
        if (_quarantineStopwatch.IsRunning && _quarantineStopwatch.Elapsed < config.AppenderQuarantineDelay)
            return;

        try
        {
            WriteMessage(message);
            _quarantineStopwatch.Stop();
        }
        catch
        {
            _quarantineStopwatch.Restart();
        }
    }
}
