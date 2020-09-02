using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Formatting;
using NFluent;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public unsafe class LogEventEdgeCaseTests
    {
        private const int _bufferSize = 1024;
        private const int _asciiHeaderSize = sizeof(ArgumentType) + sizeof(int);
        private LogEvent _logEvent;
        private StringBuffer _output;
        private GCHandle _bufferHandler;

        [SetUp]
        public void SetUp()
        {
            var buffer = new byte[_bufferSize];
            _bufferHandler = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            var bufferSegment = new BufferSegment((byte*)_bufferHandler.AddrOfPinnedObject().ToPointer(), buffer.Length);
            _logEvent = new LogEvent(bufferSegment, 10);
            _output = new StringBuffer(128) { Culture = CultureInfo.InvariantCulture };
        }

        [Test]
        public void should_truncate_ascii_string_if_buffer_is_not_large_enough()
        {
            var largeString = new string('a', 2000);
            var asciiBytes = Encoding.ASCII.GetBytes(largeString);

            _logEvent.AppendAsciiString(asciiBytes, asciiBytes.Length);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(_output.ToString().Length).Equals(_bufferSize - _asciiHeaderSize + LogManager.Config.TruncatedMessageSuffix.Length);
        }

        [Test]
        public void should_ignore_ascii_string_if_buffer_is_not_large_enough_for_header(
            [Range(_bufferSize - 2 * _asciiHeaderSize, _bufferSize)]
            int firstStringLength)
        {
            var largeString1 = new string('a', firstStringLength);
            var asciiBytes1 = Encoding.ASCII.GetBytes(largeString1);
            _logEvent.AppendAsciiString(asciiBytes1, asciiBytes1.Length);

            var largeString2 = new string('b', _bufferSize);
            var asciiBytes2 = Encoding.ASCII.GetBytes(largeString2);
            _logEvent.AppendAsciiString(asciiBytes2, asciiBytes2.Length);

            _logEvent.WriteToStringBuffer(_output);

            var expectedTextLength = Math.Min(firstStringLength, _bufferSize - _asciiHeaderSize);
            Check.That(_output.ToString()).IsEqualTo(new string('a', expectedTextLength) + LogManager.Config.TruncatedMessageSuffix);
        }

        [Test]
        public void should_truncate_raw_ascii_string_if_buffer_is_not_large_enough()
        {
            var largeString = new string('a', 2000);
            var asciiBytes = Encoding.ASCII.GetBytes(largeString);

            fixed (byte* pAsciiBytes = asciiBytes)
            {
                _logEvent.AppendAsciiString(pAsciiBytes, asciiBytes.Length);
            }

            _logEvent.WriteToStringBuffer(_output);

            Check.That(_output.ToString().Length).Equals(_bufferSize - _asciiHeaderSize + LogManager.Config.TruncatedMessageSuffix.Length);
        }

        [Test]
        public void should_ignore_append_string_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append("abc");
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_true_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(true);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_false_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(false);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_byte_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append((byte)255);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_char_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append('€');
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_short_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append((short)4321);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_int_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(1234567890);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_long_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(1234567890123456789L);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_float_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(0.123f);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_double_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(0.123d);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_decimal_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(792281625142643.37593543950335m);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_guid_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(new Guid("129ac124-e588-47e5-9d3d-fa3a4d174e29"));
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_date_time_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(new DateTime(2017, 01, 12, 13, 14, 15));
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_time_span_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.Append(new TimeSpan(1, 2, 3, 4, 5));
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        [Test]
        public void should_ignore_append_key_values_if_buffer_is_full()
        {
            FillBufferWithWhiteSpaces();
            _logEvent.AppendKeyValue("key1", (string)null)
                     .AppendKeyValue("key2", "val2")
                     .AppendKeyValue("key3", 3);
            _logEvent.WriteToStringBuffer(_output);

            Check.That(string.IsNullOrWhiteSpace(_output.ToString()));
        }

        private void FillBufferWithWhiteSpaces()
        {
            var largeString = new string(' ', _bufferSize);
            var bytes = Encoding.ASCII.GetBytes(largeString);

            _logEvent.AppendAsciiString(bytes, bytes.Length);
        }
    }
}
