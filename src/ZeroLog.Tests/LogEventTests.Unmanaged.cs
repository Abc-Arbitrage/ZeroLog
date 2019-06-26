using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Formatting;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public partial class LogEventTests
    {
        public struct UnmanagedStruct : IStringFormattable
        {
            public long A;
            public int B;
            public byte C;

            public void Format(StringBuffer buffer, StringView format)
            {
                buffer.Append(A, StringView.Empty);
                buffer.Append("-");
                buffer.Append(B, StringView.Empty);
                buffer.Append("-");
                buffer.Append(C, StringView.Empty);
            }
        }

        [Test]
        public void should_append_unmanaged()
        {
            var o = new UnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output); 

            Assert.AreEqual("1-2-3", _output.ToString());
        }

        [Test]
        public void should_append_unmanaged_byref()
        {
            var o = new UnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1-2-3", _output.ToString());
        }

        [Test]
        public void should_append_nullable_unmanaged()
        {
            UnmanagedStruct? o = new UnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1-2-3", _output.ToString());
        }

        [Test]
        public void should_append_null_nullable_unmanaged()
        {
            UnmanagedStruct? o = null;

            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_nullable_unmanaged_byref()
        {
            UnmanagedStruct? o = new UnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1-2-3", _output.ToString());
        }

        [Test]
        public void should_append_null_nullable_unmanaged_byref()
        {
            UnmanagedStruct? o = null;

            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        public struct UnmanagedStruct2 : IStringFormattable
        {
            public long A;
            public int B;
            public byte C;

            public void Format(StringBuffer buffer, StringView format)
            {
                buffer.Append(A, StringView.Empty);
                buffer.Append("-");
                buffer.Append(B, StringView.Empty);
                buffer.Append("-");
                buffer.Append(C, StringView.Empty);
            }
        }

        [Test]
        public void should_append_unmanaged_2()
        {
            var o = new UnmanagedStruct2
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged(typeof(UnmanagedStruct2));

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1-2-3", _output.ToString());
        }

        public struct ExternalUnmanagedStruct
        {
            public long A;
            public int B;
            public byte C;
        }

        public static void ExternalUnmanagedStructFormatter(ref ExternalUnmanagedStruct self, StringBuffer buffer, StringView format)
        {
            buffer.Append("External(");
            buffer.Append(self.A, StringView.Empty);
            buffer.Append("-");
            buffer.Append(self.B, StringView.Empty);
            buffer.Append("-");
            buffer.Append(self.C, StringView.Empty);
            buffer.Append(")");
        }

        [Test]
        public void should_append_external_unmanaged()
        {
            var o = new ExternalUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<ExternalUnmanagedStruct>(ExternalUnmanagedStructFormatter);

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("External(1-2-3)", _output.ToString());
        }

        [Test]
        public void should_append_external_unmanaged_byref()
        {
            var o = new ExternalUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<ExternalUnmanagedStruct>(ExternalUnmanagedStructFormatter);

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("External(1-2-3)", _output.ToString());
        }

        [Test]
        public void should_append_nullable_external_unmanaged()
        {
            ExternalUnmanagedStruct? o = new ExternalUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<ExternalUnmanagedStruct>(ExternalUnmanagedStructFormatter);

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("External(1-2-3)", _output.ToString());
        }

        [Test]
        public void should_append_null_nullable_external_unmanaged()
        {
            ExternalUnmanagedStruct? o = null;

            LogManager.RegisterUnmanaged<ExternalUnmanagedStruct>(ExternalUnmanagedStructFormatter);

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_nullable_external_unmanaged_byref()
        {
            ExternalUnmanagedStruct? o = new ExternalUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<ExternalUnmanagedStruct>(ExternalUnmanagedStructFormatter);

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("External(1-2-3)", _output.ToString());
        }

        [Test]
        public void should_append_null_nullable_external_unmanaged_byref()
        {
            ExternalUnmanagedStruct? o = null;

            LogManager.RegisterUnmanaged<ExternalUnmanagedStruct>(ExternalUnmanagedStructFormatter);

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        public struct UnregisteredUnmanagedStruct
        {
            public long A;
            public int B;
            public byte C;
        }

        [Test]
        public void should_append_unregistered_unmanaged()
        {
            var o = new UnregisteredUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Unmanaged(0x01000000000000000200000003000000)", _output.ToString());
        }

        [Test]
        public void should_append_unregistered_unmanaged_byref()
        {
            var o = new UnregisteredUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Unmanaged(0x01000000000000000200000003000000)", _output.ToString());
        }

        [Test]
        public void should_append_unregistered_nullable_unmanaged()
        {
            UnregisteredUnmanagedStruct? o = new UnregisteredUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Unmanaged(0x01000000000000000200000003000000)", _output.ToString());
        }

        [Test]
        public void should_append_null_unregistered_nullable_unmanaged()
        {
            UnregisteredUnmanagedStruct? o = null;

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_unregistered_nullable_unmanaged_byref()
        {
            UnregisteredUnmanagedStruct? o = new UnregisteredUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Unmanaged(0x01000000000000000200000003000000)", _output.ToString());
        }

        [Test]
        public void should_append_null_unregistered_nullable_unmanaged_byref()
        {
            UnregisteredUnmanagedStruct? o = null;

            _logEvent.AppendUnmanaged(ref o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        public struct UnmanagedStructWithFormatSupport : IStringFormattable
        {
            public long A;

            public void Format(StringBuffer buffer, StringView format)
            {
                buffer.Append(A, StringView.Empty);

                if (!format.IsEmpty)
                {
                    buffer.Append("[");
                    buffer.Append(format.ToString());
                    buffer.Append("]");
                }
            }
        }

        [Test]
        public void should_append_unmanaged_with_format()
        {
            var o = new UnmanagedStructWithFormatSupport
            {
                A = 42
            };

            LogManager.RegisterUnmanaged<UnmanagedStructWithFormatSupport>();

            _logEvent.AppendUnmanaged(o, "foo");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("42[foo]", _output.ToString());
        }

        [Test]
        public void should_append_unmanaged_byref_with_format()
        {
            var o = new UnmanagedStructWithFormatSupport
            {
                A = 42
            };

            LogManager.RegisterUnmanaged<UnmanagedStructWithFormatSupport>();

            _logEvent.AppendUnmanaged(ref o, "foo");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("42[foo]", _output.ToString());
        }

        [Test]
        public void should_append_nullable_unmanaged_with_format()
        {
            UnmanagedStructWithFormatSupport? o = new UnmanagedStructWithFormatSupport
            {
                A = 42
            };

            LogManager.RegisterUnmanaged<UnmanagedStructWithFormatSupport>();

            _logEvent.AppendUnmanaged(o, "foo");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("42[foo]", _output.ToString());
        }

        [Test]
        public void should_append_null_unmanaged_with_format()
        {
            UnmanagedStructWithFormatSupport? o = null;

            LogManager.RegisterUnmanaged<UnmanagedStructWithFormatSupport>();

            _logEvent.AppendUnmanaged(o, "foo");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_nullable_unmanaged_byref_with_format()
        {
            UnmanagedStructWithFormatSupport? o = new UnmanagedStructWithFormatSupport
            {
                A = 42
            };

            LogManager.RegisterUnmanaged<UnmanagedStructWithFormatSupport>();

            _logEvent.AppendUnmanaged(ref o, "foo");
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("42[foo]", _output.ToString());
        }

        public struct FailingUnmanagedStruct : IStringFormattable
        {
            public long A;
            public int B;
            public byte C;

            public void Format(StringBuffer buffer, StringView format)
            {
                buffer.Append("boom");
                throw new InvalidOperationException("Simulated failure");
            }
        }

        [Test]
        public void should_append_failing_unmanaged_as_unformatted()
        {
            var o = new FailingUnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3,
            };

            LogManager.RegisterUnmanaged<FailingUnmanagedStruct>();

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBufferUnformatted(_output);

            Assert.AreEqual("Unmanaged(0x01000000000000000200000003000000)", _output.ToString());
        }
    }
}
