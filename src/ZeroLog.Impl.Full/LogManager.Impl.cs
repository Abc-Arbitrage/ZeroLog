using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.Config;

namespace ZeroLog
{
    partial class LogManager : ILogMessageProvider, IDisposable
    {
        internal const int OutputBufferSize = 16 * 1024;

        private static LogManager? _logManager;

        private readonly ZeroLogConfiguration _config;
        private readonly ConcurrentQueue<LogMessage> _queue;
        private readonly ObjectPool<LogMessage> _pool;
        private readonly Appender[] _appenders;

        private readonly BufferSegmentProvider _bufferSegmentProvider;
        private readonly Thread _writeThread;

        private bool _isRunning;

        public static ZeroLogConfig Config { get; } = new();

        private LogManager(ZeroLogConfiguration config)
        {
            _config = config;

            _queue = new ConcurrentQueue<LogMessage>(new ConcurrentQueueCapacityInitializer(config.LogMessagePoolSize));

            _bufferSegmentProvider = new BufferSegmentProvider(config.LogMessagePoolSize * config.LogMessageBufferSize, config.LogMessageBufferSize);
            _pool = new ObjectPool<LogMessage>(config.LogMessagePoolSize, () => new LogMessage(_bufferSegmentProvider.GetSegment(), config.LogMessageStringCapacity));
            _appenders = _config.GetAllAppenders().ToArray();

            UpdateAllLogConfigurations();

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
            Interlocked.CompareExchange(ref _logManager, null, this);

            ResetAllLogConfigurations();

            _pool.Clear();
            _writeThread.Join();
            _pool.Clear();

            if (_pool.IsAnyItemAcquired())
                Thread.Sleep(100); // Can't really do much better here

            _bufferSegmentProvider.Dispose();

            foreach (var appender in _appenders)
                appender.Dispose();
        }

        public static IDisposable Initialize(ZeroLogConfiguration configuration)
        {
            configuration.Validate();

            if (_logManager is not null)
                throw new ApplicationException("LogManager is already initialized");

            Config.UpdateFrom(configuration);

            _logManager = new LogManager(configuration);
            return _logManager;
        }

        public static void Shutdown()
            => _logManager?.Dispose();

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
                var appenders = logMessage.Logger?.GetAppenders(logMessage.Level) ?? Array.Empty<Appender>();
                if (appenders.Length == 0)
                    return true;

                formattedLogMessage.SetMessage(logMessage);

                foreach (var appender in appenders)
                    appender.WriteMessage(formattedLogMessage);
            }
            finally
            {
                if (logMessage.IsPooled && _isRunning)
                    _pool.Release(logMessage);
            }

            return true;
        }

        private void FlushAppenders()
        {
            foreach (var appender in _appenders)
                appender.Flush();
        }
    }
}
