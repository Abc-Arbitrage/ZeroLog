using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog.Tests
{
    public partial class LogEventTests
    {
        public struct UnmanagedStruct : IStringFormattable
        {
            public long A;
            public int B;
            public byte C;

            public void Format(StringBuffer buffer, StringView format)
            {
                buffer.Append(this.A, StringView.Empty);
                buffer.Append("-");
                buffer.Append(this.B, StringView.Empty);
                buffer.Append("-");
                buffer.Append(this.C, StringView.Empty);
            }
        }

        [Test]
        public void should_append_unmanaged()
        {
            var o = new UnmanagedStruct()
            {
                A = 1,
                B = 2,
                C = 3,
            };

            UnmanagedCache.Register<UnmanagedStruct>();

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
            var o = new ExternalUnmanagedStruct()
            {
                A = 1,
                B = 2,
                C = 3,
            };

            UnmanagedCache.Register<ExternalUnmanagedStruct>(ExternalUnmanagedStructFormatter);

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("External(1-2-3)", _output.ToString());
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
            var o = new UnregisteredUnmanagedStruct()
            {
                A = 1,
                B = 2,
                C = 3,
            };

            _logEvent.AppendUnmanaged(o);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Unmanaged(0x01000000000000000200000003000000)", _output.ToString());
        }
    }
}
