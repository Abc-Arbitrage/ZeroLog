using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Configuration;
using ZeroLog.Support;

namespace ZeroLog;

partial class LogManager : IDisposable
{
    internal const int OutputBufferSize = 16 * 1024;

    private static LogManager? _staticLogManager;

    private Runner? _runner;

    private readonly ZeroLogConfiguration _userConfig; // Reference to the configuration object supplied by the user
    private ZeroLogConfiguration _appliedConfig; // Snapshot of the current configuration

    /// <summary>
    /// The configuration being used.
    /// </summary>
    /// <remarks>
    /// This configuration is not necessarily up to date. <see cref="ZeroLogConfiguration.ApplyChanges"/> needs to be called for the changes to be taken into account.
    /// Returns <c>null</c> when the log manager is uninitialized or shut down.
    /// </remarks>
    public static ZeroLogConfiguration? Configuration => _staticLogManager?._userConfig;

    private LogManager(ZeroLogConfiguration config)
    {
        _userConfig = config;
        _appliedConfig = config.Clone();

        _runner = config.AppendingStrategy switch
        {
            AppendingStrategy.Asynchronous => new AsyncRunner(_appliedConfig),
            AppendingStrategy.Synchronous  => new SyncRunner(_appliedConfig),
            _                              => throw new ArgumentException("Unknown execution mode")
        };

        _runner.Start();

        UpdateAllLogConfigurations();
        _userConfig.ApplyChangesRequested += ApplyConfigurationChanges;
    }

    /// <summary>
    /// Disposes the log manager, effectively shutting it down.
    /// </summary>
    public void Dispose()
    {
        var runner = Interlocked.Exchange(ref _runner, null);
        if (runner is null)
            return;

        Interlocked.CompareExchange(ref _staticLogManager, null, this);

        _userConfig.ApplyChangesRequested -= ApplyConfigurationChanges;

        try
        {
            // The appender list of each logger is needed until the runner is stopped, so we can't clear the configuration at this point
            foreach (var log in _loggers.Values)
                log.DisableLogging();

            runner.Dispose();
        }
        finally
        {
            foreach (var log in _loggers.Values)
                log.UpdateConfiguration(null, (ResolvedLoggerConfiguration?)null);
        }
    }

    /// <summary>
    /// Initializes ZeroLog.
    /// </summary>
    /// <param name="configuration">The configuration to use.</param>
    /// <returns>A disposable which shuts down ZeroLog upon disposal.</returns>
    /// <exception cref="InvalidOperationException">ZeroLog is already initialized.</exception>
    /// <remarks>
    /// Any changes to the <paramref name="configuration"/> object will only be taken into account after calling <see cref="ZeroLogConfiguration.ApplyChanges"/>.
    /// </remarks>
    public static IDisposable Initialize(ZeroLogConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configuration.Validate();

        if (_staticLogManager is not null)
            throw new InvalidOperationException("LogManager is already initialized.");

        _staticLogManager = new LogManager(configuration);
        return _staticLogManager;
    }

    /// <summary>
    /// Shuts down ZeroLog.
    /// </summary>
    public static void Shutdown()
        => _staticLogManager?.Dispose();

    /// <summary>
    /// Waits until all the messages logged so far have been processed by the appenders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is mostly meant for benchmarking, to ensure all the enqueued data has been processed.
    /// </para>
    /// <para>
    /// It could also be used for unit testing, but in that case it is recommended to set
    /// the <see cref="ZeroLogConfiguration.AppendingStrategy"/> property in the configuration
    /// to <see cref="AppendingStrategy.Synchronous"/> instead, which removes all threading-related issues.
    /// </para>
    /// </remarks>
    public static void Flush()
        => _staticLogManager?._runner?.Flush();

    /// <summary>
    /// Registers an enum type.
    /// Member names will be used when formatting the message (instead of numeric values).
    /// </summary>
    /// <param name="enumType">The enum type.</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("This code uses reflection which is not compatible with AOT compilation. Use the generic version if possible.")]
#endif
    public static void RegisterEnum(Type enumType)
        => EnumCache.Register(enumType);

    /// <summary>
    /// Registers an enum type.
    /// Member names will be used when formatting the message (instead of numeric values).
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public static void RegisterEnum<T>()
        where T : struct, Enum
        => EnumCache.Register<T>();

    /// <summary>
    /// Registers all enum types from the given assembly.
    /// Member names will be used when formatting the message (instead of numeric values).
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> was null.</exception>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("This code uses reflection which is not compatible with AOT compilation.")]
    [RequiresUnreferencedCode("This code uses reflection which is not compatible with trimming.")]
#endif
    public static void RegisterAllEnumsFrom(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        if (!RuntimeFeature.IsDynamicCodeSupported)
            return;

        foreach (var type in TypeUtil.GetLoadableTypes(assembly).Where(t => t.IsEnum))
            RegisterEnum(type);
    }

    /// <summary>
    /// Registers an unmanaged type which implements <see cref="ISpanFormattable"/>.
    /// The <see cref="ISpanFormattable.TryFormat"/> method will be used when formatting the message (instead of the binary representation).
    /// </summary>
    /// <param name="type">The unmanaged type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> was null.</exception>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not an unmanaged type or does not implement <see cref="ISpanFormattable"/>.</exception>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("This code uses reflection which is not compatible with AOT compilation. Use the generic version if possible.")]
    [RequiresUnreferencedCode("This code uses reflection which is not compatible with trimming.")]
#endif
    public static void RegisterUnmanaged(Type type)
        => UnmanagedCache.Register(type);

    /// <summary>
    /// Registers an unmanaged type which implements <see cref="ISpanFormattable"/>.
    /// The <see cref="ISpanFormattable.TryFormat"/> method will be used when formatting the message (instead of the binary representation).
    /// </summary>
    /// <typeparam name="T">The unmanaged type.</typeparam>
    /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an unmanaged type.</exception>
    public static void RegisterUnmanaged<T>()
        where T : unmanaged, ISpanFormattable
        => UnmanagedCache.Register<T>();

    /// <summary>
    /// Registers a delegate to be used for formatting an unmanaged type.
    /// </summary>
    /// <param name="formatter">The formatter delegate.</param>
    /// <typeparam name="T">The unmanaged type.</typeparam>
    public static void RegisterUnmanaged<T>(UnmanagedFormatterDelegate<T> formatter)
        where T : unmanaged
        => UnmanagedCache.Register(formatter);

    public static partial Log GetLogger(string name)
    {
        var logManager = _staticLogManager;

        return logManager != null
            ? _loggers.GetOrAdd(
                name,
                static (n, mgr) =>
                {
                    var log = new Log(n);
                    mgr.UpdateLogConfiguration(log);
                    return log;
                },
                logManager
            )
            : _loggers.GetOrAdd(
                name,
                static n => new Log(n)
            );
    }

    private void ApplyConfigurationChanges()
    {
        var newConfig = _userConfig.Clone();
        newConfig.Validate();

        _appliedConfig = newConfig;

        UpdateAllLogConfigurations();
        _runner?.UpdateConfiguration(_appliedConfig);
    }

    private void UpdateLogConfiguration(Log log)
        => log.UpdateConfiguration(_runner, _appliedConfig);

    private void UpdateAllLogConfigurations()
    {
        foreach (var log in _loggers.Values)
            UpdateLogConfiguration(log);
    }

    internal void WaitUntilNewConfigurationIsApplied() // For unit tests
        => _runner?.WaitUntilNewConfigurationIsApplied();
}
