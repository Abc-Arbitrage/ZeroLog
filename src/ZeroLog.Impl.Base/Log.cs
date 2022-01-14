using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ZeroLog;

public sealed partial class Log
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    private Level _logLevel = Level.Fatal + 1;

    internal string Name { get; }

    internal Log(string name)
    {
        Name = name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public partial bool IsEnabled(Level level);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogMessage ForLevel(Level level)
        => IsEnabled(level)
            ? InternalAcquireLogMessage(level)
            : LogMessage.Empty;

    private partial LogMessage InternalAcquireLogMessage(Level level);

    public override string ToString()
        => Name;

#if NETSTANDARD

    public partial bool IsEnabled(Level level)
        => false;

    private partial LogMessage InternalAcquireLogMessage(Level level)
        => LogMessage.Empty;

#endif
}
