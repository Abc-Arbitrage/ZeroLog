using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using ZeroLog.Configuration;

namespace ZeroLog;

partial class LogManager : IDisposable
{
    internal const int OutputBufferSize = 16 * 1024;

    private static LogManager? _staticLogManager;

    private Runner? _runner;

    private readonly ZeroLogConfiguration _originalConfig; // Reference to the configuration object supplied by the user
    private ZeroLogConfiguration _config; // Snapshot of the current configuration

    private LogManager(ZeroLogConfiguration config)
    {
        _originalConfig = config;
        _config = config.Clone();

        _runner = config.AppendingStrategy switch
        {
            AppendingStrategy.Asynchronous => new AsyncRunner(_config),
            AppendingStrategy.Synchronous  => new SyncRunner(_config),
            _                              => throw new ArgumentException("Unknown execution mode")
        };

        UpdateAllLogConfigurations();
        _originalConfig.ApplyChangesRequested += ApplyConfigurationChanges;
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

        _originalConfig.ApplyChangesRequested -= ApplyConfigurationChanges;
        ResetAllLogConfigurations();

        runner.Dispose();
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
    /// Registers an enum type.
    /// Member names will be used when formatting the message (instead of numeric values).
    /// </summary>
    /// <param name="enumType">The enum type.</param>
    public static void RegisterEnum(Type enumType)
        => EnumCache.Register(enumType);

    /// <summary>
    /// Registers an enum type.
    /// Member names will be used when formatting the message (instead of numeric values).
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public static void RegisterEnum<T>()
        where T : struct, Enum
        => RegisterEnum(typeof(T));

    /// <summary>
    /// Registers all enum types from the given assembly.
    /// Member names will be used when formatting the message (instead of numeric values).
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> was null.</exception>
    public static void RegisterAllEnumsFrom(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        foreach (var type in assembly.GetTypes().Where(t => t.IsEnum))
            RegisterEnum(type);
    }

    /// <summary>
    /// Registers an unmanaged type which implements <see cref="ISpanFormattable"/>.
    /// The <see cref="ISpanFormattable.TryFormat"/> method will be used when formatting the message (instead of the binary representation).
    /// </summary>
    /// <param name="type">The unmanaged type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> was null.</exception>
    /// <exception cref="ArgumentException"><paramref name="type"/> is not an unmanaged type or does not implement <see cref="ISpanFormattable"/>.</exception>
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
        var newConfig = _originalConfig.Clone();
        newConfig.Validate();

        _config = newConfig;

        UpdateAllLogConfigurations();
        _runner?.UpdateConfiguration(_config);
    }

    private void UpdateLogConfiguration(Log log)
        => log.UpdateConfiguration(_runner, _config);

    private void UpdateAllLogConfigurations()
    {
        foreach (var log in _loggers.Values)
            UpdateLogConfiguration(log);
    }

    private static void ResetAllLogConfigurations()
    {
        foreach (var log in _loggers.Values)
            log.UpdateConfiguration(null, (ResolvedLoggerConfiguration?)null);
    }

    internal void WaitUntilNewConfigurationIsApplied() // For unit tests
        => _runner?.WaitUntilNewConfigurationIsApplied();
}
