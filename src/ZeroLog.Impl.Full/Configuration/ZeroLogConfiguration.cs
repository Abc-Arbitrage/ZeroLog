using System;
using System.Collections.Generic;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

/// <summary>
/// The configuration to use when initializing ZeroLog.
/// </summary>
public sealed class ZeroLogConfiguration
{
    internal static ZeroLogConfiguration Default { get; } = new();

    private List<LoggerConfiguration> _loggers = new();

    internal event Action? ApplyChangesRequested;

    /// <summary>
    /// Count of pooled log messages. A log message is acquired from the pool on demand, and released by the logging thread.
    /// </summary>
    /// <remarks>
    /// Default: 1024
    /// </remarks>
    public int LogMessagePoolSize { get; init; } = 1024;

    /// <summary>
    /// The size of the buffer used to serialize log message arguments. Once exceeded, the message is truncated.
    /// All <c>Append</c> calls use a few bytes, except for those with a <c>ReadOnlySpan</c> parameter, which copy the whole value into the buffer.
    /// </summary>
    /// <remarks>
    /// Default: 128
    /// </remarks>
    public int LogMessageBufferSize { get; init; } = 128;

    /// <summary>
    /// The maximum number of <c>Append</c> calls which involve string objects that can be made for a log message.
    /// Note that string objects are also used for format strings and keys of key/value metadata.
    /// </summary>
    /// <remarks>
    /// Default: 32
    /// </remarks>
    public int LogMessageStringCapacity { get; init; } = 32;

    /// <summary>
    /// Automatically registers an enum type when it's logged for the first time. This causes allocations. Use <c>LogManager.RegisterEnum</c> when automatic registration is disabled.
    /// </summary>
    /// <remarks>
    /// Default: false
    /// </remarks>
    public bool AutoRegisterEnums { get; set; } = false;

    /// <summary>
    /// Flag indicating to use a background thread for appending log messages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If a background thread is used the application exit won't wait for <see cref="LogManager.Shutdown"/>.
    /// This allows hooking <c>LogManager.Shutdown</c> calls to <c>AppDomain.CurrentDomain.ProcessExit</c> events as this event is otherwise blocked by a foreground thread.
    /// </para>
    /// <para>
    /// Default: false
    /// </para>
    /// </remarks>
    public bool UseBackgroundThread { get; init; } = false;

    /// <summary>
    /// The string which should be logged instead of a <c>null</c> value.
    /// </summary>
    /// <remarks>
    /// Default: "null"
    /// </remarks>
    public string NullDisplayString { get; set; } = "null";

    /// <summary>
    /// The string which is appended to a message when it is truncated.
    /// </summary>
    /// <remarks>
    /// Default: " [TRUNCATED]"
    /// </remarks>
    public string TruncatedMessageSuffix { get; set; } = " [TRUNCATED]";

    /// <summary>
    /// The time an appender will be put into quarantine (not used to log messages) after it throws an exception.
    /// </summary>
    /// <remarks>
    /// Default: 15 seconds
    /// </remarks>
    public TimeSpan AppenderQuarantineDelay { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The configuration of the root logger.
    /// </summary>
    /// <remarks>
    /// The root logger is the default logger. If <c>GetLogger</c> is called for a namespace which is not configured, it will fallback to the root logger.
    /// </remarks>
    public RootLoggerConfiguration RootLogger { get; private set; } = new();

    /// <summary>
    /// Configuration for logger namespaces (besides the root logger).
    /// </summary>
    /// <remarks>
    /// ZeroLog supports hierarchical loggers. When <c>GetLogger("Foo.Bar.Baz")</c> is called, it will try to find the best matching configuration using a hierarchical namespace-like mode.
    /// If <c>Foo.Bar</c> is configured, but <c>Foo.Bar.Baz</c> is not, it will use the configuration for <c>Foo.Bar</c>.
    /// </remarks>
    public ICollection<LoggerConfiguration> Loggers => _loggers;

    /// <summary>
    /// Applies the changes made to this object since the call to <see cref="LogManager.Initialize"/>
    /// or the last call to <see cref="ApplyChanges"/>.
    /// </summary>
    /// <remarks>
    /// This method allocates.
    /// </remarks>
    public void ApplyChanges()
        => ApplyChangesRequested?.Invoke();

    /// <summary>
    /// Sets the log level of a given logger.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an utility method which updates a logger configuration in <see cref="Loggers"/> or adds one if necessary.
    /// It will update the <see cref="RootLogger"/> if the provided logger name is null or empty.
    /// </para>
    /// <para>
    /// You need to call <see cref="ApplyChanges"/> for the changes to be taken into account after ZeroLog is started.
    /// </para>
    /// </remarks>
    /// <param name="loggerName">The logger name whose configuration to update. Updates the root log level when null or empty.</param>
    /// <param name="logLevel">The new log level.</param>
    /// <seealso cref="LoggerConfiguration.Level"/>
    public void SetLogLevel(string? loggerName, LogLevel? logLevel)
    {
        if (string.IsNullOrEmpty(loggerName))
        {
            RootLogger.Level = logLevel ?? LogLevel.Trace;
            return;
        }

        foreach (var logger in _loggers)
        {
            if (logger.Name == loggerName)
            {
                logger.Level = logLevel;
                return;
            }
        }

        Loggers.Add(new LoggerConfiguration(loggerName)
        {
            Level = logLevel
        });
    }

    internal void Validate()
    {
        RootLogger.Validate();

        var loggerNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var loggerConfig in Loggers)
        {
            if (string.IsNullOrEmpty(loggerConfig.Name))
                throw new InvalidOperationException("Logger configurations must have a logger name.");

            if (!loggerNames.Add(loggerConfig.Name))
                throw new InvalidOperationException($"Multiple configurations defined for the following logger: {loggerConfig.Name}");

            loggerConfig.Validate();
        }
    }

    internal ZeroLogConfiguration Clone()
    {
        var clone = (ZeroLogConfiguration)MemberwiseClone();
        clone.RootLogger = RootLogger.Clone();
        clone._loggers = Loggers.Select(i => i.Clone()).ToList();
        return clone;
    }

    internal ResolvedLoggerConfiguration ResolveLoggerConfiguration(string loggerName)
        => ResolvedLoggerConfiguration.Resolve(loggerName, this);

    internal HashSet<Appender> GetAllAppenders()
        => RootLogger.Appenders
                     .Concat(Loggers.SelectMany(i => i.Appenders))
                     .Select(i => i.Appender)
                     .ToHashSet<Appender>(ReferenceEqualityComparer.Instance);
}
