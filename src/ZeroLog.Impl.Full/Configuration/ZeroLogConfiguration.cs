using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

public sealed class ZeroLogConfiguration
{
    internal static ZeroLogConfiguration Default { get; } = new();

    public int LogMessagePoolSize { get; init; } = 1024;
    public int LogMessageBufferSize { get; init; } = 128;
    public int LogMessageStringCapacity { get; init; } = 32;

    public bool AutoRegisterEnums { get; init; }

    public string NullDisplayString { get; init; } = "null";
    public string TruncatedMessageSuffix { get; init; } = " [TRUNCATED]";
    public string JsonSeparator { get; init; } = " ~~ ";

    public LoggerConfiguration RootLogger { get; } = new(string.Empty);
    public ICollection<LoggerConfiguration> Loggers { get; private set; } = new List<LoggerConfiguration>();

    internal void ValidateAndFreeze()
    {
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
        => Loggers.Append(RootLogger)
                  .SelectMany(i => i.Appenders)
                  .Select(i => i.Appender)
                  .DistinctBy(i => i, ReferenceEqualityComparer.Instance);
}
