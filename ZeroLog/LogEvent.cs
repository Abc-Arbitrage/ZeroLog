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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AppendFormat(string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(1);
            AppendArgumentType(ArgumentType.FormatString);
            AppendString(format);
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
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType));
            AppendArgumentType(ArgumentType.String);
            AppendString(s);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(bool b)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType));
            AppendArgumentType(b ? ArgumentType.BooleanTrue : ArgumentType.BooleanFalse);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(byte b)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte));
            AppendArgumentType(ArgumentType.Byte);
            AppendByte(b);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(byte b, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + 2 * sizeof(byte));
            AppendArgumentType(ArgumentType.Byte, true);
            AppendString(format);
            AppendByte(b);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(char c)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(char));
            AppendArgumentType(ArgumentType.Char);
            AppendChar(c);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(short s)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(short));
            AppendArgumentType(ArgumentType.Int16);
            AppendShort(s);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(short s, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(short));
            AppendArgumentType(ArgumentType.Int16, true);
            AppendString(format);
            AppendShort(s);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(int i)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(int));
            AppendArgumentType(ArgumentType.Int32);
            AppendInt(i);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(int i, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(int));
            AppendArgumentType(ArgumentType.Int32, true);
            AppendString(format);
            AppendInt(i);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(long l)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(long));
            AppendArgumentType(ArgumentType.Int64);
            AppendLong(l);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(long l, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(long));
            AppendArgumentType(ArgumentType.Int64, true);
            AppendString(format);
            AppendLong(l);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(float f)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(float));
            AppendArgumentType(ArgumentType.Single);
            AppendFloat(f);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(float f, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(float));
            AppendArgumentType(ArgumentType.Single, true);
            AppendString(format);
            AppendFloat(f);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(double d)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(double));
            AppendArgumentType(ArgumentType.Double);
            AppendDouble(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(double d, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(double));
            AppendArgumentType(ArgumentType.Double, true);
            AppendString(format);
            AppendDouble(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(decimal d)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(decimal));
            AppendArgumentType(ArgumentType.Decimal);
            AppendDecimal(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(decimal d, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(decimal));
            AppendArgumentType(ArgumentType.Decimal, true);
            AppendString(format);
            AppendDecimal(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(Guid g)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(Guid));
            AppendArgumentType(ArgumentType.Guid);
            AppendGuid(g);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(Guid g, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(Guid));
            AppendArgumentType(ArgumentType.Guid, true);
            AppendString(format);
            AppendGuid(g);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(DateTime dt)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(ulong));
            AppendArgumentType(ArgumentType.DateTime);
            AppendDateTime(dt);
            return this;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(DateTime dt, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(ulong));
            AppendArgumentType(ArgumentType.DateTime, true);
            AppendString(format);
            AppendDateTime(dt);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(TimeSpan ts)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(long));
            AppendArgumentType(ArgumentType.TimeSpan);
            AppendTimeSpan(ts);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LogEvent Append(TimeSpan ts, string format)
        {
            EnsureRemainingBytesAndStoreArgPointer(sizeof(ArgumentType) + sizeof(byte) + sizeof(long));
            AppendArgumentType(ArgumentType.TimeSpan, true);
            AppendString(format);
            AppendTimeSpan(ts);
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
                stringBuffer.Append(ref _dataPointer, StringView.Empty, _strings, _argPointers);
            }

            Debug.Assert(_dataPointer == endOfData, "Buffer over-read");
        }

        private void EnsureRemainingBytesAndStoreArgPointer(int requestedBytes)
        {
            if (_dataPointer + requestedBytes > _endOfBuffer)
                throw new Exception("Buffer is full");

            _argPointers.Add(new IntPtr(_dataPointer));
        }

        private void AppendArgumentType(ArgumentType argumentType, bool withFormatSpecifier = false)
        {
            if (withFormatSpecifier)
                *_dataPointer = (byte)((byte)argumentType | ArgumentTypeMask.FormatSpecifier);
            else
                *_dataPointer = (byte)argumentType;

            _dataPointer += sizeof(byte);
        }

        private void AppendString(string value)
        {
            *_dataPointer = (byte)_strings.Count;
            _dataPointer += sizeof(byte);
            _strings.Add(value);
        }

        private void AppendByte(byte b)
        {
            *_dataPointer = b;
            _dataPointer += sizeof(byte);
        }

        private void AppendChar(char c)
        {
            *(char*)_dataPointer = c;
            _dataPointer += sizeof(char);
        }

        private void AppendShort(short s)
        {
            *(short*)_dataPointer = s;
            _dataPointer += sizeof(short);
        }

        private void AppendInt(int i)
        {
            *(int*)_dataPointer = i;
            _dataPointer += sizeof(int);
        }

        private void AppendLong(long l)
        {
            *(long*)_dataPointer = l;
            _dataPointer += sizeof(long);
        }

        private void AppendFloat(float f)
        {
            *(float*)_dataPointer = f;
            _dataPointer += sizeof(float);
        }

        private void AppendDouble(double d)
        {
            *(double*)_dataPointer = d;
            _dataPointer += sizeof(double);
        }

        private void AppendDecimal(decimal d)
        {
            *(decimal*)_dataPointer = d;
            _dataPointer += sizeof(decimal);
        }

        private void AppendGuid(Guid g)
        {
            *(Guid*)_dataPointer = g;
            _dataPointer += sizeof(Guid);
        }

        private void AppendDateTime(DateTime dt)
        {
            *(ulong*)_dataPointer = (ulong)dt.Ticks | ((ulong)dt.Kind << 62);
            _dataPointer += sizeof(ulong);
        }

        private void AppendTimeSpan(TimeSpan ts)
        {
            *(long*)_dataPointer = ts.Ticks;
            _dataPointer += sizeof(long);
        }
    }
}
