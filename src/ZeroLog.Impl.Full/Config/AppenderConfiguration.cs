using ZeroLog.Appenders;

namespace ZeroLog.Config;

public sealed class AppenderConfiguration
{
    public Appender Appender { get; }
    public Level Level { get; init; }

    public AppenderConfiguration(Appender appender)
    {
        Appender = appender;
        Level = appender.Level;
    }

    public static implicit operator AppenderConfiguration(Appender appender)
        => new(appender);
}
