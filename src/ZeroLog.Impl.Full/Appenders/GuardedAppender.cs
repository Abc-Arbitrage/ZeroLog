using System;
using ZeroLog.Utils;

namespace ZeroLog.Appenders;

internal class GuardedAppender : IAppender
{
    private readonly TimeSpan _quarantineDelay;
    private DateTime? _nextActivationTime;

    internal IAppender Appender { get; }

    public GuardedAppender(IAppender appender, TimeSpan quarantineDelay)
    {
        Appender = appender;
        _quarantineDelay = quarantineDelay;
        _nextActivationTime = null;
    }

    public void WriteMessage(FormattedLogMessage message)
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

    public void Flush()
        => Appender.Flush();

    public void Dispose()
        => Appender.Dispose();
}
