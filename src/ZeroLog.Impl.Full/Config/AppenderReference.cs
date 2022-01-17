using ZeroLog.Appenders;

namespace ZeroLog.Config;

public sealed class AppenderReference
{
    public Appender Appender { get; }
    public Level Level { get; init; }

    public AppenderReference(Appender appender)
    {
        Appender = appender;
    }

    public static implicit operator AppenderReference(Appender appender)
        => new(appender);
}
