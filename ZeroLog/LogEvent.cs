using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly List<int> _offsets = new List<int>(_stringCapacity);
        private Log _log;
        private DateTime _timestamp;
        private int _threadId;
        private int _position = 0;
        private Level _activeLevel;

        public LogEvent(Level level)
        {
            _activeLevel = level;
        }

        public Level Level { get; private set; }
        public string Name => _log.Name;
        public DateTime Timestamp => _timestamp;

        internal void Initialize(Level level, Log log)
        {
            _timestamp = DateTime.UtcNow;
            Level = level;
            _log = log;
            _strings.Clear();
            _position = 0;
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

        internal void AppendFormat(string format)
        {
            EnsureRemainingBytesAndStoreOffset(1);

            _buffer[_position++] = (byte)ArgumentType.Format;
            _buffer[_position++] = (byte)_strings.Count;
            _strings.Add(format);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AppendGeneric<T>(T arg)
        {
            if (typeof(T) == typeof(string))
                Append((string)(object)arg);

            else if (typeof(T) == typeof(bool))
                Append((bool)(object)arg);

            else if (typeof(T) == typeof(byte))
                Append((byte)(object)arg);

            else if (typeof(T) == typeof(char))
                Append((char)(object)arg);

            else if (typeof(T) == typeof(short))
                Append((short)(object)arg);

            else if (typeof(T) == typeof(int))
                Append((int)(object)arg);

            else if (typeof(T) == typeof(long))
                Append((long)(object)arg);

            else if (typeof(T) == typeof(float))
                Append((float)(object)arg);

            else if (typeof(T) == typeof(double))
                Append((double)(object)arg);

            else if (typeof(T) == typeof(decimal))
                Append((decimal)(object)arg);

            else if (typeof(T) == typeof(Guid))
                Append((Guid)(object)arg);

            else if (typeof(T) == typeof(DateTime))
                Append((DateTime)(object)arg);

            else if (typeof(T) == typeof(TimeSpan))
                Append((TimeSpan)(object)arg);

            else
                throw new NotSupportedException($"Type {typeof(T)} is not supported ");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AppendGenericInlined<T>(T arg)
        {
            if (typeof(T) == typeof(string))
            {
                EnsureRemainingBytesAndStoreOffset(1);

                _buffer[_position++] = (byte)ArgumentType.String;
                _buffer[_position++] = (byte)_strings.Count;
                _strings.Add((string)(object)arg);
            }

            else if (typeof(T) == typeof(bool))
            {
                EnsureRemainingBytesAndStoreOffset(1);

                if ((bool)(object)arg)
                    _buffer[_position++] = (byte)ArgumentType.BooleanTrue;
                else
                    _buffer[_position++] = (byte)ArgumentType.BooleanFalse;
            }

            else if (typeof(T) == typeof(byte))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(byte));

                _buffer[_position++] = (byte)ArgumentType.Byte;
                _buffer[_position++] = (byte)(object)arg;
            }

            else if (typeof(T) == typeof(char))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(char));

                _buffer[_position++] = (byte)ArgumentType.Char;

                fixed (byte* p = _buffer)
                    *(char*)(p + _position) = (char)(object)arg;

                _position += sizeof(char);
            }

            else if (typeof(T) == typeof(short))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(short));

                _buffer[_position++] = (byte)ArgumentType.Int16;

                fixed (byte* p = _buffer)
                    *(short*)(p + _position) = (short)(object)arg;

                _position += sizeof(short);
            }

            else if (typeof(T) == typeof(int))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(int));

                _buffer[_position++] = (byte)ArgumentType.Int32;

                fixed (byte* p = _buffer)
                    *(int*)(p + _position) = (int)(object)arg;

                _position += sizeof(int);
            }

            else if (typeof(T) == typeof(long))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(long));

                _buffer[_position++] = (byte)ArgumentType.Int64;

                fixed (byte* p = _buffer)
                    *(long*)(p + _position) = (long)(object)arg;

                _position += sizeof(long);
            }

            else if (typeof(T) == typeof(float))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(float));

                _buffer[_position++] = (byte)ArgumentType.Single;

                fixed (byte* p = _buffer)
                    *(float*)(p + _position) = (float)(object)arg;

                _position += sizeof(float);
            }

            else if (typeof(T) == typeof(double))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(double));

                _buffer[_position++] = (byte)ArgumentType.Double;

                fixed (byte* p = _buffer)
                    *(double*)(p + _position) = (double)(object)arg;

                _position += sizeof(double);
            }

            else if (typeof(T) == typeof(decimal))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(decimal));

                _buffer[_position++] = (byte)ArgumentType.Decimal;

                fixed (byte* p = _buffer)
                    *(decimal*)(p + _position) = (decimal)(object)arg;

                _position += sizeof(decimal);
            }

            else if (typeof(T) == typeof(Guid))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(Guid));

                _buffer[_position++] = (byte)ArgumentType.Guid;

                fixed (byte* p = _buffer)
                    *(Guid*)(p + _position) = (Guid)(object)arg;

                _position += sizeof(Guid);
            }

            else if (typeof(T) == typeof(DateTime))
            {
                DateTime dt = (DateTime)(object)arg;
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(ulong));

                _buffer[_position++] = (byte)ArgumentType.DateTime;

                fixed (byte* p = _buffer)
                    *(ulong*)(p + _position) = (ulong)dt.Ticks | ((ulong)dt.Kind << 62);

                _position += sizeof(ulong);
            }

            else if (typeof(T) == typeof(TimeSpan))
            {
                EnsureRemainingBytesAndStoreOffset(1 + sizeof(long));

                _buffer[_position++] = (byte)ArgumentType.TimeSpan;

                fixed (byte* p = _buffer)
                    *(long*)(p + _position) = ((TimeSpan)(object)arg).Ticks;

                _position += sizeof(long);
            }

            else
                throw new NotSupportedException($"Type {typeof(T)} is not supported ");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(string s)
        {
            EnsureRemainingBytesAndStoreOffset(1);

            _buffer[_position++] = (byte)ArgumentType.String;
            _buffer[_position++] = (byte)_strings.Count;
            _strings.Add(s);
            
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(bool b)
        {
            EnsureRemainingBytesAndStoreOffset(1);

            if (b)
                _buffer[_position++] = (byte)ArgumentType.BooleanTrue;
            else
                _buffer[_position++] = (byte)ArgumentType.BooleanFalse;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(byte b)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(byte));

            _buffer[_position++] = (byte)ArgumentType.Byte;
            _buffer[_position++] = b;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(char c)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(char));

            _buffer[_position++] = (byte)ArgumentType.Char;

            fixed (byte* p = _buffer)
                *(char*)(p + _position) = c;

            _position += sizeof(char);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(short s)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(short));

            _buffer[_position++] = (byte)ArgumentType.Int16;

            fixed (byte* p = _buffer)
                *(short*)(p + _position) = s;

            _position += sizeof(short);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(int i)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(int));

            _buffer[_position++] = (byte)ArgumentType.Int32;

            fixed (byte* p = _buffer)
                *(int*)(p + _position) = i;

            _position += sizeof(int);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(long l)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(long));

            _buffer[_position++] = (byte)ArgumentType.Int64;

            fixed (byte* p = _buffer)
                *(long*)(p + _position) = l;

            _position += sizeof(long);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(float f)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(float));

            _buffer[_position++] = (byte)ArgumentType.Single;

            fixed (byte* p = _buffer)
                *(float*)(p + _position) = f;

            _position += sizeof(float);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(double d)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(double));

            _buffer[_position++] = (byte)ArgumentType.Double;

            fixed (byte* p = _buffer)
                *(double*)(p + _position) = d;

            _position += sizeof(double);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(decimal d)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(decimal));

            _buffer[_position++] = (byte)ArgumentType.Decimal;

            fixed (byte* p = _buffer)
                *(decimal*)(p + _position) = d;

            _position += sizeof(decimal);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(Guid g)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(Guid));

            _buffer[_position++] = (byte)ArgumentType.Guid;

            fixed (byte* p = _buffer)
                *(Guid*)(p + _position) = g;

            _position += sizeof(Guid);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(DateTime dt)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(ulong));

            _buffer[_position++] = (byte)ArgumentType.DateTime;

            fixed (byte* p = _buffer)
                *(ulong*)(p + _position) = (ulong)dt.Ticks | ((ulong)dt.Kind << 62);

            _position += sizeof(ulong);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(TimeSpan ts)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(long));

            _buffer[_position++] = (byte)ArgumentType.TimeSpan;

            fixed (byte* p = _buffer)
                *(long*)(p + _position) = ts.Ticks;

            _position += sizeof(long);

            return this;
        }

        public void Log()
        {
            _log.Enqueue(this);
        }

        public void WriteToStringBuffer(StringBuffer stringBuffer)
        {
            var offset = 0;
            while (offset < _position)
                offset += stringBuffer.AppendFrom(_buffer, offset, StringView.Empty, _strings, _offsets);

            //Debug.Assert(offset == _position, "Buffer over-read");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureRemainingBytesAndStoreOffset(int count)
        {
            if (_position + count > _bufferSize)
                throw new Exception("Buffer is full");

            _offsets.Add(_position);
        }
    }
}