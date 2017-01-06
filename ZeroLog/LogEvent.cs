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
        private const int _stringCapacity = 10;

        private readonly List<string> _strings = new List<string>(_stringCapacity);
        private readonly List<IntPtr> _argPointers = new List<IntPtr>(_stringCapacity);
        private Log _log;
        private DateTime _timestamp;
        private int _threadId;
        private Level _activeLevel;

        private readonly byte* _startOfBuffer;
        private readonly byte* _endOfBuffer;
        private byte* _dataPointer;

        public LogEvent(Level level, BufferSegment bufferSegment)
        {
            _activeLevel = level;
            _startOfBuffer = bufferSegment.Data;
            _dataPointer = bufferSegment.Data;
            _endOfBuffer = bufferSegment.Data + bufferSegment.Length;
        }

        internal long Sequence { get; set; }
        public Level Level { get; private set; }
        public string Name => _log.Name;
        public DateTime Timestamp => _timestamp;

        internal void Initialize(Level level, Log log)
        {
            _timestamp = DateTime.UtcNow;
            Level = level;
            _log = log;
            _strings.Clear();
            _dataPointer = _startOfBuffer;
            _threadId = Thread.CurrentThread.ManagedThreadId;
        }

        internal void AppendFormat(string format)
        {
            EnsureRemainingBytesAndStoreOffset(1);

            *_dataPointer = (byte)ArgumentType.Format;
            _dataPointer += sizeof(byte);
            *_dataPointer = (byte)_strings.Count;
            _dataPointer += sizeof(byte);

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
        public LogEvent Append(string s)
        {
            EnsureRemainingBytesAndStoreOffset(1);

            *_dataPointer = (byte)ArgumentType.String;
            _dataPointer += sizeof(byte);

            *_dataPointer = (byte)_strings.Count;
            _dataPointer += sizeof(byte);

            _strings.Add(s);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(bool b)
        {
            EnsureRemainingBytesAndStoreOffset(1);

            if (b)
                *_dataPointer = (byte)ArgumentType.BooleanTrue;
            else
                *_dataPointer = (byte)ArgumentType.BooleanFalse;

            _dataPointer += sizeof(byte);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(byte b)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(byte));

            *_dataPointer = (byte)ArgumentType.Byte;
            _dataPointer += sizeof(byte);

            *_dataPointer = b;
            _dataPointer += sizeof(byte);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(char c)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(char));

            *_dataPointer = (byte)ArgumentType.Char;
            _dataPointer += sizeof(byte);

            *(char*)_dataPointer = c;

            _dataPointer += sizeof(char);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(short s)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(short));

            *_dataPointer = (byte)ArgumentType.Int16;
            _dataPointer += sizeof(byte);

            *(short*)_dataPointer = s;
            _dataPointer += sizeof(short);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(int i)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(int));

            *_dataPointer = (byte)ArgumentType.Int32;
            _dataPointer += sizeof(byte);

            *(int*)_dataPointer = i;
            _dataPointer += sizeof(int);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(long l)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(long));

            *_dataPointer = (byte)ArgumentType.Int64;
            _dataPointer += sizeof(byte);

            *(long*)_dataPointer = l;
            _dataPointer += sizeof(long);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(float f)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(float));

            *_dataPointer = (byte)ArgumentType.Single;
            _dataPointer += sizeof(byte);

            *(float*)_dataPointer = f;
            _dataPointer += sizeof(float);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(double d)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(double));

            *_dataPointer = (byte)ArgumentType.Double;
            _dataPointer += sizeof(byte);

            *(double*)_dataPointer = d;
            _dataPointer += sizeof(double);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(decimal d)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(decimal));

            *_dataPointer = (byte)ArgumentType.Decimal;
            _dataPointer += sizeof(byte);

            *(decimal*)_dataPointer = d;
            _dataPointer += sizeof(decimal);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(Guid g)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(Guid));

            *_dataPointer = (byte)ArgumentType.Guid;
            _dataPointer += sizeof(byte);

            *(Guid*)_dataPointer = g;
            _dataPointer += sizeof(Guid);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(DateTime dt)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(ulong));

            *_dataPointer = (byte)ArgumentType.DateTime;
            _dataPointer += sizeof(byte);

            *(ulong*)_dataPointer = (ulong)dt.Ticks | ((ulong)dt.Kind << 62);
            _dataPointer += sizeof(ulong);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(TimeSpan ts)
        {
            EnsureRemainingBytesAndStoreOffset(1 + sizeof(long));

            *_dataPointer = (byte)ArgumentType.TimeSpan;
            _dataPointer += sizeof(byte);

            *(long*)_dataPointer = ts.Ticks;
            _dataPointer += sizeof(long);

            return this;
        }

        public void Log()
        {
            _log.Enqueue(this);
        }

        public void WriteToStringBuffer(StringBuffer stringBuffer)
        {
            var endOfData = _dataPointer;
            _dataPointer = _startOfBuffer;
            while (_dataPointer < endOfData)
            {
                _dataPointer += stringBuffer.Append(_dataPointer, StringView.Empty, _strings, _argPointers);
            }

            Debug.Assert(_dataPointer == endOfData, "Buffer over-read");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureRemainingBytesAndStoreOffset(int count)
        {
            if (_dataPointer + count > _endOfBuffer)
                throw new Exception("Buffer is full");

            _argPointers.Add(new IntPtr(_dataPointer));
        }
    }
}
