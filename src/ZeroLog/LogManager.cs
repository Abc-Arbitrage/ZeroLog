using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroLog
{
    public class LogManager : IInternalLogManager
    {
        private static readonly IInternalLogManager _defaultLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _defaultLogManager;

        private readonly ConcurrentQueue<IInternalLogEvent> _queue;
        private readonly ObjectPool<IInternalLogEvent> _pool;
        private readonly Encoding _encoding;
        private readonly IInternalLogEvent _noopLogEvent = new NoopLogEvent();

        public bool IsRunning { get; set; }
        public Task WriteTask { get; }
        public List<IAppender> Appenders { get; }

        internal LogManager(IEnumerable<IAppender> appenders, int size, int logEventBufferSize = 128, Level level = Level.Finest)
        {
            Level = level;
            _encoding = Encoding.Default;
            _queue = new ConcurrentQueue<IInternalLogEvent>(new FakeCollection(size));

            var bufferSegmentProvider = new BufferSegmentProvider(size * logEventBufferSize, logEventBufferSize);
            _pool = new ObjectPool<IInternalLogEvent>(size, () => new LogEvent(bufferSegmentProvider.GetSegment()));

            Appenders = new List<IAppender>(appenders);

            foreach (var appender in Appenders)
            {
                appender.SetEncoding(_encoding);
            }

            IsRunning = true;
            WriteTask = Task.Run(() => WriteToAppenders());
        }

        public Level Level { get; }

        public static ILogManager Initialize(IEnumerable<IAppender> appenders, int size = 1024, int logEventBufferSize = 128, Level level = Level.Finest)
        {
            if (_logManager != _defaultLogManager)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(appenders, size, logEventBufferSize, level);
            return _logManager;
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
            IInternalLogEvent logEvent;
            if (!_pool.TryAcquire(out logEvent))
                return _noopLogEvent;

            return logEvent;
        }

        private void WriteToAppenders()
        {
            var spinWait = default(SpinWait);
            var stringBuffer = new StringBuffer(1024);
            var destination = new byte[1024];
            while (IsRunning || !_queue.IsEmpty)
            {
                try
                {
                    if (!TryToProcessQueue(stringBuffer, destination))
                        spinWait.SpinOnce();
                }
                catch (Exception ex)
                {
                    // TODO: how can we distinguish between exceptions that occur during shutdown and normal operation
                    Console.WriteLine(ex);
                    throw;
                }
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

        private void FormatErrorMessage(StringBuffer stringBuffer, IInternalLogEvent logEvent)
        {
            stringBuffer.Clear();
            stringBuffer.Append("An error occured during formatting: ");

            logEvent.WriteToStringBufferUnformatted(stringBuffer);
        }

        private void WriteMessageLogToAppenders(byte[] destination, IInternalLogEvent logEvent, int bytesWritten)
        {
            foreach (var appender in Appenders)
            {
                // TODO: each appender should declare their own level
                if (logEvent.Level >= Level)
                    appender.WriteEvent(logEvent, destination, bytesWritten);
            }
        }

        private void FormatLogMessage(StringBuffer stringBuffer, IInternalLogEvent logEvent)
        {
            stringBuffer.Clear();
            logEvent.WriteToStringBuffer(stringBuffer);
        }

        private unsafe int CopyStringBufferToByteArray(StringBuffer stringBuffer, byte[] destination)
        {
            int bytesWritten;
            fixed (byte* dest = destination)
                bytesWritten = stringBuffer.CopyTo(dest, destination.Length, 0, stringBuffer.Count, _encoding);

            return bytesWritten;
        }
    }
}
