using System;
using System.Diagnostics;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public abstract class Appender : IDisposable
{
    private readonly Stopwatch _quarantineStopwatch = new();
    private bool _needsFlush;

    public LogLevel Level { get; init; }

    public abstract void WriteMessage(LoggedMessage message);

    public virtual void Flush()
    {
    }

    public virtual void Dispose()
    {
    }

    internal void InternalWriteMessage(LoggedMessage message, ZeroLogConfiguration config)
    {
        if (_quarantineStopwatch.IsRunning && _quarantineStopwatch.Elapsed < config.AppenderQuarantineDelay)
            return;

        try
        {
            _needsFlush = true;
            WriteMessage(message);
            _quarantineStopwatch.Stop();
        }
        catch
        {
            _quarantineStopwatch.Restart();
        }
    }

    internal void InternalFlush()
    {
        if (!_needsFlush)
            return;

        try
        {
            _needsFlush = false;
            Flush();
        }
        catch
        {
            _quarantineStopwatch.Restart();
        }
    }
}
