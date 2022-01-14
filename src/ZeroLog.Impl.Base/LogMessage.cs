using System;
using System.Threading;

namespace ZeroLog;

public sealed partial class LogMessage
{
    internal static readonly LogMessage Empty = new(string.Empty);

    public Level Level { get; private set; }
    public DateTime Timestamp { get; internal set; }
    public Thread? Thread { get; private set; }
    public Exception? Exception { get; internal set; }

    internal LogMessage(string message)
    {
#if !NETSTANDARD
        // That's definitely not pretty, but partial constructors are not supported :'(
        ConstantMessage = message;
        _strings = Array.Empty<string>();
#endif
    }

    public partial void Log();

#if NETSTANDARD

    public partial void Log()
    {
    }

#endif
}
