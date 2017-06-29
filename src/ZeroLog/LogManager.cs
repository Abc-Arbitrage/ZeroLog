using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using System.Threading.Tasks;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public class LogManager : IInternalLogManager
    {
        private static readonly IInternalLogManager _defaultLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _defaultLogManager;

        private readonly ConcurrentQueue<IInternalLogEvent> _queue;
        private readonly ObjectPool<IInternalLogEvent> _pool;
        private readonly Encoding _encoding;

        private readonly IInternalLogEvent _notifyPoolExhaustionLogEvent = new ForwardingLogEvent(SpecialLogEvents.ExhaustedPoolEvent);

        private readonly LogEventPoolExhaustionStrategy _logEventPoolExhaustionStrategy;

        public bool IsRunning { get; set; }
        public Task WriteTask { get; }
        public List<IAppender> Appenders { get; }

        internal LogManager(IEnumerable<IAppender> appenders, LogManagerConfiguration configuration)
        {
            Level = configuration.Level;

            _encoding = Encoding.Default;
            _logEventPoolExhaustionStrategy = configuration.LogEventPoolExhaustionStrategy;

            _queue = new ConcurrentQueue<IInternalLogEvent>(new FakeCollection(configuration.LogEventQueueSize));

            var bufferSegmentProvider = new BufferSegmentProvider(configuration.LogEventQueueSize * configuration.LogEventBufferSize, configuration.LogEventBufferSize);
            _pool = new ObjectPool<IInternalLogEvent>(configuration.LogEventQueueSize, () => new LogEvent(bufferSegmentProvider.GetSegment()));

            Appenders = new List<IAppender>(appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));

            foreach (var appender in Appenders)
            {
                appender.SetEncoding(_encoding);
            }

            IsRunning = true;
            WriteTask = Task.Run(() => WriteToAppenders());
        }

        public Level Level { get; }

        public static ILogManager Initialize(IEnumerable<IAppender> appenders, LogManagerConfiguration configuration)
        {
            if (_logManager != _defaultLogManager)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(appenders, configuration);
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

            if (logManager == null)
                return;

            logManager.IsRunning = false;
            logManager.WriteTask.Wait(15000);

            foreach (var appender in logManager.Appenders)
            {
                appender.Close();
            }
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
            return new Log(logManager, name);
        }

        IInternalLogEvent IInternalLogManager.AllocateLogEvent()
        {
            if (_pool.TryAcquire(out var logEvent))
                return logEvent;

            switch (_logEventPoolExhaustionStrategy)
            {
                case LogEventPoolExhaustionStrategy.WaitForLogEvent:
                    return AcquireLogEvent();

                case LogEventPoolExhaustionStrategy.DropLogMessage:
                    return NoopLogEvent.Instance;

                default:
                    return _notifyPoolExhaustionLogEvent;
            }
        }

        private IInternalLogEvent AcquireLogEvent()
        {
            IInternalLogEvent logEvent;
            var spinwait = new SpinWait();
            while (!_pool.TryAcquire(out logEvent))
            {
                spinwait.SpinOnce();
            }

            return logEvent;
        }

        private void WriteToAppenders()
        {
            var spinWait = default(SpinWait);
            var stringBuffer = new StringBuffer(16 * 1024);
            var destination = new byte[16 * 1024];
            while (IsRunning || !_queue.IsEmpty)
            {
                if (!TryToProcessQueue(stringBuffer, destination))
                    spinWait.SpinOnce();
            }
        }

        private bool TryToProcessQueue(StringBuffer stringBuffer, byte[] destination)
        {
            IInternalLogEvent logEvent;
            if (!_queue.TryDequeue(out logEvent))
                return false;

            var isSpecialEvent = false;
            if (logEvent == SpecialLogEvents.ExhaustedPoolEvent)
            {
                isSpecialEvent = true;
                logEvent.SetTimestamp(DateTime.UtcNow);
            }

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

            if (!isSpecialEvent)
                _pool.Release(logEvent);

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
            for (var i = 0; i < Appenders.Count; i++)
            {
                var appender = Appenders[i];
                if (logEvent.Level >= Level)
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
