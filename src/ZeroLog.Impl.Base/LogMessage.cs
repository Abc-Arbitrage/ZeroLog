using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace ZeroLog;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public sealed partial class LogMessage
{
    public LogLevel Level { get; private set; }
    public DateTime Timestamp { get; internal set; }
    public Thread? Thread { get; private set; }
    public Exception? Exception { get; internal set; }

    public LogMessage WithException(Exception? exception)
    {
        Exception = exception;
        return this;
    }

    public partial void Log();

#if NETSTANDARD

    internal static readonly LogMessage Empty = new();

    private LogMessage()
    {
    }

    public partial void Log()
    {
    }

    public override string ToString()
        => string.Empty;

#endif
}
