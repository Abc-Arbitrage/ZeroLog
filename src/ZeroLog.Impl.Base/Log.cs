using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZeroLog;

/// <summary>
/// Represents a named logger used by applications to log messages.
/// </summary>
public sealed partial class Log
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    private LogLevel _logLevel = LogLevel.None;

    internal string Name { get; }
    internal string CompactName { get; }

    internal Log(string name)
    {
        Name = name;
        CompactName = GetCompactName(name);
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

    internal static string GetCompactName(string? name)
    {
        name = name?.Trim('.');

        if (name is null or "")
            return string.Empty;

        var lastDotIndex = name.LastIndexOf('.');
        if (lastDotIndex < 0)
            return name;

        var sb = new StringBuilder();
        var nextChar = 0;

        while (nextChar < lastDotIndex)
        {
            var c = name[nextChar];

            if (c == '.')
            {
                ++nextChar;
                continue;
            }

            sb.Append(c);

            var nextDot = name.IndexOf('.', nextChar + 1);
            if (nextDot < 0)
                break;

            nextChar = nextDot + 1;
        }

        sb.Append(name, lastDotIndex, name.Length - lastDotIndex);
        return sb.ToString();
    }

#if NETSTANDARD

    public partial bool IsEnabled(LogLevel level)
        => false;

    private partial LogMessage InternalAcquireLogMessage(LogLevel level)
        => LogMessage.Empty;

#endif
}
