using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using Roslyn.Utilities;

namespace ZeroLog
{
    public class LogManager
    {
        private readonly Level _level;
        private static LogManager _logManager;

        private readonly ConcurrentQueue<LogEvent> _queue;
        private readonly ObjectPool<LogEvent> _pool;
        private readonly Encoding _encoding;
        private readonly List<IAppender> _appenders;
        private readonly Task _writeTask;
        private bool _isRunning = true;
        
        internal LogManager(IEnumerable<IAppender> appenders, int size, Level level = Level.Finest)
        {
            _level = level;
            _encoding = Encoding.Default;
            _queue = new ConcurrentQueue<LogEvent>(new FakeCollection(size));
            var bufferSegmentProvider = new BufferSegmentProvider(size * 128, 128);
            _pool = new ObjectPool<LogEvent>(() => new LogEvent(bufferSegmentProvider.GetSegment()), size);

            foreach (var appender in appenders)
            {
                appender.SetEncoding(_encoding);
            }

            _appenders = new List<IAppender>(appenders);

            _writeTask = Task.Run(() => WriteToAppenders());
        }

        public static LogManager Initialize(IEnumerable<IAppender> appenders, int size = 1024, Level level = Level.Finest)
        {
            if (_logManager != null)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(appenders, size, level);
            return _logManager;
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = null;

            if (logManager == null)
                return;

            logManager._isRunning = false;
            // TODO: shutdown all the logs

            logManager._writeTask.Wait(15000);
        }

        public Level Level => _level;

        public static Log GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public static Log GetLogger(string name)
        {
            if (_logManager == null)
                throw new ApplicationException("LogManager is not yet initialized, please call LogManager.Initialize()");

            var log = new Log(_logManager, name);
            return log;
        }

        internal void Enqueue(LogEvent logEvent)
        {
            _queue.Enqueue(logEvent);
        }

        internal LogEvent AllocateLogEvent()
        {
            return _pool.Allocate();
        }

        private void WriteToAppenders()
        {
            var stringBuffer = new StringBuffer();
            var destination = new byte[1024];
            while (_isRunning || !_queue.IsEmpty)
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
                foreach (var appender in _appenders)
                {
                    // TODO: each appender should declare their own level
                    if (logEvent.Level <= _level)
                        appender.WriteEvent(logEvent, destination, bytesWritten);
                }

                _pool.Free(logEvent);
            }
        }
    }
}
