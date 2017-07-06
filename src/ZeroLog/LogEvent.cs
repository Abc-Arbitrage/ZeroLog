using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Formatting;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal unsafe class LogEvent : IInternalLogEvent
    {
        private const int _stringCapacity = 10;

        private readonly List<string> _strings = new List<string>(_stringCapacity);
        private readonly List<IntPtr> _argPointers = new List<IntPtr>(_stringCapacity);
        private Log _log;

        protected readonly byte* _startOfBuffer;
        protected readonly byte* _endOfBuffer;
        protected byte* _dataPointer;

        public LogEvent(BufferSegment bufferSegment)
        {
            _startOfBuffer = bufferSegment.Data;
            _dataPointer = bufferSegment.Data;
            _endOfBuffer = bufferSegment.Data + bufferSegment.Length;
        }

        public Level Level { get; private set; }
        public DateTime Timestamp { get; private set; }
        public int ThreadId { get; private set; }
        public string Name => _log.Name;
        public IList<IAppender> Appenders => _log.Appenders;
        public virtual bool IsPooled => true;

        public void Initialize(Level level, Log log)
        {
            Timestamp = SystemDateTime.UtcNow;
            Level = level;
            _log = log;
            _strings.Clear();
            _argPointers.Clear();
            _dataPointer = _startOfBuffer;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendFormat(string format)
        {
            if (!HasEnoughBytes(1))
                return;

            AppendArgumentType(ArgumentType.FormatString);
            AppendString(format);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AppendGeneric<T>(T arg)
        {
            // Some remarks here:
            // - The JIT knows the type of "arg" at runtime and will be able the remove useless branches for each
            //   struct specific jitted version of this method.
            // - Since a jitted version of this method will be shared for all reference types, the optimisation
            //   we just mentionned earlier can't occur. That's why we put the test against string at the top.
            // - Casting to "object" then to the desired value type will force the C# compiler to emit boxing and 
            //   unboxing IL opcodes, but the JIT is smart enough to prevent the actual boxing/unboxing from happening.

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
        public ILogEvent Append(string s)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType)))
                return this;

            AppendArgumentType(ArgumentType.String);
            AppendString(s);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendAsciiString(byte[] bytes, int length)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer);
            remainingBytes -= sizeof(ArgumentType) + sizeof(byte);
            if (remainingBytes <= 0)
                return this;

            length = Math.Min(length, remainingBytes);

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt(length);
            AppendBytes(bytes, length);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendAsciiString(byte* bytes, int length)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer);
            remainingBytes -= sizeof(ArgumentType) + sizeof(byte);
            if (remainingBytes <= 0)
                return this;

            length = Math.Min(length, remainingBytes);

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt(length);
            AppendBytes(bytes, length);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(bool b)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(bool)))
                return this;

            AppendArgumentType(ArgumentType.Boolean);
            AppendBool(b);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(byte b)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte)))
                return this;

            AppendArgumentType(ArgumentType.Byte);
            AppendByte(b);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(byte b, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + 2 * sizeof(byte)))
                return this;

            AppendArgumentType(ArgumentType.Byte, true);
            AppendString(format);
            AppendByte(b);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(char c)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(char)))
                return this;

            AppendArgumentType(ArgumentType.Char);
            AppendChar(c);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(short s)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(short)))
                return this;

            AppendArgumentType(ArgumentType.Int16);
            AppendShort(s);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(short s, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(short)))
                return this;

            AppendArgumentType(ArgumentType.Int16, true);
            AppendString(format);
            AppendShort(s);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(int i)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(int)))
                return this;

            AppendArgumentType(ArgumentType.Int32);
            AppendInt(i);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(int i, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(int)))
                return this;

            AppendArgumentType(ArgumentType.Int32, true);
            AppendString(format);
            AppendInt(i);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(long l)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(long)))
                return this;

            AppendArgumentType(ArgumentType.Int64);
            AppendLong(l);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(long l, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(long)))
                return this;

            AppendArgumentType(ArgumentType.Int64, true);
            AppendString(format);
            AppendLong(l);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(float f)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(float)))
                return this;

            AppendArgumentType(ArgumentType.Single);
            AppendFloat(f);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(float f, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(float)))
                return this;

            AppendArgumentType(ArgumentType.Single, true);
            AppendString(format);
            AppendFloat(f);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(double d)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(double)))
                return this;

            AppendArgumentType(ArgumentType.Double);
            AppendDouble(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(double d, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(double)))
                return this;

            AppendArgumentType(ArgumentType.Double, true);
            AppendString(format);
            AppendDouble(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(decimal d)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(decimal)))
                return this;

            AppendArgumentType(ArgumentType.Decimal);
            AppendDecimal(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(decimal d, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(decimal)))
                return this;

            AppendArgumentType(ArgumentType.Decimal, true);
            AppendString(format);
            AppendDecimal(d);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(Guid g)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(Guid)))
                return this;

            AppendArgumentType(ArgumentType.Guid);
            AppendGuid(g);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(Guid g, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(Guid)))
                return this;

            AppendArgumentType(ArgumentType.Guid, true);
            AppendString(format);
            AppendGuid(g);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(DateTime dt)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(DateTime)))
                return this;

            AppendArgumentType(ArgumentType.DateTime);
            AppendDateTime(dt);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(DateTime dt, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(DateTime)))
                return this;

            AppendArgumentType(ArgumentType.DateTime, true);
            AppendString(format);
            AppendDateTime(dt);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(TimeSpan ts)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(TimeSpan)))
                return this;

            AppendArgumentType(ArgumentType.TimeSpan);
            AppendTimeSpan(ts);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(TimeSpan ts, string format)
        {
            if (!HasEnoughBytes(sizeof(ArgumentType) + sizeof(byte) + sizeof(TimeSpan)))
                return this;

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
            var dataPointer = _startOfBuffer;

            while (dataPointer < endOfData)
            {
                stringBuffer.Append(ref dataPointer, StringView.Empty, _strings, _argPointers);
            }

            Debug.Assert(dataPointer == endOfData, "Buffer over-read");
        }

        public void WriteToStringBufferUnformatted(StringBuffer stringBuffer)
        {
            var endOfData = _dataPointer;
            var dataPointer = _startOfBuffer;

            while (dataPointer < endOfData)
            {
                AppendArgumentToStringBufferUnformatted(stringBuffer, ref dataPointer);

                if (dataPointer < endOfData)
                    stringBuffer.Append(", ");
            }

            Debug.Assert(dataPointer == endOfData, "Buffer over-read");
        }

        private void AppendArgumentToStringBufferUnformatted(StringBuffer stringBuffer, ref byte* dataPointer)
        {
            var argument = *dataPointer;
            dataPointer += sizeof(ArgumentType);

            var argumentType = (ArgumentType)(argument & ArgumentTypeMask.ArgumentType);

            var hasFormatSpecifier = (argument & ArgumentTypeMask.FormatSpecifier) != 0;
            if (hasFormatSpecifier)
                dataPointer += sizeof(byte); // Skip it

            switch (argumentType)
            {
                case ArgumentType.String:
                    stringBuffer.Append('"');
                    stringBuffer.Append(_strings[*dataPointer]);
                    stringBuffer.Append('"');
                    dataPointer += sizeof(byte);
                    break;

                case ArgumentType.AsciiString:
                    var length = *(int*)dataPointer;
                    dataPointer += sizeof(int);
                    stringBuffer.Append('"');
                    stringBuffer.Append(new AsciiString(dataPointer, length));
                    stringBuffer.Append('"');
                    dataPointer += length;
                    break;

                case ArgumentType.Boolean:
                    stringBuffer.Append(*(bool*)dataPointer);
                    dataPointer += sizeof(bool);
                    break;

                case ArgumentType.Byte:
                    stringBuffer.Append(*dataPointer, StringView.Empty);
                    dataPointer += sizeof(byte);
                    break;

                case ArgumentType.Char:
                    stringBuffer.Append('\'');
                    stringBuffer.Append(*(char*)dataPointer);
                    stringBuffer.Append('\'');
                    dataPointer += sizeof(char);
                    break;

                case ArgumentType.Int16:
                    stringBuffer.Append(*(short*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(short);
                    break;

                case ArgumentType.Int32:
                    stringBuffer.Append(*(int*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(int);
                    break;

                case ArgumentType.Int64:
                    stringBuffer.Append(*(long*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(long);
                    break;

                case ArgumentType.Single:
                    stringBuffer.Append(*(float*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(float);
                    break;

                case ArgumentType.Double:
                    stringBuffer.Append(*(double*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(double);
                    break;

                case ArgumentType.Decimal:
                    stringBuffer.Append(*(decimal*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(decimal);
                    break;

                case ArgumentType.Guid:
                    stringBuffer.Append(*(Guid*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(Guid);
                    break;

                case ArgumentType.DateTime:
                    stringBuffer.Append(*(DateTime*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(DateTime);
                    break;

                case ArgumentType.TimeSpan:
                    stringBuffer.Append(*(TimeSpan*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(TimeSpan);
                    break;

                case ArgumentType.FormatString:
                    stringBuffer.Append('"');
                    stringBuffer.Append(_strings[*dataPointer]);
                    stringBuffer.Append('"');
                    dataPointer += sizeof(byte);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool HasEnoughBytes(int requestedBytes)
        {
            return _dataPointer + requestedBytes <= _endOfBuffer;
        }

        private void AppendArgumentType(ArgumentType argumentType, bool withFormatSpecifier = false)
        {
            _argPointers.Add(new IntPtr(_dataPointer));

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

        private void AppendBool(bool b)
        {
            *(bool*)_dataPointer = b;
            _dataPointer += sizeof(bool);
        }

        private void AppendByte(byte b)
        {
            *_dataPointer = b;
            _dataPointer += sizeof(byte);
        }

        private void AppendBytes(byte[] bytes, int length)
        {
            fixed (byte* b = bytes)
            {
                for (var i = 0; i < length; i++)
                {
                    *_dataPointer = b[i];
                    _dataPointer += sizeof(byte);
                }
            }
        }

        private void AppendBytes(byte* bytes, int length)
        {
            for (var i = 0; i < length; i++)
            {
                *_dataPointer = bytes[i];
                _dataPointer += sizeof(byte);
            }
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
            *(DateTime*)_dataPointer = dt;
            _dataPointer += sizeof(DateTime);
        }

        private void AppendTimeSpan(TimeSpan ts)
        {
            *(TimeSpan*)_dataPointer = ts;
            _dataPointer += sizeof(TimeSpan);
        }

        public void SetTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }
}
