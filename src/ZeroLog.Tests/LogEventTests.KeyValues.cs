using System;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public partial class LogEventTests
    {
        [Test]
        public void should_append_single_key_value()
        {
            _logEvent.AppendKeyValue("myKey", "myValue");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"myValue\" }", _output.ToString());
        }

        [Test]
        public void should_append_multiple_key_values()
        {
            _logEvent.AppendKeyValue("myKey", "myValue");
            _logEvent.AppendKeyValue("otherKey", 2);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"myValue\", \"otherKey\": 2 }", _output.ToString());
        }

        [Test]
        public void should_append_formatted_string_mixed_with_key_values()
        {
            // TODO(lmanners): There are more edge cases here.
            _logEvent.AppendKeyValue("myKey", "myValue");
            _logEvent.AppendFormat("Some {} message");
            _logEvent.Append("formatted");
            _logEvent.AppendKeyValue("otherKey", 2);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Some formatted message ~~ { \"myKey\": \"myValue\", \"otherKey\": 2 }", _output.ToString());
        }

        [Test]
        public void should_be_chainable()
        {
            _logEvent.AppendKeyValue("myKey", 1.1f).AppendKeyValue("otherKey", new Guid());
            _logEvent.Append("message");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("message ~~ { \"myKey\": 1.1, \"otherKey\": \"00000000-0000-0000-0000-000000000000\" }", _output.ToString());
        }

        [TestCase]
        public void should_support_char()
        {
            _logEvent.AppendKeyValue("key1", 'a');
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key1\": \"a\" }", _output.ToString());
        }

        [TestCase]
        public void should_support_datetime()
        {
            _logEvent.AppendKeyValue("key1", DateTime.MinValue);
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key1\": \"0001-01-01 00:00:00.000\" }", _output.ToString());
        }

        [TestCase]
        public void should_support_boolean()
        {
            _logEvent.AppendKeyValue("key1", true).AppendKeyValue("key2", false);
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key1\": \"true\", \"key2\": \"false\" }", _output.ToString());
        }

        [TestCase]
        public void should_support_single_null_key_value()
        {
            _logEvent.AppendKeyValue("key1", (int?)null);
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key1\": \"null\" }", _output.ToString());
        }

        [TestCase]
        public void should_support_null_string_key_value()
        {
            _logEvent.AppendKeyValue("key1", "val1")
                     .AppendKeyValue("key2", (string)null)
                     .AppendKeyValue("key3", 3);
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key1\": \"val1\", \"key2\": \"null\", \"key3\": 3 }", _output.ToString());
        }

        [TestCase]
        public void should_support_number_types()
        {
            _logEvent.AppendKeyValue("byte", (byte)1)
                     .AppendKeyValue("short", (short)2)
                     .AppendKeyValue("int", 3)
                     .AppendKeyValue("long", 4L)
                     .AppendKeyValue("float", 5.5f)
                     .AppendKeyValue("double", 6.6d)
                     .AppendKeyValue("decimal", 6.6m);

            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"byte\": 1, \"short\": 2, \"int\": 3, \"long\": 4, \"float\": 5.5, \"double\": 6.6, \"decimal\": 6.6 }",
                            _output.ToString());
        }

        [TestCase]
        public void should_handle_truncated_key_values()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.TruncateMessage);
            for (var i = 0; i < 6; i++)
            {
                _logEvent.AppendKeyValue($"key{i}", $"value{i}");
            }

            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(
                " ~~ { \"key0\": \"value0\", \"key1\": \"value1\", \"key2\": \"value2\", \"key3\": \"value3\", \"key4\": \"value4\" } [TRUNCATED]",
                _output.ToString());
        }

        [TestCase]
        public void should_allocate_space_for_more_key_values()
        {
            for (var i = 0; i < 6; i++)
            {
                _logEvent.AppendKeyValue($"key{i}", $"value{i}");
            }

            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(
                " ~~ { \"key0\": \"value0\", \"key1\": \"value1\", \"key2\": \"value2\", \"key3\": \"value3\", \"key4\": \"value4\", \"key5\": \"value5\" }",
                _output.ToString());
        }

        [TestCase]
        public void should_handle_partially_truncated_key_value()
        {
            _logEvent.Initialize(Level.Info, null, LogEventArgumentExhaustionStrategy.TruncateMessage);

            // This value 'consumes' one slot in the buffer. This will cause us to be out of space when appending the value for key4.
            _logEvent.Append("msg");

            for (var i = 0; i < 5; i++)
            {
                _logEvent.AppendKeyValue($"key{i}", $"value{i}");
            }

            _logEvent.WriteToStringBuffer(_output);

            // 'key4' is not present because there wasn't space for its value.
            Assert.AreEqual(
                "msg ~~ { \"key0\": \"value0\", \"key1\": \"value1\", \"key2\": \"value2\", \"key3\": \"value3\" } [TRUNCATED]",
                _output.ToString());
        }
    }
}
