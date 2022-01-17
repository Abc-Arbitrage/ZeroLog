using System;
using ZeroLog.Utils;

namespace ZeroLog.Appenders;

internal class GuardedAppender : Appender
{
    private readonly TimeSpan _quarantineDelay;
    private DateTime? _nextActivationTime;

    internal Appender Appender { get; }

    public GuardedAppender(Appender appender, TimeSpan quarantineDelay)
    {
        Appender = appender;
        _quarantineDelay = quarantineDelay;
        _nextActivationTime = null;
    }

    public override void WriteMessage(FormattedLogMessage message)
    {
        if (_nextActivationTime.HasValue && _nextActivationTime.Value > SystemDateTime.UtcNow)
            return;

        try
        {
            Appender.WriteMessage(message);
            _nextActivationTime = null;
        }
        catch (Exception)
        {
            _nextActivationTime = SystemDateTime.UtcNow + _quarantineDelay;
        }
    }

    public override void Flush()
    {
        Appender.Flush();
        base.Flush();
    }

    public override void Dispose()
    {
        Appender.Dispose();
        base.Dispose();
    }
}
