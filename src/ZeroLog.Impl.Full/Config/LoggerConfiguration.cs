using System;
using System.Collections.Generic;
using ZeroLog.Appenders;

namespace ZeroLog.Config;

public sealed class LoggerConfiguration
{
    public string Name { get; }
    internal string NameWithPeriod { get; }

    public Level Level { get; init; }
    public bool IncludeParentAppenders { get; init; } = true;
    public LogMessagePoolExhaustionStrategy? LogMessagePoolExhaustionStrategy { get; set; }
    public ICollection<AppenderReference> Appenders { get; } = new List<AppenderReference>();

    public LoggerConfiguration(string name)
    {
        Name = name;
        NameWithPeriod = $"{name}.";
    }

    public LoggerConfiguration(Type type)
        : this(type.FullName ?? throw new InvalidOperationException("Invalid type name"))
    {
    }

    internal void Validate()
    {
        var appenderRefs = new HashSet<Appender>(ReferenceEqualityComparer.Instance);

        foreach (var appenderRef in Appenders)
        {
            if (!appenderRefs.Add(appenderRef.Appender))
                throw new InvalidOperationException($"Multiple appender configurations for the same appender ({appenderRef.Appender.GetType()}) instance defined in the following logger configuration: {Name}");
        }
    }
}
