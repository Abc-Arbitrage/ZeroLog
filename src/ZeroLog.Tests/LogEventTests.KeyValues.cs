using System;
using System.Text;
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
            _logEvent.AppendKeyValue("myKey", "myValue");
            _logEvent.AppendFormat("Some {} message");
            _logEvent.Append("formatted");
            _logEvent.AppendKeyValue("otherKey", 2);
            _logEvent.Append("...");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Some formatted message... ~~ { \"myKey\": \"myValue\", \"otherKey\": 2 }", _output.ToString());
        }

        [Test]
        public void should_append_key_byte_array_value()
        {
            var bytes = Encoding.ASCII.GetBytes("myValue");
            _logEvent.AppendKeyValueAscii("myKey", bytes, bytes.Length);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"myValue\" }", _output.ToString());
        }

        [Test]
        public void should_append_key_byte_span_value()
        {
            var bytes = Encoding.ASCII.GetBytes("myValue");
            _logEvent.AppendKeyValueAscii("myKey", bytes.AsSpan());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"myValue\" }", _output.ToString());
        }

        [Test]
        public void should_append_key_char_span_value()
        {
            _logEvent.AppendKeyValueAscii("myKey", "myValue".AsSpan());
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"myValue\" }", _output.ToString());
        }

        [Test]
        public void should_ignore_byte_array_value_with_negative_length()
        {
            var bytes = Encoding.Default.GetBytes("myValue");
            _logEvent.AppendKeyValueAscii("myKey", bytes, -1);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("", _output.ToString());
        }

        [Test]
        public void should_support_byte_array_value_that_is_empty()
        {
            _logEvent.AppendKeyValueAscii("myKey", new byte[0], 0);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"\" }", _output.ToString());
        }

        [Test]
        public void should_support_null_byte_array()
        {
            _logEvent.AppendKeyValueAscii("myKey", (byte[])null, 0);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": null }", _output.ToString());
        }

        [Test]
        public void should_support_empty_byte_pointer()
        {
            var bytes = new byte[1];
            unsafe
            {
                fixed (byte* pBytes = bytes)
                {
                    _logEvent.AppendKeyValueAscii("myKey", pBytes, 0);
                }
            }

            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"\" }", _output.ToString());
        }

        [Test]
        public unsafe void should_support_null_byte_pointer()
        {
            _logEvent.AppendKeyValueAscii("myKey", (byte*)null, 0);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": null }", _output.ToString());
        }

        [Test]
        public void should_support_byte_span_value_that_is_empty()
        {
            _logEvent.AppendKeyValueAscii("myKey", ReadOnlySpan<byte>.Empty);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"\" }", _output.ToString());
        }

        [Test]
        public void should_support_char_span_value_that_is_empty()
        {
            _logEvent.AppendKeyValueAscii("myKey", ReadOnlySpan<char>.Empty);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"\" }", _output.ToString());
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
            Assert.AreEqual(" ~~ { \"key1\": true, \"key2\": false }", _output.ToString());
        }

        [TestCase]
        public void should_support_single_null_key_value()
        {
            _logEvent.AppendKeyValue("key1", (int?)null);
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key1\": null }", _output.ToString());
        }

        [TestCase]
        public void should_support_null_string_key_value()
        {
            _logEvent.AppendKeyValue("key1", "val1")
                     .AppendKeyValue("key2", (string)null)
                     .AppendKeyValue("key3", 3);
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key1\": \"val1\", \"key2\": null, \"key3\": 3 }", _output.ToString());
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

        [TestCase('\\', "\\\\")]
        [TestCase('"', "\\\"")]
        [TestCase('\u0000', "\\u0000")]
        [TestCase('\u0000', "\\u0000")]
        [TestCase('\u0001', "\\u0001")]
        [TestCase('\u0002', "\\u0002")]
        [TestCase('\u0003', "\\u0003")]
        [TestCase('\u0004', "\\u0004")]
        [TestCase('\u0005', "\\u0005")]
        [TestCase('\u0006', "\\u0006")]
        [TestCase('\u0007', "\\u0007")]
        [TestCase('\u0008', "\\b")]
        [TestCase('\u0009', "\\t")]
        [TestCase('\u000A', "\\n")]
        [TestCase('\u000B', "\\u000b")]
        [TestCase('\u000C', "\\f")]
        [TestCase('\u000D', "\\r")]
        [TestCase('\u000E', "\\u000e")]
        [TestCase('\u000F', "\\u000f")]
        [TestCase('\u0010', "\\u0010")]
        [TestCase('\u0011', "\\u0011")]
        [TestCase('\u0012', "\\u0012")]
        [TestCase('\u0013', "\\u0013")]
        [TestCase('\u0014', "\\u0014")]
        [TestCase('\u0015', "\\u0015")]
        [TestCase('\u0016', "\\u0016")]
        [TestCase('\u0017', "\\u0017")]
        [TestCase('\u0018', "\\u0018")]
        [TestCase('\u0019', "\\u0019")]
        [TestCase('\u001A', "\\u001a")]
        [TestCase('\u001B', "\\u001b")]
        [TestCase('\u001C', "\\u001c")]
        [TestCase('\u001D', "\\u001d")]
        [TestCase('\u001E', "\\u001e")]
        [TestCase('\u001F', "\\u001f")]
        public void should_handle_escaped_characters(char character, string expected)
        {
            _logEvent.AppendKeyValue("myKey", character);
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"myKey\": \"" + expected + "\" }", _output.ToString());
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

        [TestCase]
        public void should_escape_strings_for_json()
        {
            _logEvent.AppendKeyValue("key \\ \" \t \n", "Hello \u0001 \0 there");
            _logEvent.WriteToStringBuffer(_output);
            Assert.AreEqual(" ~~ { \"key \\\\ \\\" \\t \\n\": \"Hello \\u0001 \\u0000 there\" }", _output.ToString());
        }
    }
}
