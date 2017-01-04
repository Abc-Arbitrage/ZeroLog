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
        private static LogManager _logManager;

        private readonly ConcurrentQueue<LogEvent> _queue;
        private readonly ObjectPool<LogEvent> _pool;
        private readonly Encoding _encoding;
        private readonly List<IAppender> _appenders;
        private readonly Task _writeTask;
        private bool _isRunning = true;
        private readonly byte[] _newlineBytes;

        private LogManager(IEnumerable<IAppender> appenders, Level level = Level.Finest)
        {
            _encoding = Encoding.Default;
            _queue = new ConcurrentQueue<LogEvent>();
            _pool = new ObjectPool<LogEvent>(() => new LogEvent(level), 1024);

            foreach (var appender in appenders)
            {
                appender.SetEncoding(_encoding);
            }

            _appenders = new List<IAppender>(appenders);

            _writeTask = Task.Run(() => WriteToAppenders());
        }

        public static LogManager Initialize(IEnumerable<IAppender> appenders)
        {
            if (_logManager != null)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(appenders);
            return _logManager;
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = null;

            logManager._isRunning = false;
            // TODO: shutdown all the logs

            logManager._writeTask.Wait(1000);
        }

        public static Log GetLogger(Type type)
        {
            if (_logManager == null)
                throw new ApplicationException("LogManager is not yet initialized, please call LogManager.Initialize()");

            var log = new Log(_logManager, type.FullName);
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
            byte[] destination = new byte[1024];
            while ( _isRunning)
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
                    bytesWritten = stringBuffer.CopyTo(dest, 0, stringBuffer.Count, _encoding);

                // Write to appenders
                foreach (var appender in _appenders)
                {
                    appender.WriteEvent(logEvent, destination, bytesWritten);
                }

                _pool.Free(logEvent);
            }
        }
    }
}