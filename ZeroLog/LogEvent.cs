using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Formatting;
using System.Threading;

namespace ZeroLog
{
    public unsafe class LogEvent
    {
        private const int _bufferSize = 1024;
        private const int _stringCapacity = 10;

        private readonly byte[] _buffer = new byte[_bufferSize];
        private readonly List<string> _strings = new List<string>(_stringCapacity);
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

        public LogEvent Append(string s)
        {
            EnsureRemainingBytes(1);

            _buffer[_position++] = (byte)ArgumentType.String;
            _strings.Add(s);

            return this;
        }

        public LogEvent Append(bool b)
        {
            EnsureRemainingBytes(1);

            if (b)
                _buffer[_position++] = (byte)ArgumentType.BooleanTrue;
            else
                _buffer[_position++] = (byte)ArgumentType.BooleanFalse;

            return this;
        }

        public LogEvent Append(byte b)
        {
            EnsureRemainingBytes(1 + sizeof(byte));

            _buffer[_position++] = (byte)ArgumentType.Byte;
            _buffer[_position++] = b;

            return this;
        }

        public LogEvent Append(char c)
        {
            EnsureRemainingBytes(1 + sizeof(char));

            _buffer[_position++] = (byte)ArgumentType.Char;

            fixed (byte* p = _buffer)
                *(char*)(p + _position) = c;

            _position += sizeof(char);

            return this;
        }

        public LogEvent Append(short s)
        {
            EnsureRemainingBytes(1 + sizeof(short));

            _buffer[_position++] = (byte)ArgumentType.Int16;

            fixed (byte* p = _buffer)
                *(short*)(p + _position) = s;

            _position += sizeof(short);

            return this;
        }

        public LogEvent Append(int i)
        {
            EnsureRemainingBytes(1 + sizeof(int));

            _buffer[_position++] = (byte)ArgumentType.Int32;

            fixed (byte* p = _buffer)
                *(int*)(p + _position) = i;

            _position += sizeof(int);

            return this;
        }

        public LogEvent Append(long l)
        {
            EnsureRemainingBytes(1 + sizeof(long));

            _buffer[_position++] = (byte)ArgumentType.Int64;

            fixed (byte* p = _buffer)
                *(long*)(p + _position) = l;

            _position += sizeof(long);

            return this;
        }

        public LogEvent Append(float f)
        {
            EnsureRemainingBytes(1 + sizeof(float));

            _buffer[_position++] = (byte)ArgumentType.Single;

            fixed (byte* p = _buffer)
                *(float*)(p + _position) = f;

            _position += sizeof(float);

            return this;
        }

        public LogEvent Append(double d)
        {
            EnsureRemainingBytes(1 + sizeof(double));

            _buffer[_position++] = (byte)ArgumentType.Double;

            fixed (byte* p = _buffer)
                *(double*)(p + _position) = d;

            _position += sizeof(double);

            return this;
        }

        public LogEvent Append(decimal d)
        {
            EnsureRemainingBytes(1 + sizeof(decimal));

            _buffer[_position++] = (byte)ArgumentType.Decimal;

            fixed (byte* p = _buffer)
                *(decimal*)(p + _position) = d;

            _position += sizeof(decimal);

            return this;
        }

        public LogEvent Append(Guid g)
        {
            EnsureRemainingBytes(1 + sizeof(Guid));

            _buffer[_position++] = (byte)ArgumentType.Decimal;

            fixed (byte* p = _buffer)
                *(Guid*)(p + _position) = g;

            _position += sizeof(Guid);

            return this;
        }

        public LogEvent Append(DateTime dt)
        {
            EnsureRemainingBytes(1 + sizeof(ulong));

            _buffer[_position++] = (byte)ArgumentType.DateTime;

            fixed (byte* p = _buffer)
                *(ulong*)(p + _position) = (ulong)dt.Ticks | ((ulong)dt.Kind << 62);

            _position += sizeof(ulong);

            return this;
        }

        public LogEvent Append(TimeSpan ts)
        {
            EnsureRemainingBytes(1 + sizeof(long));

            _buffer[_position++] = (byte)ArgumentType.DateTime;

            fixed (byte* p = _buffer)
                *(long*)(p + _position) = ts.Ticks;

            _position += sizeof(long);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureRemainingBytes(int count)
        {
            if (_position + count > _bufferSize)
                throw new Exception("Buffer is full");
        }
    }
}