using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using Roslyn.Utilities;

namespace ZeroLog
{
    public class LogManager : IInternalLogManager
    {
        private static readonly IInternalLogManager _defaultLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _defaultLogManager;

        private readonly ConcurrentQueue<LogEvent> _queue;
        private readonly ObjectPool<LogEvent> _pool;
        private readonly Encoding _encoding;
        public bool IsRunning { get; set; }
        public Task WriteTask { get; }
        public List<IAppender> Appenders { get; }

        internal LogManager(IEnumerable<IAppender> appenders, int size, int logEventBufferSize = 128, Level level = Level.Finest)
        {
            Level = level;
            _encoding = Encoding.Default;
            _queue = new ConcurrentQueue<LogEvent>(new FakeCollection(size));
            var bufferSegmentProvider = new BufferSegmentProvider(size * logEventBufferSize, logEventBufferSize);
            _pool = new ObjectPool<LogEvent>(() => new LogEvent(bufferSegmentProvider.GetSegment()), size);

            foreach (var appender in appenders)
            {
                appender.SetEncoding(_encoding);
            }

            Appenders = new List<IAppender>(appenders);

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

        void IInternalLogManager.Enqueue(LogEvent logEvent)
        {
            _queue.Enqueue(logEvent);
        }

        ILog IInternalLogManager.GetNewLog(IInternalLogManager logManager, string name)
        {
            return new Log(logManager, name);
        }

        LogEvent IInternalLogManager.AllocateLogEvent()
        {
            return _pool.Allocate();
        }

        private void WriteToAppenders()
        {
            var stringBuffer = new StringBuffer(1024);
            var destination = new byte[1024];
            while (IsRunning || !_queue.IsEmpty)
            {
                try
                {
                    TryToProcessQueue(stringBuffer, destination);
                }
                catch (Exception ex)
                {
                    // TODO: how can we distinguish between exceptions that occur during shutdown and normal operation
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }

        private unsafe void TryToProcessQueue(StringBuffer stringBuffer, byte[] destination)
        {
            LogEvent logEvent;
            if (_queue.TryDequeue(out logEvent))
            {
                // Write format only once
                logEvent.WriteToStringBuffer(stringBuffer);
                int bytesWritten;
                fixed (byte* dest = destination)
                    bytesWritten = stringBuffer.CopyTo(dest, destination.Length, 0, stringBuffer.Count, _encoding);

                stringBuffer.Clear();

                // Write to appenders
                foreach (var appender in Appenders)
                {
                    // TODO: each appender should declare their own level
                    if (logEvent.Level >= Level)
                        appender.WriteEvent(logEvent, destination, bytesWritten);
                }

                _pool.Free(logEvent);
            }
        }
    }
}
