using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using System.Threading.Tasks;
using ZeroLog.Appenders;
using ZeroLog.Appenders.Builders;
using ZeroLog.Config;
using ZeroLog.ConfigResolvers;

namespace ZeroLog
{
    public class LogManager : IInternalLogManager
    {
        private static readonly IInternalLogManager _defaultLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _defaultLogManager;

        private readonly ConcurrentBag<Log> _loggers;
        private readonly ConcurrentQueue<IInternalLogEvent> _queue;
        private readonly ObjectPool<IInternalLogEvent> _pool;
        private readonly Encoding _encoding;

        private readonly LogEventPoolExhaustionStrategy _logEventPoolExhaustionStrategy;

        private readonly BufferSegmentProvider _bufferSegmentProvider;

        private readonly IConfigurationResolver _configResolver;

        public bool IsRunning { get; set; }
        public Task WriteTask { get; }
        //public List<IAppender> Appenders { get; }

        internal LogManager(IConfigurationResolver configResolver, LogManagerConfiguration configuration)
        {
            _configResolver = configResolver;

            _encoding = Encoding.Default;
            _logEventPoolExhaustionStrategy = configuration.LogEventPoolExhaustionStrategy;

            _loggers = new ConcurrentBag<Log>();
            _queue = new ConcurrentQueue<IInternalLogEvent>(new ConcurrentQueueCapacityInitializer(configuration.LogEventQueueSize));

            _bufferSegmentProvider = new BufferSegmentProvider(configuration.LogEventQueueSize * configuration.LogEventBufferSize, configuration.LogEventBufferSize);
            _pool = new ObjectPool<IInternalLogEvent>(configuration.LogEventQueueSize, () => new LogEvent(_bufferSegmentProvider.GetSegment()));

            configResolver.Initialize(_encoding);
            configResolver.Updated += () =>
            {
                foreach (var logger in _loggers)
                    logger.ResetConfiguration();
            };

            IsRunning = true;
            WriteTask = Task.Factory.StartNew(WriteToAppenders, TaskCreationOptions.LongRunning);
        }

        public Level Level => _configResolver.ResolveLevel("");

        public static ILogManager ConfigureAndWatch(string filepath)
        {
            return Configurator.ConfigureAndWatch(filepath);
        }

        public static ILogManager Initialize(IConfigurationResolver configResolver, LogManagerConfiguration configuration)
        {
            if (_logManager != _defaultLogManager)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(configResolver, configuration);
            return _logManager;
        }

        public static ILogManager Initialize(IEnumerable<IAppender> appenders, LogManagerConfiguration configuration)
        {
            if (_logManager != _defaultLogManager)
                throw new ApplicationException("LogManager is already initialized");

            var dummyResolver = new DummyResolver(appenders, configuration.Level, configuration.LogEventPoolExhaustionStrategy);
            _logManager = new LogManager(dummyResolver, configuration);
            return _logManager;
        }

        public static ILogManager Initialize(IEnumerable<IAppender> appenders, int logEventQueueSize = 1024, int logEventBufferSize = 128, Level level = Level.Finest)
        {
            return Initialize(appenders, new LogManagerConfiguration
            {
                LogEventQueueSize = logEventQueueSize,
                LogEventBufferSize = logEventBufferSize,
                Level = level,
            });
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = _defaultLogManager;

            logManager?.Dispose();
        }

        public void Dispose()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            WriteTask.Wait(15000);

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
            _loggers.Add(logger);
            return logger;
        }

        public IList<IAppender> ResolveAppenders(string name)
            => _configResolver.ResolveAppenders(name);

        public LogEventPoolExhaustionStrategy ResolveLogEventPoolExhaustionStrategy(string name)
            => _configResolver.ResolveExhaustionStrategy(name);

        public Level ResolveLevel(string name)
            => _configResolver.ResolveLevel(name);

        IInternalLogEvent IInternalLogManager.AllocateLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, IInternalLogEvent notifyPoolExhaustionLogEvent)
        {
            if (_pool.TryAcquire(out var logEvent))
                return logEvent;

            switch (logEventPoolExhaustionStrategy)
            {
                case LogEventPoolExhaustionStrategy.WaitForLogEvent:
                    return AcquireLogEvent();

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

            while (IsRunning || !_queue.IsEmpty)
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
    }
}
