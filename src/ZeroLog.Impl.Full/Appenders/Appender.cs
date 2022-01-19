using System;

namespace ZeroLog.Appenders;

public abstract class Appender : IDisposable
{
    public Level Level { get; init; }

    public abstract void WriteMessage(FormattedLogMessage message);

    public virtual void Flush()
    {
    }

    public virtual void Dispose()
    {
    }
}
