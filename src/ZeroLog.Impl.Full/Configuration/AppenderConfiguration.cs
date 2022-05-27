using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

/// <summary>
/// An appender configuration. Appenders can be implicitly cast to this class.
/// </summary>
public sealed class AppenderConfiguration
{
    /// <summary>
    /// The appender to use.
    /// </summary>
    public Appender Appender { get; }

    /// <summary>
    /// The minimum log level of messages this appender should handle.
    /// This can be higher than the level defined on the appender.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Creates a configuration for an appender.
    /// </summary>
    /// <param name="appender">The appender to configure.</param>
    public AppenderConfiguration(Appender appender)
    {
        Appender = appender;
        Level = appender.Level;
    }

    /// <summary>
    /// Creates a configuration for an appender.
    /// </summary>
    /// <param name="appender">The appender to configure.</param>
    public static implicit operator AppenderConfiguration(Appender appender)
        => new(appender);

    internal AppenderConfiguration Clone()
        => (AppenderConfiguration)MemberwiseClone();
}
