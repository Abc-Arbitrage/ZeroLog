using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Formatting;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public unsafe partial class LogEventTests
    {
        private const int _bufferLength = 1024;
        private const int _argCapacity = 10;

        private LogEvent _logEvent;
        private StringBuffer _output;
        private GCHandle _bufferHandler;

        [SetUp]
        public void SetUp()
        {
            var buffer = new byte[_bufferLength];
            _bufferHandler = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            var bufferSegment = new BufferSegment((byte*)_bufferHandler.AddrOfPinnedObject().ToPointer(), buffer.Length);
            _logEvent = new LogEvent(bufferSegment, _argCapacity);
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.Allocate);
            _output = new StringBuffer(128) { Culture = CultureInfo.InvariantCulture };
        }

        [TearDown]
        public void Teardown()
        {
            _bufferHandler.Free();
        }

        [Test]
        public void should_append_string()
        {
            _logEvent.Append("abc");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("abc", _output.ToString());
        }

        [Test]
        public void should_append_null_string()
        {
            _logEvent.Append((string)null);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_byte_array()
        {
            var bytes = Encoding.Default.GetBytes("abc");
            _logEvent.AppendAsciiString(bytes, bytes.Length);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("abc", _output.ToString());
        }

        [Test]
        public void should_append_byte_span()
        {
            var bytes = Encoding.Default.GetBytes("abc");
            _logEvent.AppendAsciiString(bytes.AsSpan());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("abc", _output.ToString());
        }

        [Test]
        public void should_append_char_span()
        {
            _logEvent.AppendAsciiString("abc".AsSpan());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("abc", _output.ToString());
        }

        [Test]
        public void should_ignore_byte_array_with_negative_length()
        {
            var bytes = Encoding.Default.GetBytes("abc");
            _logEvent.AppendAsciiString(bytes, -1);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("", _output.ToString());
        }

        [Test]
        public void should_ignore_empty_byte_array()
        {
            _logEvent.AppendAsciiString(new byte[0], 0);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("", _output.ToString());
        }

        [Test]
        public void should_ignore_empty_byte_span()
        {
            _logEvent.AppendAsciiString(ReadOnlySpan<byte>.Empty);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("", _output.ToString());
        }

        [Test]
        public void should_ignore_empty_char_span()
        {
            _logEvent.AppendAsciiString(ReadOnlySpan<char>.Empty);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("", _output.ToString());
        }

        [Test]
        public void should_truncate_byte_array_after_too_many_args()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.TruncateMessage);
            for (var i = 0; i < _argCapacity; ++i)
                _logEvent.Append(".");

            var bytes = Encoding.Default.GetBytes("abc");
            _logEvent.AppendAsciiString(bytes, bytes.Length);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(new string('.', _argCapacity) + LogManager.Config.TruncatedMessageSuffix, _output.ToString());
        }

        [Test]
        public void should_truncate_byte_span_after_too_many_args()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.TruncateMessage);
            for (var i = 0; i < _argCapacity; ++i)
                _logEvent.Append(".");

            var bytes = Encoding.Default.GetBytes("abc");
            _logEvent.AppendAsciiString(bytes.AsSpan());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(new string('.', _argCapacity) + LogManager.Config.TruncatedMessageSuffix, _output.ToString());
        }

        [Test]
        public void should_truncate_char_span_after_too_many_args()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.TruncateMessage);
            for (var i = 0; i < _argCapacity; ++i)
                _logEvent.Append(".");

            _logEvent.AppendAsciiString("abc".AsSpan());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(new string('.', _argCapacity) + LogManager.Config.TruncatedMessageSuffix, _output.ToString());
        }

        [Test]
        public void should_append_null_byte_array()
        {
            _logEvent.AppendAsciiString((byte[])null, 0);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_unsafe_byte_array()
        {
            var bytes = Encoding.Default.GetBytes("abc");
            fixed (byte* b = bytes)
            {
                _logEvent.AppendAsciiString(b, bytes.Length);
            }

            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("abc", _output.ToString());
        }

        [Test]
        public void should_truncate_unsafe_byte_array_after_too_many_args()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.TruncateMessage);
            for (var i = 0; i < _argCapacity; ++i)
                _logEvent.Append(".");

            var bytes = Encoding.Default.GetBytes("abc");

            fixed (byte* b = bytes)
            {
                _logEvent.AppendAsciiString(b, bytes.Length);
            }

            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(new string('.', _argCapacity) + LogManager.Config.TruncatedMessageSuffix, _output.ToString());
        }

        [Test]
        public void should_append_null_unsafe_byte_array()
        {
            _logEvent.AppendAsciiString((byte*)null, 0);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_true()
        {
            _logEvent.Append(true);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("True", _output.ToString());
        }

        [Test]
        public void should_append_false()
        {
            _logEvent.Append(false);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("False", _output.ToString());
        }

        [Test]
        public void should_append_byte()
        {
            _logEvent.Append((byte)255);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("255", _output.ToString());
        }

        [Test]
        public void should_append_char()
        {
            _logEvent.Append('€');
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("€", _output.ToString());
        }

        [Test]
        public void should_append_short()
        {
            _logEvent.Append((short)4321);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("4321", _output.ToString());
        }

        [Test]
        public void should_append_int()
        {
            _logEvent.Append(1234567890);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1234567890", _output.ToString());
        }

        [Test]
        public void should_append_long()
        {
            _logEvent.Append(1234567890123456789L);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1234567890123456789", _output.ToString());
        }

        [Test]
        public void should_append_float()
        {
            _logEvent.Append(0.123f);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("0.123", _output.ToString());
        }

        [Test]
        public void should_append_double()
        {
            _logEvent.Append(0.123d);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("0.123", _output.ToString());
        }

        [Test]
        public void should_append_decimal()
        {
            _logEvent.Append(792281625142643.37593543950335m);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("792281625142643.37593543950335", _output.ToString());
        }

        [Test]
        public void should_append_guid()
        {
            _logEvent.Append(new Guid("129ac124-e588-47e5-9d3d-fa3a4d174e29"));
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("129ac124-e588-47e5-9d3d-fa3a4d174e29", _output.ToString());
        }

        [Test]
        public void should_append_date_time()
        {
            _logEvent.Append(new DateTime(2017, 01, 12, 13, 14, 15));
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("2017-01-12 13:14:15.000", _output.ToString());
        }

        [Test]
        public void should_append_time_span()
        {
            _logEvent.Append(new TimeSpan(1, 2, 3, 4, 5));
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1.02:03:04.0050000", _output.ToString());
        }

        [Test]
        public void should_append_all_types()
        {
            _logEvent.Append("AbC");
            _logEvent.Append(false);
            _logEvent.Append(true);
            _logEvent.Append((byte)128);
            _logEvent.Append('£');
            _logEvent.Append((short)12345);
            _logEvent.Append(-128);
            _logEvent.Append(999999999999999999L);
            _logEvent.Append(123.456f);
            _logEvent.Append(789.012d);
            _logEvent.Append(345.67890m);
            _logEvent.Append(new Guid("129ac124-e588-47e5-9d3d-fa3a4d174e29"));
            _logEvent.Append(new DateTime(2017, 01, 12, 13, 14, 15));
            _logEvent.Append(new TimeSpan(1, 2, 3, 4, 5));

            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("AbCFalseTrue128£12345-128999999999999999999123.456789.012345.67890129ac124-e588-47e5-9d3d-fa3a4d174e292017-01-12 13:14:15.0001.02:03:04.0050000", _output.ToString());
        }

        [Test]
        public void should_append_unmanaged_from_append_generic()
        {
            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            _logEvent.AppendGeneric(new UnmanagedStruct()
            {
                A = 1,
                B = 2,
                C = 3
            });

            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1-2-3", _output.ToString());
        }

        [TestCase(typeof(bool))]
        [TestCase(typeof(byte))]
        [TestCase(typeof(char))]
        [TestCase(typeof(short))]
        [TestCase(typeof(int))]
        [TestCase(typeof(long))]
        [TestCase(typeof(float))]
        [TestCase(typeof(double))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(Guid))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(TimeSpan))]
        public void should_append_nullable(Type type)
        {
            typeof(LogEventTests).GetMethod(nameof(should_append_nullable), BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null)
                                 .MakeGenericMethod(type)
                                 .Invoke(this, new object[0]);
        }

        private void should_append_nullable<T>()
            where T : struct
        {
            ((dynamic)_logEvent).Append((T?)null);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());

            _output.Clear();
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.Default);

            ((dynamic)_logEvent).AppendGeneric((T?)null);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());

            _output.Clear();
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.Default);

            ((dynamic)_logEvent).Append((T?)new T());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreNotEqual("null", _output.ToString());

            _output.Clear();
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.Default);

            ((dynamic)_logEvent).AppendGeneric((T?)new T());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreNotEqual("null", _output.ToString());
        }

        [Test]
        public void should_truncate_log_message()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.TruncateMessage);

            for (var i = 0; i < _argCapacity; ++i)
                _logEvent.Append(".");

            _logEvent.Append("!");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(new string('.', _argCapacity) + LogManager.Config.TruncatedMessageSuffix, _output.ToString());
        }

        [Test]
        public void should_not_truncate_log_message()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.Allocate);

            for (var i = 0; i < _argCapacity * 2; ++i)
                _logEvent.Append(".");

            _logEvent.Append("!");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(new string('.', _argCapacity * 2) + "!", _output.ToString());
        }
    }
}
