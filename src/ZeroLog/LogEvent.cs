using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Formatting;
using System.Threading;
using InlineIL;
using ZeroLog.Appenders;
using ZeroLog.Utils;
using static InlineIL.IL.Emit;

#if false

namespace ZeroLog
{
    internal unsafe partial class LogEvent : IInternalLogEvent
    {
        private const int _maxArgCapacity = byte.MaxValue;
        private string[] _strings;
        private IntPtr[] _argPointers;
        private Log _log;
        private LogEventArgumentExhaustionStrategy _argumentExhaustionStrategy;

        protected readonly byte* _startOfBuffer;
        protected readonly byte* _endOfBuffer;
        protected byte* _dataPointer;
        private byte _argCount;
        private bool _isTruncated;

        public LogEvent(BufferSegment bufferSegment, int argCapacity)
        {
            argCapacity = Math.Min(argCapacity, _maxArgCapacity);
            _argPointers = new IntPtr[argCapacity];
            _strings = new string[argCapacity];

            _startOfBuffer = bufferSegment.Data;
            _dataPointer = bufferSegment.Data;
            _endOfBuffer = bufferSegment.Data + bufferSegment.Length;
            _log = default!;
        }

        public Level Level { get; private set; }
        public DateTime Timestamp { get; private set; }
        public Thread? Thread { get; private set; }
        public string Name => _log.Name;
        public IAppender[] Appenders => _log.Appenders;
        public virtual bool IsPooled => true;

        public void Initialize(Level level, Log log, LogEventArgumentExhaustionStrategy argumentExhaustionStrategy)
        {
            Timestamp = SystemDateTime.UtcNow;
            Level = level;
            _log = log;
            _argCount = 0;
            _dataPointer = _startOfBuffer;
            _isTruncated = false;
            _argumentExhaustionStrategy = argumentExhaustionStrategy;
            Thread = Thread.CurrentThread;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AppendGenericSlow<T>(T arg)
        {
            if (TypeUtilSlow<T>.IsNullableEnum)
                AppendNullableEnumInternal(arg);
            else if (TypeUtilSlow<T>.IsUnmanaged)
                AppendUnmanagedInternal(arg);
            else
                throw new NotSupportedException($"Type {typeof(T)} is not supported ");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent Append(string? s)
        {
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(byte), 1))
                return this;

            if (s == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.String);
            AppendString(s);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendKeyValue(string key, string? value)
        {
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(byte), 2))
                return this;

            AppendArgumentType(ArgumentType.KeyString);
            AppendString(key);

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.String);
            AppendString(value);
            return this;
        }

        public ILogEvent AppendKeyValue<T>(string key, T value)
            where T : struct, Enum
        {
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(EnumArg), 2))
                return this;

            AppendArgumentType(ArgumentType.KeyString);
            AppendString(key);

            AppendArgumentType(ArgumentType.Enum);
            *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value));
            _dataPointer += sizeof(EnumArg);
            return this;
        }

        public ILogEvent AppendKeyValue<T>(string key, T? value)
            where T : struct, Enum
        {
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(EnumArg), 2))
                return this;

            AppendArgumentType(ArgumentType.KeyString);
            AppendString(key);

            if (value == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.Enum);
            *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value.GetValueOrDefault()));
            _dataPointer += sizeof(EnumArg);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendKeyValueAscii(string key, byte[]? bytes, int length)
        {
            if (length < 0 || !PrepareAppend(sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(int) + length, 2))
                return this;

            AppendArgumentType(ArgumentType.KeyString);
            AppendString(key);

            if (bytes is null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);

            if (length != 0)
                AppendBytes(bytes, length);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendKeyValueAscii(string key, byte* bytes, int length)
        {
            if (length < 0 || !PrepareAppend(sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(int) + length, 2))
                return this;

            AppendArgumentType(ArgumentType.KeyString);
            AppendString(key);

            if (bytes == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return this;
            }

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);
            AppendBytes(bytes, length);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendKeyValueAscii(string key, ReadOnlySpan<byte> bytes)
        {
            var length = bytes.Length;
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(int) + length, 2))
                return this;

            AppendArgumentType(ArgumentType.KeyString);
            AppendString(key);

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);
            AppendBytes(bytes);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendKeyValueAscii(string key, ReadOnlySpan<char> chars)
        {
            var length = chars.Length;
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(int) + length, 2))
                return this;

            AppendArgumentType(ArgumentType.KeyString);
            AppendString(key);

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);

            foreach (var c in chars)
                *_dataPointer++ = (byte)c;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendAsciiString(byte[]? bytes, int length)
        {
            if (bytes == null)
            {
                if (PrepareAppend(sizeof(ArgumentType), 1))
                    AppendArgumentType(ArgumentType.Null);

                return this;
            }

            var remainingBytes = (int)(_endOfBuffer - _dataPointer);
            remainingBytes -= sizeof(ArgumentType) + sizeof(int);

            if (length > remainingBytes)
            {
                _isTruncated = true;
                length = remainingBytes;
            }

            if (length <= 0 || !PrepareAppend(sizeof(ArgumentType) + sizeof(int) + length, 1))
                return this;

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);
            AppendBytes(bytes, length);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendAsciiString(byte* bytes, int length)
        {
            if (bytes == null)
            {
                if (PrepareAppend(sizeof(ArgumentType), 1))
                    AppendArgumentType(ArgumentType.Null);

                return this;
            }

            var remainingBytes = (int)(_endOfBuffer - _dataPointer);
            remainingBytes -= sizeof(ArgumentType) + sizeof(int);

            if (length > remainingBytes)
            {
                _isTruncated = true;
                length = remainingBytes;
            }

            if (length <= 0 || !PrepareAppend(sizeof(ArgumentType) + sizeof(int) + length, 1))
                return this;

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);
            AppendBytes(bytes, length);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendAsciiString(ReadOnlySpan<byte> bytes)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer);
            remainingBytes -= sizeof(ArgumentType) + sizeof(int);

            var length = bytes.Length;

            if (length > remainingBytes)
            {
                _isTruncated = true;
                length = remainingBytes;
            }

            if (length <= 0 || !PrepareAppend(sizeof(ArgumentType) + sizeof(int) + length, 1))
                return this;

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);
            AppendBytes(bytes.Slice(0, length));
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendAsciiString(ReadOnlySpan<char> chars)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer);
            remainingBytes -= sizeof(ArgumentType) + sizeof(int);

            var length = chars.Length;

            if (length > remainingBytes)
            {
                _isTruncated = true;
                length = remainingBytes;
            }

            if (length <= 0 || !PrepareAppend(sizeof(ArgumentType) + sizeof(int) + length, 1))
                return this;

            AppendArgumentType(ArgumentType.AsciiString);
            AppendInt32(length);

            foreach (var c in chars.Slice(0, length))
                *_dataPointer++ = (byte)c;

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendEnum<T>(T value)
            where T : struct, Enum
        {
            return AppendEnumInternal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILogEvent AppendEnum<T>(T? value)
            where T : struct, Enum
        {
            if (value == null)
            {
                if (PrepareAppend(sizeof(ArgumentType), 1))
                    AppendArgumentType(ArgumentType.Null);

                return this;
            }

            return AppendEnumInternal(value.GetValueOrDefault());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ILogEvent AppendEnumInternal<T>(T value)
        {
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(EnumArg), 1))
                return this;

            AppendArgumentType(ArgumentType.Enum);
            *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value));
            _dataPointer += sizeof(EnumArg);
            return this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AppendNullableEnumInternal<T>(T value) // T = Nullable<SomeEnum>
        {
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(EnumArg), 1))
                return;

            var enumValue = EnumCache.ToUInt64Nullable(value);
            if (enumValue == null)
            {
                AppendArgumentType(ArgumentType.Null);
                return;
            }

            AppendArgumentType(ArgumentType.Enum);
            *(EnumArg*)_dataPointer = new EnumArg(TypeUtilSlow<T>.UnderlyingTypeHandle, enumValue.GetValueOrDefault());
            _dataPointer += sizeof(EnumArg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AppendUnmanagedInternal<T>(T arg) // T = unmanaged or Nullable<unmanaged>
        {
            if (!PrepareAppend(sizeof(ArgumentType) + sizeof(UnmanagedArgHeader) + UnsafeTools.SizeOf<T>(), 1))
                return;

            // If T is a Nullable<unmanaged>, we copy it as-is and let the formatter deal with it.
            // We're already in a slower execution path at this point.

            AppendArgumentType(ArgumentType.Unmanaged);
            *(UnmanagedArgHeader*)_dataPointer = new UnmanagedArgHeader(TypeUtil<T>.TypeHandle, UnsafeTools.SizeOf<T>());
            _dataPointer += sizeof(UnmanagedArgHeader);
            IL.Push(_dataPointer);
            IL.Push(arg);
            Stobj(typeof(T));
            _dataPointer += UnsafeTools.SizeOf<T>();
        }

        public void Log()
        {
            _log.Enqueue(this);
        }

        public void WriteToStringBuffer(StringBuffer stringBuffer, KeyValuePointerBuffer keyValuePointerBuffer)
        {
            var endOfData = _dataPointer;
            var dataPointer = _startOfBuffer;

            keyValuePointerBuffer.Clear();

            while (dataPointer < endOfData)
            {
                if (!ConsumeKeyValue(ref dataPointer, keyValuePointerBuffer))
                    stringBuffer.Append(ref dataPointer, StringView.Empty, _strings, _argPointers, _argCount);
            }

            Debug.Assert(dataPointer == endOfData, "Buffer over-read");

            if (keyValuePointerBuffer.KeyPointerCount > 0)
                JsonWriter.WriteJsonToStringBuffer(stringBuffer, keyValuePointerBuffer, _strings);

            if (_isTruncated)
                stringBuffer.Append(LogManager.Config.TruncatedMessageSuffix);
        }

        private static bool ConsumeKeyValue(ref byte* dataPointer, KeyValuePointerBuffer keyValuePointerBuffer)
        {
            var argumentType = (ArgumentType)(*dataPointer & ArgumentTypeMask.ArgumentType);
            if (argumentType != ArgumentType.KeyString)
                return false;

            // Save a pointer to the key for later when we append the Key/Value JSON.
            keyValuePointerBuffer.AddKeyPointer(dataPointer);

            // Skip the key.
            SkipCurrentArgument(ref dataPointer);

            // Skip the value.
            SkipCurrentArgument(ref dataPointer);

            return true;
        }

        private static void SkipCurrentArgument(ref byte* dataPointer)
        {
            var argumentType = (ArgumentType)(*dataPointer & ArgumentTypeMask.ArgumentType);
            dataPointer += sizeof(ArgumentType);

            switch (argumentType)
            {
                case ArgumentType.String:
                case ArgumentType.KeyString:
                case ArgumentType.Byte:
                    dataPointer += sizeof(byte);
                    break;

                case ArgumentType.AsciiString:
                    var length = *(int*)dataPointer;
                    dataPointer += sizeof(int) + length;
                    break;

                case ArgumentType.Boolean:
                    dataPointer += sizeof(bool);
                    break;

                case ArgumentType.Char:
                    dataPointer += sizeof(char);
                    break;

                case ArgumentType.Int16:
                    dataPointer += sizeof(short);
                    break;

                case ArgumentType.Int32:
                    dataPointer += sizeof(int);
                    break;

                case ArgumentType.Int64:
                    dataPointer += sizeof(long);
                    break;

                case ArgumentType.Single:
                    dataPointer += sizeof(float);
                    break;

                case ArgumentType.Double:
                    dataPointer += sizeof(double);
                    break;

                case ArgumentType.Decimal:
                    dataPointer += sizeof(decimal);
                    break;

                case ArgumentType.Guid:
                    dataPointer += sizeof(Guid);
                    break;

                case ArgumentType.DateTime:
                    dataPointer += sizeof(DateTime);
                    break;

                case ArgumentType.TimeSpan:
                    dataPointer += sizeof(TimeSpan);
                    break;

                case ArgumentType.Enum:
                    dataPointer += sizeof(EnumArg);
                    break;

                case ArgumentType.Null:
                    break;

                case ArgumentType.Unmanaged:
                    throw new NotSupportedException($"Type is not supported {argumentType}");

                default:
                    throw new ArgumentOutOfRangeException();
            }
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

            if (_isTruncated)
                stringBuffer.Append(LogManager.Config.TruncatedMessageSuffix);
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
                case ArgumentType.KeyString:
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

                case ArgumentType.Enum:
                    var enumArg = (EnumArg*)dataPointer;
                    dataPointer += sizeof(EnumArg);
                    enumArg->AppendTo(stringBuffer);
                    break;

                case ArgumentType.Unmanaged:
                    var unmanagedArgHeader = (UnmanagedArgHeader*)dataPointer;
                    dataPointer += sizeof(UnmanagedArgHeader);
                    unmanagedArgHeader->AppendUnformattedTo(stringBuffer, dataPointer);
                    dataPointer += unmanagedArgHeader->Size;
                    break;

                case ArgumentType.Null:
                    stringBuffer.Append(LogManager.Config.NullDisplayString);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool PrepareAppend(int requestedBytes, int requestedArgSlots)
            => _dataPointer + requestedBytes <= _endOfBuffer
               && _argCount + requestedArgSlots <= _argPointers.Length
               || PrepareAppendSlow(requestedBytes);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool PrepareAppendSlow(int requestedBytes)
        {
            if (_dataPointer + requestedBytes <= _endOfBuffer && _argumentExhaustionStrategy == LogEventArgumentExhaustionStrategy.Allocate)
            {
                var newCapacity = Math.Min(_argPointers.Length * 2, _maxArgCapacity);
                if (newCapacity > _argPointers.Length)
                {
                    Array.Resize(ref _argPointers, newCapacity);
                    Array.Resize(ref _strings, newCapacity);
                    return true;
                }
            }

            _isTruncated = true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendArgumentType(ArgumentType argumentType)
        {
            _argPointers[_argCount++] = new IntPtr(_dataPointer);
            *(ArgumentType*)_dataPointer = argumentType;
            _dataPointer += sizeof(ArgumentType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
        private void AppendArgumentTypeWithFormat(ArgumentType argumentType)
        {
            _argPointers[_argCount++] = new IntPtr(_dataPointer);
            *(ArgumentType*)_dataPointer = argumentType | (ArgumentType)ArgumentTypeMask.FormatSpecifier;
            _dataPointer += sizeof(ArgumentType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendString(string value)
        {
            var argIndex = _argCount - 1;
            *_dataPointer = (byte)argIndex;
            _dataPointer += sizeof(byte);
            _strings[argIndex] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendBytes(byte[] bytes, int length)
        {
            UnsafeTools.CopyBlockUnaligned(ref *_dataPointer, ref bytes[0], (uint)length);
            _dataPointer += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendBytes(byte* bytes, int length)
        {
            UnsafeTools.CopyBlockUnaligned(_dataPointer, bytes, (uint)length);
            _dataPointer += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendBytes(ReadOnlySpan<byte> bytes)
        {
            bytes.CopyTo(new Span<byte>(_dataPointer, bytes.Length));
            _dataPointer += bytes.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendInt32(int i)
        {
            *(int*)_dataPointer = i;
            _dataPointer += sizeof(int);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendByte(byte i)
        {
            *_dataPointer = i;
            _dataPointer += sizeof(byte);
        }

        public void SetTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }
}

#endif
