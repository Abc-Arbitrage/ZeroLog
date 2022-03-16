using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

/// <summary>
/// The configuration to use when initializing ZeroLog.
/// </summary>
public sealed class ZeroLogConfiguration
{
    internal static ZeroLogConfiguration Default { get; } = new();

    /// <summary>
    /// Count of pooled log messages. A log message is acquired from the pool on demand, and released by the logging thread.
    /// </summary>
    /// <remarks>
    /// Default: 1024
    /// </remarks>
    public int LogMessagePoolSize { get; init; } = 1024;

    /// <summary>
    /// The size of the buffer used to serialize log message arguments. Once exceeded, the message is truncated.
    /// All Append calls use a few bytes, except for <c>AppendAsciiString</c> which copies the whole string into the buffer.
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
    public bool AutoRegisterEnums { get; init; } = false;

    /// <summary>
    /// The string which should be logged instead of a <c>null</c> value.
    /// </summary>
    /// <remarks>
    /// Default: "null"
    /// </remarks>
    public string NullDisplayString { get; init; } = "null";

    /// <summary>
    /// The string which is appended to a message when it is truncated.
    /// </summary>
    /// <remarks>
    /// Default: " [TRUNCATED]"
    /// </remarks>
    public string TruncatedMessageSuffix { get; init; } = " [TRUNCATED]";

    /// <summary>
    /// The time an appender will be put into quarantine (not used to log messages) after it throws an exception.
    /// </summary>
    /// <remarks>
    /// Default: 15 seconds
    /// </remarks>
    public TimeSpan AppenderQuarantineDelay { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The configuration of the root logger.
    /// </summary>
    /// <remarks>
    /// The root logger is the default logger. If <c>GetLogger</c> is called for a namespace which is not configured, it will fallback to the root logger.
    /// </remarks>
    public RootLoggerConfiguration RootLogger { get; } = new();

    /// <summary>
    /// Configuration for logger namespaces (besides the root logger).
    /// </summary>
    /// <remarks>
    /// ZeroLog supports hierarchical loggers. When <c>GetLogger("Foo.Bar.Baz")</c> is called, it will try to find the best matching configuration using a hierarchical namespace-like mode.
    /// If <c>Foo.Bar</c> is configured, but <c>Foo.Bar.Baz</c> is not, it will use the configuration for <c>Foo.Bar</c>.
    /// </remarks>
    public ICollection<LoggerConfiguration> Loggers { get; private set; } = new List<LoggerConfiguration>();

    internal void ValidateAndFreeze()
    {
        RootLogger.ValidateAndFreeze();

        var loggerNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var loggerConfig in Loggers)
        {
            if (string.IsNullOrEmpty(loggerConfig.Name))
                throw new InvalidOperationException("Logger configurations must have a logger name.");

            if (!loggerNames.Add(loggerConfig.Name))
                throw new InvalidOperationException($"Multiple configurations defined for the following logger: {loggerConfig.Name}");

            loggerConfig.ValidateAndFreeze();
        }

        Loggers = ImmutableList.CreateRange(Loggers);
    }

    internal ResolvedLoggerConfiguration ResolveLoggerConfiguration(string loggerName)
        => ResolvedLoggerConfiguration.Resolve(loggerName, this);

    internal IEnumerable<Appender> GetAllAppenders()
        => RootLogger.Appenders
                     .Concat(Loggers.SelectMany(i => i.Appenders))
                     .Select(i => i.Appender)
                     .DistinctBy(i => i, ReferenceEqualityComparer.Instance);
}
