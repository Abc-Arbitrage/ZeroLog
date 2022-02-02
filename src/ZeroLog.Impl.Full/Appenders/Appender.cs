using System;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public abstract class Appender : IDisposable
{
    public LogLevel Level { get; init; }

    public abstract void WriteMessage(FormattedLogMessage message);

    public virtual void Flush()
    {
    }

    public virtual void Dispose()
    {
    }
}
