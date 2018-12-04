using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using JetBrains.Annotations;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;
using ZeroLog.Utils;

namespace ZeroLog
{
    public class LogManager : IInternalLogManager
    {
        private static readonly IInternalLogManager _noOpLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _noOpLogManager;

        private readonly ConcurrentDictionary<string, Log> _loggers = new ConcurrentDictionary<string, Log>();
        private readonly ConcurrentQueue<IInternalLogEvent> _queue;
        private readonly ObjectPool<IInternalLogEvent> _pool;

        private readonly BufferSegmentProvider _bufferSegmentProvider;
        private readonly IConfigurationResolver _configResolver;
        private readonly Thread _writeThread;

        private bool _isRunning;
        private readonly Encoding _encoding = Encoding.UTF8;
        private IAppender[] _appenders = ArrayUtil.Empty<IAppender>();

        public static ZeroLogConfig Config { get; } = new ZeroLogConfig();

        internal LogManager(IConfigurationResolver configResolver, ZeroLogInitializationConfig config)
        {
            _configResolver = configResolver;

            _queue = new ConcurrentQueue<IInternalLogEvent>(new ConcurrentQueueCapacityInitializer(config.LogEventQueueSize));

            _bufferSegmentProvider = new BufferSegmentProvider(config.LogEventQueueSize * config.LogEventBufferSize, config.LogEventBufferSize);
            _pool = new ObjectPool<IInternalLogEvent>(config.LogEventQueueSize, () => new LogEvent(_bufferSegmentProvider.GetSegment(), config.LogEventArgumentCapacity));

            configResolver.Initialize(_encoding);
            configResolver.Updated += () =>
            {
                foreach (var logger in _loggers.Values)
                    logger.ResetConfiguration();

                UpdateAppenders();
            };

            UpdateAppenders();

            _isRunning = true;

            _writeThread = new Thread(WriteThread)
            {
                Name = $"{nameof(ZeroLog)}.{nameof(WriteThread)}"
            };

            _writeThread.Start();
        }

        public Level Level { get; private set; }

        public static ILogManager Initialize(IConfigurationResolver configResolver, [CanBeNull] ZeroLogInitializationConfig config = null)
        {
            if (_logManager != _noOpLogManager)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(configResolver, config ?? new ZeroLogInitializationConfig());
            return _logManager;
        }

        [Obsolete("Use the overload with the " + nameof(ZeroLogInitializationConfig) + " parameter")]
        public static ILogManager Initialize(IConfigurationResolver configResolver, int logEventQueueSize, int logEventBufferSize = 128)
        {
            return Initialize(
                configResolver,
                new ZeroLogInitializationConfig
                {
                    LogEventQueueSize = logEventQueueSize,
                    LogEventBufferSize = logEventBufferSize
                }
            );
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = _noOpLogManager;

            logManager?.Dispose();
        }

        public static void RegisterEnum([NotNull] Type enumType)
            => EnumCache.Register(enumType);

        public static void RegisterEnum<T>()
            where T : struct, Enum
            => RegisterEnum(typeof(T));

        public static void RegisterAllEnumsFrom([NotNull] Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            foreach (var type in assembly.GetTypes().Where(t => t.IsEnum))
                RegisterEnum(type);
        }

        public void Dispose()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _writeThread.Join();

            _configResolver.Dispose();
            _bufferSegmentProvider.Dispose();
        }

        public static ILog GetLogger<T>()
            => GetLogger(typeof(T));

        public static ILog GetLogger(Type type)
            => GetLogger(type.FullName);

        public static ILog GetLogger(string name)
        {
            if (_logManager == null)
                throw new ApplicationException("LogManager is not yet initialized, please call LogManager.Initialize()");

            return _logManager.GetLog(name);
        }

        void IInternalLogManager.Enqueue(IInternalLogEvent logEvent)
            => _queue.Enqueue(logEvent);

        ILog IInternalLogManager.GetLog(string name)
            => _loggers.GetOrAdd(name, n => new Log(this, n));

        LogConfig IInternalLogManager.ResolveLogConfig(string name)
            => _configResolver.ResolveLogConfig(name);

        IInternalLogEvent IInternalLogManager.AcquireLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, IInternalLogEvent notifyPoolExhaustionLogEvent, Level level, Log log)
        {
            IInternalLogEvent Initialize(IInternalLogEvent l)
            {
                l.Initialize(level, log);
                return l;
            }

            if (_pool.TryAcquire(out var logEvent))
                return Initialize(logEvent);

            switch (logEventPoolExhaustionStrategy)
            {
                case LogEventPoolExhaustionStrategy.WaitForLogEvent:
                    return Initialize(AcquireLogEventWait());

                case LogEventPoolExhaustionStrategy.DropLogMessage:
                    return NoopLogEvent.Instance;

                default:
                    return notifyPoolExhaustionLogEvent;
            }
        }

        private IInternalLogEvent AcquireLogEventWait()
        {
            var spinWait = new SpinWait();

            IInternalLogEvent logEvent;
            while (!_pool.TryAcquire(out logEvent))
            {
                spinWait.SpinOnce();
            }

            return logEvent;
        }

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
            var stringBuffer = new StringBuffer(16 * 1024);
            var destination = new byte[16 * 1024];
            var flush = false;

            while (_isRunning || !_queue.IsEmpty)
            {
                if (TryToProcessQueue(stringBuffer, destination))
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

        private bool TryToProcessQueue(StringBuffer stringBuffer, byte[] destination)
        {
            if (!_queue.TryDequeue(out var logEvent))
                return false;

            try
            {
                if (!logEvent.IsPooled)
                    logEvent.SetTimestamp(SystemDateTime.UtcNow);

                if ((logEvent.Appenders?.Length ?? 0) <= 0)
                    return true;

                try
                {
                    FormatLogMessage(stringBuffer, logEvent);
                }
                catch (Exception)
                {
                    FormatErrorMessage(stringBuffer, logEvent);
                }

                var bytesWritten = CopyStringBufferToByteArray(stringBuffer, destination);

                WriteMessageLogToAppenders(destination, logEvent, bytesWritten);
            }
            finally
            {
                if (logEvent.IsPooled)
                    _pool.Release(logEvent);
            }

            return true;
        }

        private static void FormatErrorMessage(StringBuffer stringBuffer, IInternalLogEvent logEvent)
        {
            stringBuffer.Clear();
            stringBuffer.Append("An error occured during formatting: ");

            logEvent.WriteToStringBufferUnformatted(stringBuffer);
        }

        private static void WriteMessageLogToAppenders(byte[] destination, IInternalLogEvent logEvent, int bytesWritten)
        {
            foreach (var appender in logEvent.Appenders)
            {
                // if (logEvent.Level >= Level) // TODO Check this ? log event should not be in queue if not > Level
                appender.WriteEvent(logEvent, destination, bytesWritten);
            }
        }

        private static void FormatLogMessage(StringBuffer stringBuffer, IInternalLogEvent logEvent)
        {
            stringBuffer.Clear();
            logEvent.WriteToStringBuffer(stringBuffer);
        }

        private unsafe int CopyStringBufferToByteArray(StringBuffer stringBuffer, byte[] destination)
        {
            fixed (byte* dest = &destination[0])
            {
                return stringBuffer.CopyTo(dest, destination.Length, 0, stringBuffer.Count, _encoding);
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

        BufferSegment IInternalLogManager.GetBufferSegment() => _bufferSegmentProvider.GetSegment();
    }
}
