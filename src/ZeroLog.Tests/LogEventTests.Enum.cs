using System.Globalization;
using NCrunch.Framework;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    public partial class LogEventTests
    {
        [Test]
        public void should_append_enum()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendEnum(TestEnum.Bar);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Bar", _output.ToString());
        }

        [Test]
        public void should_append_nullable_enum()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendEnum((TestEnum?)TestEnum.Bar);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Bar", _output.ToString());
        }

        [Test]
        public void should_append_null_enum()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendEnum((TestEnum?)null);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        public void should_append_enum_key_value()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendKeyValue("myKey", TestEnum.Bar);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"Bar\" }", _output.ToString());
        }

        [Test]
        public void should_append_nullable_enum_key_value()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendKeyValue("myKey", (TestEnum?)TestEnum.Bar);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": \"Bar\" }", _output.ToString());
        }

        [Test]
        public void should_append_null_enum_key_value()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendKeyValue("myKey", (TestEnum?)null);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": null }", _output.ToString());
        }

        [Test]
        public void should_append_unknown_enum_key_value()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendKeyValue("myKey", (TestEnum)(-42));
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(" ~~ { \"myKey\": -42 }", _output.ToString());
        }

        [Test]
        public void should_append_enum_generic()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendGeneric(TestEnum.Baz);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Baz", _output.ToString());
        }

        [Test]
        public void should_append_nullable_enum_generic()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendGeneric((TestEnum?)TestEnum.Baz);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("Baz", _output.ToString());
        }

        [Test]
        public void should_append_null_enum_generic()
        {
            LogManager.RegisterEnum(typeof(TestEnum));

            _logEvent.AppendGeneric((TestEnum?)null);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("null", _output.ToString());
        }

        [Test]
        [ExclusivelyUses("EnumRegistration")]
        public void should_append_unregistered_enum()
        {
            _logEvent.AppendEnum(UnregisteredEnum.Bar);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("1", _output.ToString());
        }

        [Test]
        [ExclusivelyUses("EnumRegistration")]
        public void should_append_unregistered_enum_negative()
        {
            _logEvent.AppendEnum(UnregisteredEnum.Neg);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual("-1", _output.ToString());
        }

        [Test]
        [ExclusivelyUses("EnumRegistration")]
        public void should_append_unregistered_enum_large()
        {
            _logEvent.AppendEnum(UnregisteredEnumLarge.LargeValue);
            _logEvent.WriteToStringBuffer(_output);

            Assert.AreEqual(((ulong)UnregisteredEnumLarge.LargeValue).ToString(CultureInfo.InvariantCulture), _output.ToString());
        }

        [Test]
        [ExclusivelyUses("EnumRegistration")]
        public void should_auto_register_enum()
        {
            try
            {
                LogManager.Config.LazyRegisterEnums = true;

                _logEvent.AppendEnum(AutoRegisterEnum.Bar);
                _logEvent.WriteToStringBuffer(_output);

                Assert.AreEqual("Bar", _output.ToString());
            }
            finally
            {
                LogManager.Config.LazyRegisterEnums = false;
            }
        }

        private enum TestEnum
        {
            Foo,
            Bar,
            Baz
        }

        private enum UnregisteredEnum
        {
            Foo,
            Bar,
            Baz,
            Neg = -1
        }

        private enum UnregisteredEnumLarge : ulong
        {
            LargeValue = long.MaxValue + 42UL
        }

        private enum AutoRegisterEnum
        {
            Foo,
            Bar,
            Baz
        }
    }
}
