using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ZeroLog;

/// <summary>
/// Represents a named logger used by applications to log messages.
/// </summary>
public sealed partial class Log
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    private LogLevel _logLevel = LogLevel.None;

    internal string Name { get; }

    internal Log(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Indicates whether logs of the given level are enabled for this logger.
    /// </summary>
    /// <param name="level">The log level.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial bool IsEnabled(LogLevel level);

    /// <summary>
    /// Returns a log message builder for the given level.
    /// </summary>
    /// <param name="level">The log level.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogMessage ForLevel(LogLevel level)
        => IsEnabled(level)
            ? InternalAcquireLogMessage(level)
            : LogMessage.Empty;

    private partial LogMessage InternalAcquireLogMessage(LogLevel level);

    /// <summary>
    /// Returns the logger name.
    /// </summary>
    public override string ToString()
        => Name;

#if NETSTANDARD

    public partial bool IsEnabled(LogLevel level)
        => false;

    private partial LogMessage InternalAcquireLogMessage(LogLevel level)
        => LogMessage.Empty;

#endif
}
