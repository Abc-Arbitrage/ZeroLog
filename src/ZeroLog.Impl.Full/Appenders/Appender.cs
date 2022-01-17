using System;

namespace ZeroLog.Appenders;

public abstract class Appender : IDisposable
{
    public abstract void WriteMessage(FormattedLogMessage message);

    public virtual void Flush()
    {
    }

    public virtual void Dispose()
    {
    }
}
