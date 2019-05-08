using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        internal const int OutputBufferSize = 16 * 1024;

        private static readonly IInternalLogManager _noOpLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _noOpLogManager;
        private static readonly Encoding _encoding = Encoding.UTF8;

        private readonly ConcurrentDictionary<string, Log> _loggers = new ConcurrentDictionary<string, Log>();
        private readonly ConcurrentQueue<IInternalLogEvent> _queue;
        private readonly ObjectPool<IInternalLogEvent> _pool;

        private readonly BufferSegmentProvider _bufferSegmentProvider;
        private readonly IConfigurationResolver _configResolver;
        private readonly Thread _writeThread;

        private bool _isRunning;
        private IAppender[] _appenders = ArrayUtil.Empty<IAppender>();

        public static ZeroLogConfig Config { get; } = new ZeroLogConfig();

        internal LogManager(IConfigurationResolver configResolver, ZeroLogInitializationConfig config)
        {
            config.Validate();

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

        public static void RegisterUnmanaged<T>()
            where T : unmanaged, IStringFormattable
            => UnmanagedCache.Register<T>();

        public static void RegisterUnmanaged<T>(UnmanagedFormatterDelegate<T> formatter)
            where T : unmanaged
            => UnmanagedCache.Register<T>(formatter);

        public void Dispose()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            _pool.Clear();
            _writeThread.Join();
            _pool.Clear();

            if (_pool.IsAnyItemAcquired())
                Thread.Sleep(100); // Can't really do much better here

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

        IInternalLogEvent IInternalLogManager.AcquireLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy)
        {
            if (_pool.TryAcquire(out var logEvent))
                return logEvent;

            if (!_isRunning)
                return NoopLogEvent.Instance;

            switch (logEventPoolExhaustionStrategy)
            {
                case LogEventPoolExhaustionStrategy.WaitForLogEvent:
                    return AcquireLogEventWait();

                case LogEventPoolExhaustionStrategy.DropLogMessage:
                    return NoopLogEvent.Instance;

                default:
                    return null;
            }
        }

        private IInternalLogEvent AcquireLogEventWait()
        {
            var spinWait = new SpinWait();

            IInternalLogEvent logEvent;
            while (!_pool.TryAcquire(out logEvent))
            {
                spinWait.SpinOnce();

                if (!_isRunning)
                    return NoopLogEvent.Instance;
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
            var stringBuffer = new StringBuffer(OutputBufferSize);
            var destination = new byte[OutputBufferSize];
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

                int bytesWritten;

                try
                {
                    FormatLogMessage(stringBuffer, logEvent);
                    bytesWritten = CopyStringBufferToByteArray(stringBuffer, destination);
                }
                catch
                {
                    HandleFormattingError(stringBuffer, logEvent, destination, out bytesWritten);
                }

                WriteMessageLogToAppenders(destination, logEvent, bytesWritten);
            }
            finally
            {
                if (logEvent.IsPooled && _isRunning)
                    _pool.Release(logEvent);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void HandleFormattingError(StringBuffer stringBuffer, IInternalLogEvent logEvent, byte[] destination, out int bytesWritten)
        {
            try
            {
                stringBuffer.Clear();
                stringBuffer.Append("An error occured during formatting: ");

                logEvent.WriteToStringBufferUnformatted(stringBuffer);
                bytesWritten = CopyStringBufferToByteArray(stringBuffer, destination);
            }
            catch (Exception ex)
            {
                stringBuffer.Clear();
                stringBuffer.Append("An error occured during formatting: ");
                stringBuffer.Append(ex.Message);
                bytesWritten = CopyStringBufferToByteArray(stringBuffer, destination);
            }
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

        private static unsafe int CopyStringBufferToByteArray(StringBuffer stringBuffer, byte[] destination)
        {
            fixed (byte* dest = &destination[0])
            {
                // This works only for ASCII strings, but doing the real check would be expensive, and it's an edge case...
                if (stringBuffer.Count > destination.Length)
                    TruncateMessage(stringBuffer, destination.Length);

                return stringBuffer.CopyTo(dest, destination.Length, 0, stringBuffer.Count, _encoding);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TruncateMessage(StringBuffer stringBuffer, int maxLength)
        {
            var suffix = Config.TruncatedMessageSuffix;
            var maxMessageLength = maxLength - suffix.Length;

            if (maxMessageLength > 0)
            {
                stringBuffer.Count = maxMessageLength;
                stringBuffer.Append(suffix);
            }
            else
            {
                stringBuffer.Count = maxLength;
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

        internal ConcurrentQueue<IInternalLogEvent> GetInternalQueue() => _queue; // Used by unit tests
    }
}
