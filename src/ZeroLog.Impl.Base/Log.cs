using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ZeroLog;

public sealed partial class Log
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    private LogLevel _logLevel = LogLevel.None;

    internal string Name { get; }

    internal Log(string name)
    {
        Name = name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial bool IsEnabled(LogLevel level);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogMessage ForLevel(LogLevel level)
        => IsEnabled(level)
            ? InternalAcquireLogMessage(level)
            : LogMessage.Empty;

    private partial LogMessage InternalAcquireLogMessage(LogLevel level);

    public override string ToString()
        => Name;

#if NETSTANDARD

    public partial bool IsEnabled(LogLevel level)
        => false;

    private partial LogMessage InternalAcquireLogMessage(LogLevel level)
        => LogMessage.Empty;

#endif
}
