using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Formatting;
using System.Threading;

namespace ZeroLog
{
    public unsafe class LogEvent
    {
        private readonly byte[] _buffer = new byte[1024];
        private readonly List<string> _strings = new List<string>(10);
        private Log _log;
        private DateTime _timeStamp;
        private int _threadId;
        private Level _level;
        private int _position = 0;

        public LogEvent()
        {
        }

        internal void Initialize(Level level, Log log)
        {
            _timeStamp = DateTime.UtcNow;
            _level = level;
            _log = log;
            _strings.Clear();
            _position = 0;
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

        public LogEvent Format(string format)
        {
            Append(format);
            return this;
        }

        public LogEvent Append(string s)
        {
            _buffer[_position++] = (byte)ArgumentType.String;
            _strings.Add(s);
            return this;
        }

        public LogEvent Append(int i)
        {
            
            return this;
        }

        public void Log()
        {
            _log.Enqueue(this);
        }

        public void WriteToStringBuffer(StringBuffer formatted)
        {
            // Process _buffer
            
        }
    }
}