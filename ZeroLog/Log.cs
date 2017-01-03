using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Formatting;
using System.Threading.Tasks;
using Roslyn.Utilities;

namespace ZeroLog
{
    public class Log
    {
        private readonly ConcurrentQueue<LogEvent> _queue = new ConcurrentQueue<LogEvent>();
        private readonly ObjectPool<LogEvent> _pool;
        private readonly IAppender[] _appenders;
        private readonly Encoding _encoding;

        public Log()
        {
            _encoding = Encoding.Default;
            _pool = new ObjectPool<LogEvent>(() => new LogEvent(this), 100);
            Task.Run(() => WriteToAppenders());
        }

        private unsafe void WriteToAppenders()
        {
            var stringBuffer = new StringBuffer();
            byte[] destination = new byte[1024];
            while (true)
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

        public LogEvent Fatal()
        {
            return GetLogEventFor(Level.Fatal);
        }

        public LogEvent Error()
        {
            return GetLogEventFor(Level.Error);
        }

        public LogEvent Warning()
        {
            return GetLogEventFor(Level.Warning);
        }

        public LogEvent Info()
        {
            return GetLogEventFor(Level.Info);
        }

        public LogEvent Debug()
        {
            return GetLogEventFor(Level.Debug);
        }

        public LogEvent Verbose()
        {
            return GetLogEventFor(Level.Verbose);
        }

        public LogEvent Finest()
        {
            return GetLogEventFor(Level.Finest);
        }

        private LogEvent GetLogEventFor(Level level)
        {
            var logEvent = _pool.Allocate();
            logEvent.SetLevel(level);
            return logEvent;
        }

        internal void Enqueue(LogEvent logEvent)
        {
            _queue.Enqueue(logEvent);
        }
    }
}
