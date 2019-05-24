using System;
using System.Reflection;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public partial class LogEventTests
    {
        [Test]
        public void should_append_byte_with_format()
        {
            _logEvent.Append((byte)240, "X4");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("00F0", _output.ToString());
        }

        [Test]
        public void should_append_short_with_format()
        {
            _logEvent.Append((short)-23805, "X4");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("A303", _output.ToString());
        }

        [Test]
        public void should_append_int_with_format()
        {
            _logEvent.Append(-16325, "X");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("FFFFC03B", _output.ToString());
        }

        [Test]
        public void should_append_long_with_format()
        {
            _logEvent.Append(-16325L, "X");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("FFFFFFFFFFFFC03B", _output.ToString());
        }

        [Test]
        public void should_append_float_with_format()
        {
            _logEvent.Append(1054.32179F, "E");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1.054322E+003", _output.ToString());
        }

        [Test]
        public void should_append_double_with_format()
        {
            _logEvent.Append(1054.32179d, "P3");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("105,432.179 %", _output.ToString());
        }

        [Test]
        public void should_append_decimal_with_format()
        {
            _logEvent.Append(16325.62m, "E04");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1.6326E+004", _output.ToString());
        }

        [Test]
        public void should_append_guid_with_format()
        {
            _logEvent.Append(new Guid("129ac124-e588-47e5-9d3d-fa3a4d174e29"), "X");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("{0x129ac124,0xe588,0x47e5,{0x9d,0x3d,0xfa,0x3a,0x4d,0x17,0x4e,0x29}}", _output.ToString());
        }

        [Test]
        public void should_append_date_time_with_format()
        {
            _logEvent.Append(new DateTime(2017, 01, 12, 13, 14, 15), "yyyy-MM-dd");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("2017-01-12", _output.ToString());
        }

        [Test]
        public void should_append_time_span_with_format()
        {
            _logEvent.Append(new TimeSpan(1, 2, 3, 4, 5), "TODO in StringFormatter");
            _logEvent.WriteToStringBuffer(_output);

            Assert.IsNotEmpty(_output.ToString());

            // TODO: StringFormatter doesn't handle TimeSpan formats right now
            // Assert.AreEqual("1.02:03:04.0050000", _output.ToString());
        }

        [TestCase(typeof(byte), "X4")]
        [TestCase(typeof(short), "X4")]
        [TestCase(typeof(int), "X")]
        [TestCase(typeof(long), "X")]
        [TestCase(typeof(float), "E")]
        [TestCase(typeof(double), "P3")]
        [TestCase(typeof(decimal), "E04")]
        [TestCase(typeof(Guid), "X")]
        [TestCase(typeof(DateTime), "yyyy-MM-dd")]
        [TestCase(typeof(TimeSpan), "TODO in StringFormatter")]
        public void should_append_nullable_with_format(Type type, string format)
        {
            typeof(LogEventTests).GetMethod(nameof(should_append_nullable_with_format), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string) }, null)
                                 .MakeGenericMethod(type)
                                 .Invoke(this, new object[] { format });
        }

        private void should_append_nullable_with_format<T>(string format)
            where T : struct
        {
            ((dynamic)_logEvent).Append((T?)new T(), format);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreNotEqual("null", _output.ToString());

            _output.Clear();
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.Default);

            ((dynamic)_logEvent).Append((T?)null, format);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }
    }
}
