using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Formatting;
using NFluent;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public partial class LogEventTests
    {
        private LogEvent _logEvent;
        private StringBuffer _output;
        private BufferSegmentProvider _bufferSegmentProvider;
        private Encoding _encoding;

        [SetUp]
        public void SetUp()
        {
            _bufferSegmentProvider = new BufferSegmentProvider(1024, 1024);
            _logEvent = new LogEvent(_bufferSegmentProvider.GetSegment());
            _output = new StringBuffer(128) { Culture = CultureInfo.InvariantCulture };
            _encoding = Encoding.Default;
        }

        [Test]
        public void should_append_string()
        {
            _logEvent.Append("abc");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("abc", _output.ToString());
        }

        [Test]
        public unsafe void should_append_weird_raw_string()
        {
            Encoding inputEncoding = Encoding.UTF32;
            var bytes = inputEncoding.GetBytes("Z̷͙̗̻͖̣̹͉̫̬̪̖̤͆ͤ̓ͫͭ̀̐͜͞ͅͅαлγo");
            _logEvent.Append(bytes, bytes.Length, inputEncoding);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Z̷͙̗̻͖̣̹͉̫̬̪̖̤͆ͤ̓ͫͭ̀̐͜͞ͅͅαлγo", _output.ToString());

            var outputBytes = new byte[128];
            fixed (byte* o = outputBytes)
            {
                Encoding outputEncoding = Encoding.UTF8;
                var bytesWritten = _output.CopyTo(o, outputBytes.Length, 0, _output.Count, outputEncoding);

                var expected = outputEncoding.GetBytes("Z̷͙̗̻͖̣̹͉̫̬̪̖̤͆ͤ̓ͫͭ̀̐͜͞ͅͅαлγo");
                Check.That(outputBytes.Take(bytesWritten)).ContainsExactly(expected);
            }
        }

        [TestCaseSource(nameof(GetEncodings))]
        public unsafe void should_append_raw_string(Encoding inputEncoding, Encoding outputEncoding)
        {
            var bytes = inputEncoding.GetBytes("abc");
            _logEvent.Append(bytes, bytes.Length, inputEncoding);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("abc", _output.ToString());

            var outputBytes = new byte[32];
            fixed (byte* o = outputBytes)
            {
                var bytesWritten = _output.CopyTo(o, outputBytes.Length, 0, _output.Count, outputEncoding);

                var expected = outputEncoding.GetBytes("abc");
                Check.That(outputBytes.Take(bytesWritten)).ContainsExactly(expected);
            }
        }

        private IEnumerable<TestCaseData> GetEncodings()
        {
            var allEncodings = new[] { Encoding.UTF8, Encoding.ASCII, Encoding.UTF32, Encoding.Unicode, Encoding.UTF7, Encoding.UTF8, };

            return from inputEncoding in allEncodings
                   from outputEncoding in allEncodings
                   select new TestCaseData(inputEncoding, outputEncoding);
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
        public unsafe void should_append_unsafe_byte_array()
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

            Assert.AreEqual("02:03:04.005", _output.ToString());
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

            Assert.AreEqual("AbCFalseTrue128£12345-128999999999999999999123.456789.012345.67890129ac124-e588-47e5-9d3d-fa3a4d174e292017-01-12 13:14:15.00002:03:04.005", _output.ToString());
        }

        [Test]
        public void should_append_format()
        {
            _logEvent.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}");
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

            Assert.AreEqual("AbCFalseTrue128£12345-128999999999999999999123.456789.012345.67890129ac124-e588-47e5-9d3d-fa3a4d174e292017-01-12 13:14:15.00002:03:04.005", _output.ToString());
        }
    }
}
