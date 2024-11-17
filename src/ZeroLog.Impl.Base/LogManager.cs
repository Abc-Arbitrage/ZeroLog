using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZeroLog;

/// <summary>
/// The entry point of ZeroLog.
/// </summary>
[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
public sealed partial class LogManager
{
    private static readonly ConcurrentDictionary<string, Log> _loggers = new();

    /// <summary>
    /// Returns a logger for the given type.
    /// </summary>
    /// <remarks>
    /// The logger name will be the full name of the type.
    /// </remarks>
    /// <typeparam name="T">The type.</typeparam>
    public static Log GetLogger<T>()
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        => GetLogger(typeof(T));

    /// <summary>
    /// Returns a logger for the given type.
    /// </summary>
    /// <remarks>
    /// The logger name will be the full name of the type.
    /// </remarks>
    /// <param name="type">The type.</param>
    public static Log GetLogger(Type type)
        => GetLogger(type.FullName!);

    /// <summary>
    /// Returns a logger for the given name.
    /// </summary>
    /// <param name="name">The logger name.</param>
    public static partial Log GetLogger(string name);

#if NETSTANDARD

    public static partial Log GetLogger(string name)
        => _loggers.GetOrAdd(name, static n => new Log(n));

#endif
}
