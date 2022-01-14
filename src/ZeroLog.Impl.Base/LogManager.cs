using System;
using System.Collections.Concurrent;

namespace ZeroLog;

public sealed partial class LogManager
{
    private static readonly ConcurrentDictionary<string, Log> _loggers = new();

    public static Log GetLogger<T>()
        => GetLogger(typeof(T));

    public static Log GetLogger(Type type)
        => GetLogger(type.FullName!);

    public static partial Log GetLogger(string name);

#if NETSTANDARD

    public static partial Log GetLogger(string name)
    {
        return _loggers.GetOrAdd(
            name,
            static n => new Log(n)
        );
    }

#endif
}
