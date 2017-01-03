using System;
using System.Collections.Generic;
using System.IO;

namespace ZeroLog
{
    public unsafe class LogEvent
    {
        private readonly Log _log;
        private readonly byte[] _buffer = new byte[1024];
        private readonly List<string> _strings = new List<string>(10);
        private readonly DateTime _timeStamp;
        private readonly int _threadId;
        private Level _level;
        private int _position = 0;

        public LogEvent(Log log)
        {
            _log = log;
            _timeStamp = DateTime.UtcNow;
        }

        internal void SetLevel(Level level)
        {
            _level = level;
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