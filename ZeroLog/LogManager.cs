using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace ZeroLog
{
    public class LogManager
    {
        private static LogManager _logManager;

        private readonly Disruptor<LogEvent> _disruptor;
        private readonly Encoding _encoding;
        private readonly List<IAppender> _appenders;
        private bool _isRunning = true;

        private LogManager(IEnumerable<IAppender> appenders, int size, Level level = Level.Finest)
        {
            _encoding = Encoding.Default;
            var bufferSegmentProvider = new BufferSegmentProvider(size * 128, 128);
            _disruptor = new Disruptor<LogEvent>(() => new LogEvent(level, bufferSegmentProvider.GetSegment()), size, TaskScheduler.Default);

            foreach (var appender in appenders)
            {
                appender.SetEncoding(_encoding);
            }

            _appenders = new List<IAppender>(appenders);

            _disruptor.HandleEventsWith(new LogEventHandler(_appenders, _encoding));
            _disruptor.Start();
        }

        public static LogManager Initialize(IEnumerable<IAppender> appenders, int size = 1024)
        {
            if (_logManager != null)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(appenders, size);
            return _logManager;
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = null;

            if (logManager == null)
                return;

            logManager._disruptor.Shutdown(TimeSpan.FromSeconds(15));
            logManager._isRunning = false;
            // TODO: shutdown all the logs
        }

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
            _disruptor.RingBuffer.Publish(logEvent.Sequence);
        }

        internal LogEvent AllocateLogEvent()
        {
            long sequence;
            try
            {
                sequence = _disruptor.RingBuffer.TryNext();
            }
            catch (Exception)
            {
                Console.WriteLine($"Could get log event - ring buffer was full");
                return null;
            }

            var logEvent = _disruptor.RingBuffer[sequence];
            logEvent.Sequence = sequence;
            return logEvent;
        }

        private class LogEventHandler : IEventHandler<LogEvent>
        {
            private readonly List<IAppender> _appenders;
            private readonly Encoding _encoding;
            private readonly StringBuffer _stringBuffer;
            private readonly byte[] _destination;

            public LogEventHandler(List<IAppender> appenders, Encoding encoding)
            {
                _appenders = appenders;
                _encoding = encoding;

                _stringBuffer = new StringBuffer();
                _destination = new byte[1024];
            }

            public unsafe void OnEvent(LogEvent logEvent, long sequence, bool endOfBatch)
            {
                logEvent.WriteToStringBuffer(_stringBuffer);
                int bytesWritten;
                fixed (byte* dest = _destination)
                    bytesWritten = _stringBuffer.CopyTo(dest, 0, _stringBuffer.Count, _encoding);

                _stringBuffer.Clear();

                // Write to appenders
                foreach (var appender in _appenders)
                {
                    appender.WriteEvent(logEvent, _destination, bytesWritten);
                }
            }
        }
    }
}