using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog
{
    public sealed class LogManager : IInternalLogManager
    {
        internal const int OutputBufferSize = 16 * 1024;

        internal static readonly Encoding DefaultEncoding = Encoding.UTF8;
        private static readonly Encoding _encoding = DefaultEncoding;

        private static LogManager? _logManager;

        private readonly ConcurrentDictionary<string, Log> _loggers = new();
        private readonly ConcurrentQueue<LogMessage> _queue;
        private readonly ObjectPool<LogMessage> _pool;

        private readonly BufferSegmentProvider _bufferSegmentProvider;
        private readonly IConfigurationResolver _configResolver;
        private readonly Thread _writeThread;

        private bool _isRunning;
        private IAppender[] _appenders = Array.Empty<IAppender>();

        public Level Level { get; private set; }
        public static ZeroLogConfig Config { get; } = new();

        internal LogManager(IConfigurationResolver configResolver, ZeroLogInitializationConfig config)
        {
            config.Validate();

            _configResolver = configResolver;

            _queue = new ConcurrentQueue<LogMessage>(new ConcurrentQueueCapacityInitializer(config.LogEventQueueSize));

            _bufferSegmentProvider = new BufferSegmentProvider(config.LogEventQueueSize * config.LogEventBufferSize, config.LogEventBufferSize);
            _pool = new ObjectPool<LogMessage>(config.LogEventQueueSize, () => new LogMessage(_bufferSegmentProvider.GetSegment(), config.LogEventArgumentCapacity));

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

        public static Log GetLogger<T>()
            => GetLogger(typeof(T));

        public static Log GetLogger(Type type)
            => GetLogger(type.FullName!);

        public static Log GetLogger(string name)
            => _logManager != null
                ? _logManager.GetLog(name)
                : Log.CreateEmpty(name);

        private Log GetLog(string name)
            => _loggers.GetOrAdd(name, static (n, mgr) => new Log(mgr, n), this);

        void IInternalLogManager.Enqueue(LogMessage message)
            => _queue.Enqueue(message);

        LogConfig IInternalLogManager.ResolveLogConfig(string name)
            => _configResolver.ResolveLogConfig(name);

        LogMessage? IInternalLogManager.AcquireLogMessage(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy)
        {
            if (_pool.TryAcquire(out var logMessage))
                return logMessage;

            if (!_isRunning)
                return LogMessage.Empty;

            switch (logEventPoolExhaustionStrategy)
            {
                case LogEventPoolExhaustionStrategy.WaitForLogEvent:
                {
                    var spinWait = new SpinWait();

                    while (!_pool.TryAcquire(out logMessage))
                    {
                        spinWait.SpinOnce();

                        if (!_isRunning)
                            return LogMessage.Empty;
                    }

                    return logMessage;
                }

                case LogEventPoolExhaustionStrategy.DropLogMessage:
                    return LogMessage.Empty;

                default:
                    return null;
            }
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
            var charBuffer = GC.AllocateUninitializedArray<char>(OutputBufferSize);
            var destination = GC.AllocateUninitializedArray<byte>(OutputBufferSize);
            var keyValuePointerBuffer = new KeyValuePointerBuffer();
            var flush = false;

            while (_isRunning || !_queue.IsEmpty)
            {
                if (TryToProcessQueue(stringBuffer, charBuffer, destination, keyValuePointerBuffer))
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

        private bool TryToProcessQueue(StringBuffer stringBuffer, char[] charBuffer, byte[] destination, KeyValuePointerBuffer keyValuePointerBuffer)
        {
            if (!_queue.TryDequeue(out var logMessage))
                return false;

            try
            {
                if ((logMessage.Logger?.Appenders.Length ?? 0) <= 0)
                    return true;

                int bytesWritten;

                try
                {
                    // TODO : Rewrite this without StringBuffer

                    stringBuffer.Clear();
                    var messageLength = logMessage.WriteTo(charBuffer);
                    stringBuffer.Append(charBuffer.AsSpan(0, messageLength));
                    //logMessage.WriteToStringBuffer(stringBuffer, keyValuePointerBuffer);
                    bytesWritten = CopyStringBufferToByteArray(stringBuffer, destination);
                }
                catch (Exception ex)
                {
                    HandleFormattingError(stringBuffer, destination, ex, out bytesWritten);
                }

                WriteMessageLogToAppenders(destination, logMessage, bytesWritten);
            }
            finally
            {
                if (logMessage.IsPooled && _isRunning)
                    _pool.Release(logMessage);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void HandleFormattingError(StringBuffer stringBuffer, byte[] destination, Exception exception, out int bytesWritten)
        {
            stringBuffer.Clear();
            stringBuffer.Append("An error occured during formatting: ");
            stringBuffer.Append(exception.Message);
            bytesWritten = CopyStringBufferToByteArray(stringBuffer, destination);
        }

        private static void WriteMessageLogToAppenders(byte[] destination, LogMessage logMessage, int bytesWritten)
        {
            foreach (var appender in logMessage.Logger?.Appenders ?? Array.Empty<IAppender>())
            {
                // if (logEvent.Level >= Level) // TODO Check this ? log event should not be in queue if not > Level
                appender.WriteMessage(logMessage, destination, bytesWritten);
            }
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

        internal ConcurrentQueue<LogMessage> GetInternalQueue()
            => _queue; // Used by unit tests
    }
}
