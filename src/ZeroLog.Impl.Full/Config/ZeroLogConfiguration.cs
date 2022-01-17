using System;
using System.Collections.Generic;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog.Config;

public sealed class ZeroLogConfiguration
{
    public int LogMessagePoolSize { get; init; } = 1024;
    public int LogMessageBufferSize { get; init; } = 128;
    public int LogMessageStringCapacity { get; init; } = 32;

    public bool AutoRegisterEnums { get; init; }

    public string NullDisplayString { get; init; } = "null";
    public string TruncatedMessageSuffix { get; init; } = " [TRUNCATED]";
    public string JsonSeparator { get; init; } = " ~~ ";

    public LoggerConfiguration RootLogger { get; } = new(string.Empty);
    public ICollection<LoggerConfiguration> Loggers { get; } = new List<LoggerConfiguration>();

    internal void Validate()
    {
        var loggerNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var loggerConfig in Loggers)
        {
            if (!loggerNames.Add(loggerConfig.Name))
                throw new InvalidOperationException($"Multiple configurations defined for the following logger: {loggerConfig.Name}");

            loggerConfig.Validate();
        }
    }

    internal ResolvedLoggerConfiguration ResolveLoggerConfiguration(string loggerName)
        => ResolvedLoggerConfiguration.Resolve(loggerName, this);

    internal IEnumerable<Appender> GetAllAppenders()
        => Loggers.Append(RootLogger)
                  .SelectMany(i => i.Appenders)
                  .Select(i => i.Appender)
                  .DistinctBy(i => i, ReferenceEqualityComparer.Instance);
}
