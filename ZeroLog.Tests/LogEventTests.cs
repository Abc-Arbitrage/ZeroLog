using System;
using System.Globalization;
using System.Text.Formatting;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public class LogEventTests
    {
        private LogEvent _logEvent;
        private StringBuffer _output;
        private BufferSegmentProvider _bufferSegmentProvider;

        [SetUp]
        public void SetUp()
        {
            _bufferSegmentProvider = new BufferSegmentProvider(1024, 1024);
            _logEvent = new LogEvent(Level.Finest, _bufferSegmentProvider.GetSegment());
            _output = new StringBuffer(128) { Culture = CultureInfo.InvariantCulture };
        }

        [Test]
        public void should_append_string()
        {
            _logEvent.Append("abc");
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
            _logEvent.Append(new DateTime(2017, 01, 12, 13, 14, 15));
            _logEvent.Append(new TimeSpan(1, 2, 3, 4, 5));

            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("AbCFalseTrue128£12345-128999999999999999999123.456789.012345.678902017-01-12 13:14:15.00002:03:04.005", _output.ToString());
        }

        [Test]
        public void should_append_format()
        {
            _logEvent.AppendFormat("{0} {2} {1}");
            _logEvent.Append(1);
            _logEvent.Append("3");
            _logEvent.Append(2);

            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1 2 3", _output.ToString());
        }
    }
}
