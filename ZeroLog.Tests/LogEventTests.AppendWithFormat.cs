using System;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public partial class LogEventTests
    {
        [Test]
        public void should_append_byte_with_format()
        {
            _logEvent.Append((byte)240, "X4");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("00F0", _output.ToString());
        }

        [Test]
        public void should_append_short_with_format()
        {
            _logEvent.Append((short)-23805, "X4");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("A303", _output.ToString());
        }

        [Test]
        public void should_append_int_with_format()
        {
            _logEvent.Append(-16325, "X");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("FFFFC03B", _output.ToString());
        }

        [Test]
        public void should_append_long_with_format()
        {
            _logEvent.Append(-16325L, "X");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("FFFFFFFFFFFFC03B", _output.ToString());
        }

        [Test]
        public void should_append_float_with_format()
        {
            _logEvent.Append(1054.32179F, "E");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("1.054322E+003", _output.ToString());
        }

        [Test]
        public void should_append_double_with_format()
        {
            _logEvent.Append(1054.32179d, "P3");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("105,432.179 %", _output.ToString());
        }

        [Test]
        public void should_append_decimal_with_format()
        {
            _logEvent.Append(16325.62m, "E04");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("1.6326E+004", _output.ToString());
        }

        [Test]
        public void should_append_guid_with_format()
        {
            _logEvent.Append(new Guid("129ac124-e588-47e5-9d3d-fa3a4d174e29"), "X");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("{0x129ac124,0xe588,0x47e5,{0x9d,0x3d,0xfa,0x3a,0x4d,0x17,0x4e,0x29}}", _output.ToString());
        }

        [Test]
        public void should_append_date_time_with_format()
        {
            _logEvent.Append(new DateTime(2017, 01, 12, 13, 14, 15), "yyyy-MM-dd");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("2017-01-12", _output.ToString());
        }

        [Test]
        public void should_append_time_span_with_format()
        {
            _logEvent.Append(new TimeSpan(1, 2, 3, 4, 5), "TODO in StringFormatter");
            _logEvent.WriteToStringBuffer(_output, _encoding);

            Assert.AreEqual("02:03:04.005", _output.ToString());
        }
    }
}
