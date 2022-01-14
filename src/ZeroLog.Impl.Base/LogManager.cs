using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZeroLog;

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
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
        => _loggers.GetOrAdd(name, static n => new Log(n));

#endif
}
