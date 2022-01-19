using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

public sealed class AppenderConfiguration
{
    public Appender Appender { get; }
    public LogLevel Level { get; init; }

    public AppenderConfiguration(Appender appender)
    {
        Appender = appender;
        Level = appender.Level;
    }

    public static implicit operator AppenderConfiguration(Appender appender)
        => new(appender);
}
