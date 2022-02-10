using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

public sealed class LoggerConfiguration : ILoggerConfiguration
{
    public string Name { get; }
    internal string NameWithPeriod { get; }

    public LogLevel? Level { get; init; }
    public LogMessagePoolExhaustionStrategy? LogMessagePoolExhaustionStrategy { get; init; }

    public bool IncludeParentAppenders { get; init; } = true;
    public ICollection<AppenderConfiguration> Appenders { get; private set; } = new List<AppenderConfiguration>();

    public LoggerConfiguration(string name)
    {
        Name = name;
        NameWithPeriod = $"{name}.";
    }

    public LoggerConfiguration(Type type)
        : this(type.FullName ?? throw new InvalidOperationException("Invalid type name"))
    {
    }

    internal void ValidateAndFreeze()
    {
        var appenderRefs = new HashSet<Appender>(ReferenceEqualityComparer.Instance);

        foreach (var appenderRef in Appenders)
        {
            if (!appenderRefs.Add(appenderRef.Appender))
                throw new InvalidOperationException($"Multiple appender configurations for the same appender instance (of type {appenderRef.Appender.GetType()}) defined in the following logger configuration: {Name}");
        }

        Appenders = ImmutableList.CreateRange(Appenders);
    }
}

public sealed class RootLoggerConfiguration : ILoggerConfiguration
{
    public LogLevel Level { get; set; } = LogLevel.Trace;
    public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; set; } = LogMessagePoolExhaustionStrategy.Default;

    public ICollection<AppenderConfiguration> Appenders { get; private set; } = new List<AppenderConfiguration>();

    internal RootLoggerConfiguration()
    {
    }

    LogLevel? ILoggerConfiguration.Level => Level;
    LogMessagePoolExhaustionStrategy? ILoggerConfiguration.LogMessagePoolExhaustionStrategy => LogMessagePoolExhaustionStrategy;
    bool ILoggerConfiguration.IncludeParentAppenders => false;

    internal void ValidateAndFreeze()
    {
        var appenderRefs = new HashSet<Appender>(ReferenceEqualityComparer.Instance);

        foreach (var appenderRef in Appenders)
        {
            if (!appenderRefs.Add(appenderRef.Appender))
                throw new InvalidOperationException($"Multiple appender configurations for the same appender instance (of type {appenderRef.Appender.GetType()}) defined in the root logger configuration.");
        }

        Appenders = ImmutableList.CreateRange(Appenders);
    }
}

internal interface ILoggerConfiguration
{
    LogLevel? Level { get; }
    LogMessagePoolExhaustionStrategy? LogMessagePoolExhaustionStrategy { get; }

    bool IncludeParentAppenders { get; }
    ICollection<AppenderConfiguration> Appenders { get; }
}
