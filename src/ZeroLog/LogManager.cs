using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Formatting;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog
{
    public sealed class LogManager : ILogManager, ILogMessageProvider, IDisposable
    {
        internal const int OutputBufferSize = 16 * 1024;

        private static LogManager? _logManager;
        private static readonly ConcurrentDictionary<string, Log> _loggers = new();

        private readonly ConcurrentQueue<LogMessage> _queue;
        private readonly ObjectPool<LogMessage> _pool;

        private readonly BufferSegmentProvider _bufferSegmentProvider;
        private readonly IConfigurationResolver _configResolver;
        private readonly Thread _writeThread;

        private bool _isRunning;
        private IAppender[] _appenders = Array.Empty<IAppender>();

        public Level Level { get; private set; }
        public static ZeroLogConfig Config { get; } = new();

        private LogManager(IConfigurationResolver configResolver, ZeroLogInitializationConfig config)
        {
            config.Validate();

            _configResolver = configResolver;

            _queue = new ConcurrentQueue<LogMessage>(new ConcurrentQueueCapacityInitializer(config.LogEventQueueSize));

            _bufferSegmentProvider = new BufferSegmentProvider(config.LogEventQueueSize * config.LogEventBufferSize, config.LogEventBufferSize);
            _pool = new ObjectPool<LogMessage>(config.LogEventQueueSize, () => new LogMessage(_bufferSegmentProvider.GetSegment(), config.LogEventArgumentCapacity));

            configResolver.Updated += () =>
            {
                UpdateAllLogConfigurations();
                UpdateAppenders();
            };

            UpdateAllLogConfigurations();
            UpdateAppenders();

            _isRunning = true;

            _writeThread = new Thread(WriteThread)
            {
                Name = $"{nameof(ZeroLog)}.{nameof(WriteThread)}"
            };

            _writeThread.Start();
        }

        public void Dispose()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            ResetAllLogConfigurations();

            _pool.Clear();
            _writeThread.Join();
            _pool.Clear();

            if (_pool.IsAnyItemAcquired())
                Thread.Sleep(100); // Can't really do much better here

            _configResolver.Dispose();
            _bufferSegmentProvider.Dispose();
        }

        public static ILogManager Initialize(IConfigurationResolver configResolver, ZeroLogInitializationConfig? config = null)
        {
            if (_logManager is not null)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(configResolver, config ?? new ZeroLogInitializationConfig());
            return _logManager;
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = null;

            logManager?.Dispose();
        }

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
            where T : unmanaged, IStringFormattable
            => UnmanagedCache.Register<T>();

        public static void RegisterUnmanaged<T>(UnmanagedFormatterDelegate<T> formatter)
            where T : unmanaged
            => UnmanagedCache.Register(formatter);

        public static Log GetLogger<T>()
            => GetLogger(typeof(T));

        public static Log GetLogger(Type type)
            => GetLogger(type.FullName!);

        public static Log GetLogger(string name)
        {
            var logManager = _logManager;

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
            => log.UpdateConfiguration(this, _configResolver.ResolveLogConfig(log.Name));

        private void UpdateAllLogConfigurations()
        {
            foreach (var log in _loggers.Values)
                UpdateLogConfiguration(log);
        }

        private static void ResetAllLogConfigurations()
        {
            foreach (var log in _loggers.Values)
                log.UpdateConfiguration(null, default);
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

        private void WriteThread()
        {
            try
            {
                WriteToAppenders();
            }
            catch (Exception ex)
            {
                try
                {
                    Console.Error.WriteLine("Fatal error in ZeroLog." + nameof(WriteThread) + ":");
                    Console.Error.WriteLine(ex);

                    Shutdown();
                }
                catch
                {
                    // Don't kill the process
                }
            }
        }

        private void WriteToAppenders()
        {
            var spinWait = new SpinWait();
            var formattedMessage = new FormattedLogMessage(OutputBufferSize);
            var flush = false;

            while (_isRunning || !_queue.IsEmpty)
            {
                if (TryToProcessQueue(formattedMessage))
                {
                    spinWait.Reset();
                    flush = true;
                    continue;
                }

                if (flush && spinWait.NextSpinWillYield && Config.FlushAppenders)
                {
                    FlushAppenders();
                    flush = false;
                }
                else
                {
                    spinWait.SpinOnce();
                }
            }

            FlushAppenders();
        }

        private bool TryToProcessQueue(FormattedLogMessage formattedLogMessage)
        {
            if (!_queue.TryDequeue(out var logMessage))
                return false;

            try
            {
                if ((logMessage.Logger?.Appenders.Length ?? 0) <= 0)
                    return true;

                try
                {
                    formattedLogMessage.SetMessage(logMessage);
                }
                catch (Exception)
                {
                    // TODO Handle formatting errors
                    return true;
                }

                WriteMessageLogToAppenders(formattedLogMessage);
            }
            finally
            {
                if (logMessage.IsPooled && _isRunning)
                    _pool.Release(logMessage);
            }

            return true;
        }

        private static void WriteMessageLogToAppenders(FormattedLogMessage message)
        {
            foreach (var appender in message.Message.Logger?.Appenders ?? Array.Empty<IAppender>())
            {
                // if (logEvent.Level >= Level) // TODO Check this ? log event should not be in queue if not > Level
                appender.WriteMessage(message);
            }
        }

        private void UpdateAppenders()
        {
            var appenders = _configResolver.GetAllAppenders().ToArray();
            Thread.MemoryBarrier();
            _appenders = appenders;

            Level = _configResolver.ResolveLogConfig(string.Empty).Level;
        }

        private void FlushAppenders()
        {
            foreach (var appender in _appenders)
                appender.Flush();
        }
    }
}
