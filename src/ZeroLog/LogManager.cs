using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using System.Threading.Tasks;
using ExtraConstraints;
using JetBrains.Annotations;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;
using ZeroLog.Utils;

namespace ZeroLog
{
    public class LogManager : IInternalLogManager
    {
        private static readonly IInternalLogManager _defaultLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _defaultLogManager;

        private readonly ConcurrentQueue<Log> _loggers;
        private readonly ConcurrentQueue<IInternalLogEvent> _queue;
        private readonly ObjectPool<IInternalLogEvent> _pool;

        private readonly BufferSegmentProvider _bufferSegmentProvider;
        private readonly IConfigurationResolver _configResolver;
        private readonly Task _writeTask;

        private bool _isRunning;
        private readonly Encoding _encoding = Encoding.UTF8;

        internal LogManager(IConfigurationResolver configResolver, int logEventQueueSize = 1024, int logEventBufferSize = 128)
        {
            _configResolver = configResolver;

            _loggers = new ConcurrentQueue<Log>();
            _queue = new ConcurrentQueue<IInternalLogEvent>(new ConcurrentQueueCapacityInitializer(logEventQueueSize));

            _bufferSegmentProvider = new BufferSegmentProvider(logEventQueueSize * logEventBufferSize, logEventBufferSize);
            _pool = new ObjectPool<IInternalLogEvent>(logEventQueueSize, () => new LogEvent(_bufferSegmentProvider.GetSegment()));

            configResolver.Initialize(_encoding);
            configResolver.Updated += () =>
            {
                foreach (var logger in _loggers)
                {
                    logger.ResetConfiguration();
                }
            };

            _isRunning = true;
            _writeTask = Task.Factory.StartNew(WriteToAppenders, TaskCreationOptions.LongRunning);
        }

        public Level Level => _configResolver.ResolveLevel("");

        public static ILogManager Initialize(IConfigurationResolver configResolver, int logEventQueueSize = 1024, int logEventBufferSize = 128)
        {
            if (_logManager != _defaultLogManager)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(configResolver, logEventQueueSize, logEventBufferSize);
            return _logManager;
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = _defaultLogManager;

            logManager?.Dispose();
        }

        public static void RegisterEnum([NotNull] Type enumType)
            => EnumCache.Register(enumType);

        public static void RegisterEnum<[EnumConstraint] T>()
            => EnumCache.Register(typeof(T));

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
            _writeTask.Wait(15000);

            _configResolver.Dispose();
            _bufferSegmentProvider.Dispose();
        }

        public static ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public static ILog GetLogger(string name)
        {
            if (_logManager == null)
                throw new ApplicationException("LogManager is not yet initialized, please call LogManager.Initialize()");

            var log = _logManager.GetNewLog(_logManager, name);
            return log;
        }

        void IInternalLogManager.Enqueue(IInternalLogEvent logEvent)
        {
            _queue.Enqueue(logEvent);
        }

        ILog IInternalLogManager.GetNewLog(IInternalLogManager logManager, string name)
        {
            var logger = new Log(logManager, name);
            _loggers.Enqueue(logger);
            return logger;
        }

        IList<IAppender> IInternalLogManager.ResolveAppenders(string name)
            => _configResolver.ResolveAppenders(name);

        LogEventPoolExhaustionStrategy IInternalLogManager.ResolveLogEventPoolExhaustionStrategy(string name)
            => _configResolver.ResolveExhaustionStrategy(name);

        Level IInternalLogManager.ResolveLevel(string name)
            => _configResolver.ResolveLevel(name);

        IInternalLogEvent IInternalLogManager.AllocateLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, IInternalLogEvent notifyPoolExhaustionLogEvent, Level level, Log log)
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
                    return Initialize(AcquireLogEvent());

                case LogEventPoolExhaustionStrategy.DropLogMessage:
                    return NoopLogEvent.Instance;

                default:
                    return notifyPoolExhaustionLogEvent;
            }
        }

        private IInternalLogEvent AcquireLogEvent()
        {
            var spinwait = new SpinWait();

            IInternalLogEvent logEvent;
            while (!_pool.TryAcquire(out logEvent))
            {
                spinwait.SpinOnce();
            }

            return logEvent;
        }

        private void WriteToAppenders()
        {
            var spinWait = new SpinWait();
            var stringBuffer = new StringBuffer(16 * 1024);
            var destination = new byte[16 * 1024];

            while (_isRunning || !_queue.IsEmpty)
            {
                if (TryToProcessQueue(stringBuffer, destination))
                    spinWait.Reset();
                else
                    spinWait.SpinOnce();
            }
        }

        private bool TryToProcessQueue(StringBuffer stringBuffer, byte[] destination)
        {
            if (!_queue.TryDequeue(out var logEvent))
                return false;

            try
            {
                if (!logEvent.IsPooled)
                    logEvent.SetTimestamp(SystemDateTime.UtcNow);

                if ((logEvent.Appenders?.Count ?? 0) <= 0)
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

        private void WriteMessageLogToAppenders(byte[] destination, IInternalLogEvent logEvent, int bytesWritten)
        {
            var appenders = logEvent.Appenders;
            for (var i = 0; i < appenders.Count; i++)
            {
                var appender = appenders[i];
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
            int bytesWritten;
            fixed (byte* dest = destination)
            {
                bytesWritten = stringBuffer.CopyTo(dest, destination.Length, 0, stringBuffer.Count, _encoding);
            }

            return bytesWritten;
        }

        BufferSegment IInternalLogManager.GetBufferSegment() => _bufferSegmentProvider.GetSegment();
    }
}
