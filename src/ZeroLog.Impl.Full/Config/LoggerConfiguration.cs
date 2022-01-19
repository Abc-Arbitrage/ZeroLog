using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ZeroLog.Appenders;

namespace ZeroLog.Config;

public sealed class LoggerConfiguration
{
    public string Name { get; }
    internal string NameWithPeriod { get; }

    public Level? Level { get; set; }
    public LogMessagePoolExhaustionStrategy? LogMessagePoolExhaustionStrategy { get; set; }

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
