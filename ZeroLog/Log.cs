using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Roslyn.Utilities;

namespace ZeroLog
{
    public partial class Log
    {
        private readonly LogManager _logManager;
        private readonly ConcurrentQueue<LogEvent> _queue = new ConcurrentQueue<LogEvent>();
        private readonly ObjectPool<LogEvent> _pool;
        private readonly IAppender[] _appenders;
        private readonly Encoding _encoding;

        internal Log(LogManager logManager, string name)
        {
            _logManager = logManager;
            Name = name;
        }

        internal string Name { get; }

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
            var logEvent = _logManager.AllocateLogEvent();
            logEvent?.Initialize(level, this);
            return logEvent;
        }

        internal void Enqueue(LogEvent logEvent)
        {
            _logManager.Enqueue(logEvent);
        }
    }
}
