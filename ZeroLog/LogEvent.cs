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
        private Log _log;
        private DateTime _timeStamp;
        private int _threadId;
        private int _position = 0;
        private Level _activeLevel;

        public LogEvent(Level level)
        {
            _activeLevel = level;
        }

        public Level Level { get; private set; }
        public string Name => _log.Name;

        internal void Initialize(Level level, Log log)
        {
            _timeStamp = DateTime.UtcNow;
            Level = level;
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

            _buffer[_position++] = (byte)ArgumentType.Guid;

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
            var readBytes = 0;
            var nextStringIndex = 0;

            while (readBytes < _position)
            {
                var argumentType = (ArgumentType)_buffer[readBytes++];

                switch (argumentType)
                {
                    case ArgumentType.String:
                        stringBuffer.Append(_strings[nextStringIndex++]);
                        break;

                    case ArgumentType.BooleanTrue:
                        stringBuffer.Append(true);
                        break;

                    case ArgumentType.BooleanFalse:
                        stringBuffer.Append(false);
                        break;

                    case ArgumentType.Byte:
                        stringBuffer.Append(_buffer[readBytes++], StringView.Empty);
                        break;

                    case ArgumentType.Char:
                        stringBuffer.Append(BitConverter.ToChar(_buffer, readBytes));
                        readBytes += sizeof(char);
                        break;

                    case ArgumentType.Int16:
                        stringBuffer.Append(BitConverter.ToInt16(_buffer, readBytes), StringView.Empty);
                        readBytes += sizeof(short);
                        break;

                    case ArgumentType.Int32:
                        stringBuffer.Append(BitConverter.ToInt32(_buffer, readBytes), StringView.Empty);
                        readBytes += sizeof(int);
                        break;

                    case ArgumentType.Int64:
                        stringBuffer.Append(BitConverter.ToInt64(_buffer, readBytes), StringView.Empty);
                        readBytes += sizeof(long);
                        break;

                    case ArgumentType.Single:
                        stringBuffer.Append(BitConverter.ToSingle(_buffer, readBytes), StringView.Empty);
                        readBytes += sizeof(float);
                        break;

                    case ArgumentType.Double:
                        stringBuffer.Append(BitConverter.ToDouble(_buffer, readBytes), StringView.Empty);
                        readBytes += sizeof(double);
                        break;

                    case ArgumentType.Decimal:
                        stringBuffer.Append(ReadDecimal(readBytes), StringView.Empty);
                        readBytes += sizeof(decimal);
                        break;

                    case ArgumentType.Guid:
                        var guid = ReadGuid(readBytes);
                        readBytes += sizeof(Guid);
                        throw new NotImplementedException(); //TODO

                    case ArgumentType.DateTime:
                        var dateTime = ReadDateTime(readBytes); //TODO
                        readBytes += sizeof(ulong);
                        throw new NotImplementedException();

                    case ArgumentType.TimeSpan:
                        var timeSpan = ReadTimeSpan(readBytes);
                        readBytes += sizeof(long);
                        throw new NotImplementedException(); //TODO

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Debug.Assert(readBytes == _position, "Buffer over-read");
        }

        private decimal ReadDecimal(int position)
        {
            fixed (byte* pbyte = _buffer)
                return *(decimal*)(pbyte + position);
        }

        private Guid ReadGuid(int position)
        {
            fixed (byte* pbyte = _buffer)
                return *(Guid*)(pbyte + position);
        }

        private DateTime ReadDateTime(int position)
        {
            var dateData = BitConverter.ToUInt64(_buffer, position);
            var ticks = (long)(dateData & 0x3FFFFFFFFFFFFFFF);
            var kind = (DateTimeKind)(dateData & 0xC000000000000000);
            return new DateTime(ticks, kind); 
        }

        private TimeSpan ReadTimeSpan(int position)
        {
            var ticks = BitConverter.ToInt64(_buffer, position);
            return new TimeSpan(ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureRemainingBytes(int count)
        {
            if (_position + count > _bufferSize)
                throw new Exception("Buffer is full");
        }
    }
}