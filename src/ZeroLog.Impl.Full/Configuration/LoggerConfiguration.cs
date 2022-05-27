using System;
using System.Collections.Generic;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

/// <summary>
/// The hierarchical configuration for a logger.
/// </summary>
public sealed class LoggerConfiguration : ILoggerConfiguration
{
    /// <summary>
    /// The logger name or partial namespace.
    /// </summary>
    public string Name { get; }

    internal string NameWithPeriod { get; }

    /// <summary>
    /// The minimum log level for this level.
    /// </summary>
    /// <remarks>
    /// The level of the parent logger is inherited by default.
    /// </remarks>
    public LogLevel? Level { get; set; }

    /// <summary>
    /// The strategy to apply on log message pool exhaustion.
    /// </summary>
    /// <remarks>
    /// The strategy of the parent logger is inherited by default.
    /// </remarks>
    public LogMessagePoolExhaustionStrategy? LogMessagePoolExhaustionStrategy { get; set; }

    /// <summary>
    /// Indicate whether appenders defined on parent levels should be included.
    /// </summary>
    /// <remarks>
    /// True by default.
    /// </remarks>
    public bool IncludeParentAppenders { get; set; } = true;

    /// <summary>
    /// The appenders to use for this level.
    /// </summary>
    /// <remarks>
    /// Appenders defined on parent levels are included by default unless <see cref="IncludeParentAppenders"/> is set to false.
    /// </remarks>
    public ICollection<AppenderConfiguration> Appenders { get; private set; } = new List<AppenderConfiguration>();

    /// <summary>
    /// Creates a configuration for a logger.
    /// </summary>
    /// <param name="name">The logger name or partial namespace.</param>
    public LoggerConfiguration(string name)
    {
        Name = name;
        NameWithPeriod = $"{name}.";
    }

    /// <summary>
    /// Creates a configuration for a type.
    /// </summary>
    /// <param name="type">The type which uses the logger.</param>
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
                throw new InvalidOperationException($"Multiple appender configurations for the same appender instance (of type {appenderRef.Appender.GetType()}) defined in the following logger configuration: {Name}");
        }
    }

    internal LoggerConfiguration Clone()
    {
        var clone = (LoggerConfiguration)MemberwiseClone();
        clone.Appenders = Appenders.Select(i => i.Clone()).ToList();
        return clone;
    }
}

/// <summary>
/// The configuration of the root logger.
/// </summary>
public sealed class RootLoggerConfiguration : ILoggerConfiguration
{
    /// <summary>
    /// The minimum default log level.
    /// </summary>
    public LogLevel Level { get; set; } = LogLevel.Trace;

    /// <summary>
    /// The strategy to apply on log message pool exhaustion by default.
    /// </summary>
    public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; set; } = LogMessagePoolExhaustionStrategy.Default;

    /// <summary>
    /// The appenders to use by default.
    /// </summary>
    public ICollection<AppenderConfiguration> Appenders { get; private set; } = new List<AppenderConfiguration>();

    internal RootLoggerConfiguration()
    {
    }

    LogLevel? ILoggerConfiguration.Level => Level;
    LogMessagePoolExhaustionStrategy? ILoggerConfiguration.LogMessagePoolExhaustionStrategy => LogMessagePoolExhaustionStrategy;
    bool ILoggerConfiguration.IncludeParentAppenders => false;

    internal void Validate()
    {
        var appenderRefs = new HashSet<Appender>(ReferenceEqualityComparer.Instance);

        foreach (var appenderRef in Appenders)
        {
            if (!appenderRefs.Add(appenderRef.Appender))
                throw new InvalidOperationException($"Multiple appender configurations for the same appender instance (of type {appenderRef.Appender.GetType()}) defined in the root logger configuration.");
        }
    }

    internal RootLoggerConfiguration Clone()
    {
        var clone = (RootLoggerConfiguration)MemberwiseClone();
        clone.Appenders = Appenders.Select(i => i.Clone()).ToList();
        return clone;
    }
}

internal interface ILoggerConfiguration
{
    LogLevel? Level { get; }
    LogMessagePoolExhaustionStrategy? LogMessagePoolExhaustionStrategy { get; }

    bool IncludeParentAppenders { get; }
    ICollection<AppenderConfiguration> Appenders { get; }
}
