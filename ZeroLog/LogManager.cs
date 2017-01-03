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
        private bool _isRunning;

        private LogManager()
        {
            _encoding = Encoding.Default;
            _queue = new ConcurrentQueue<LogEvent>();
            _pool = new ObjectPool<LogEvent>(() => new LogEvent(), 1024);
            _appenders = new List<IAppender>();

            Task.Run(() => WriteToAppenders());
        }

        public static LogManager Initialize(IEnumerable<IAppender> appenders)
        {
            if (_logManager != null)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager();
            _logManager._appenders.AddRange(appenders);
            return _logManager;
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = null;

            logManager._isRunning = false;
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
            while (_isRunning)
            {
                try
                {
                    TryToProcessQueue(stringBuffer, destination);
                }
                catch (Exception ex)
                {
                    // TODO: how can we distinguish between exceptions that occur during shutdown and normal operation
                    Console.WriteLine(ex);
                }
            }
        }

        private unsafe void TryToProcessQueue(StringBuffer stringBuffer, byte[] destination)
        {
            LogEvent logEvent;
            if (_queue.TryDequeue(out logEvent))
            {
                logEvent.WriteToStringBuffer(stringBuffer);
                int bytesWritten;
                fixed (byte* dest = destination)
                    bytesWritten = stringBuffer.CopyTo(dest, 0, stringBuffer.Count, _encoding);

                _pool.Free(logEvent);

                // Write to appenders
                foreach (var appender in _appenders)
                {
                    var stream = appender.GetStream();
                    stream.Write(destination, 0, bytesWritten);
                }
            }
        }
    }
}