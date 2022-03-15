using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using ZeroLog.Configuration;
using ZeroLog.Support;

namespace ZeroLog;

partial class LogManager : ILogMessageProvider, IDisposable
{
    internal const int OutputBufferSize = 16 * 1024;

    private static LogManager? _staticLogManager;

    private readonly ZeroLogConfiguration _config;
    private readonly ConcurrentQueue<LogMessage> _queue;
    private readonly ObjectPool<LogMessage> _pool;

    private readonly AppenderThread _appenderThread;

    private bool _isRunning;

    private LogManager(ZeroLogConfiguration config)
    {
        _config = config;

        _queue = new ConcurrentQueue<LogMessage>(new ConcurrentQueueCapacityInitializer(config.LogMessagePoolSize));

        var bufferSegmentProvider = new BufferSegmentProvider(config.LogMessagePoolSize, config.LogMessageBufferSize);
        _pool = new ObjectPool<LogMessage>(config.LogMessagePoolSize, () => new LogMessage(bufferSegmentProvider.GetSegment(), config.LogMessageStringCapacity, true));

        UpdateAllLogConfigurations();

        _isRunning = true;

        // Instantiate this last
        _appenderThread = new AppenderThread(this);
    }

    public void Dispose()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        Interlocked.CompareExchange(ref _staticLogManager, null, this);

        ResetAllLogConfigurations();

        _pool.Clear();
        _appenderThread.Join();
        _pool.Clear();

        if (_pool.IsAnyItemAcquired())
            Thread.Sleep(100); // Can't really do much better here

        _appenderThread.DisposeAppenders();
    }

    public static IDisposable Initialize(ZeroLogConfiguration configuration)
    {
        configuration.ValidateAndFreeze();

        if (_staticLogManager is not null)
            throw new ApplicationException("LogManager is already initialized");

        _staticLogManager = new LogManager(configuration);
        return _staticLogManager;
    }

    public static void Shutdown()
        => _staticLogManager?.Dispose();

    public static void RegisterEnum(Type enumType)
        => EnumCache.Register(enumType);

    public static void RegisterEnum<T>()
        where T : struct, Enum
        => RegisterEnum(typeof(T));

    public static void RegisterAllEnumsFrom(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        foreach (var type in assembly.GetTypes().Where(t => t.IsEnum))
            RegisterEnum(type);
    }

    public static void RegisterUnmanaged(Type type)
        => UnmanagedCache.Register(type);

    public static void RegisterUnmanaged<T>()
        where T : unmanaged, ISpanFormattable
        => UnmanagedCache.Register<T>();

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

    internal void UpdateLogConfiguration(Log log)
        => log.UpdateConfiguration(this, _config.ResolveLoggerConfiguration(log.Name));

    private void UpdateAllLogConfigurations()
    {
        foreach (var log in _loggers.Values)
            UpdateLogConfiguration(log);
    }

    private static void ResetAllLogConfigurations()
    {
        foreach (var log in _loggers.Values)
            log.UpdateConfiguration(null, null);
    }

    LogMessage? ILogMessageProvider.TryAcquireLogMessage()
    {
        if (_pool.TryAcquire(out var logMessage))
            return logMessage;

        if (!_isRunning)
            return LogMessage.Empty;

        return null;
    }

    void ILogMessageProvider.Submit(LogMessage message)
        => _queue.Enqueue(message);
}
