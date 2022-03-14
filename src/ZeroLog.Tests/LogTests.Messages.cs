
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[SuppressMessage("ReSharper", "RedundantCast")]
partial class LogTests
{
    [Test]
    public void should_not_log_above_level_Trace()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Trace + 1));
        _log.Trace("Foo");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_not_log_above_level_Trace_interpolated()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Trace + 1));
        _log.Trace($"Foo {42}");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_log_with_append_Trace()
    {
        _log.Trace().Append("Foo").Log();

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_String_Trace()
    {
        _log.Trace("foo");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual("foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_Exception_Trace()
    {
        var exception = new InvalidOperationException();
        _log.Trace("Foo", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_log_interpolated_String_Trace()
    {
        _log.Trace($"foo {NoInline("bar")} baz {NoInline("foobar")}");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual("foo bar baz foobar");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_interpolated_Exception_Trace()
    {
        var exception = new InvalidOperationException();
        _log.Trace($"Foo {42}", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo 42");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_not_allocate_String_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace("Foo"));
    }

    [Test]
    public void should_not_allocate_interpolated_String_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {NoInline("bar")} baz {NoInline("foobar")}"));
    }

    [Test]
    public void should_log_Boolean_Trace()
    {
        _log.Trace($"foo {TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Boolean_Trace_nullable()
    {
        _log.Trace($"foo {(bool?)TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_not_allocate_Boolean_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Byte_Trace()
    {
        _log.Trace($"foo {TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Trace_nullable()
    {
        _log.Trace($"foo {(byte?)TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Byte_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(byte?)TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Byte_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_SByte_Trace()
    {
        _log.Trace($"foo {TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Trace_nullable()
    {
        _log.Trace($"foo {(sbyte?)TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_SByte_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(sbyte?)TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_SByte_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_Char_Trace()
    {
        _log.Trace($"foo {TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Char_Trace_nullable()
    {
        _log.Trace($"foo {(char?)TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_not_allocate_Char_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Int16_Trace()
    {
        _log.Trace($"foo {TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Trace_nullable()
    {
        _log.Trace($"foo {(short?)TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int16_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(short?)TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int16_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_UInt16_Trace()
    {
        _log.Trace($"foo {TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Trace_nullable()
    {
        _log.Trace($"foo {(ushort?)TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt16_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(ushort?)TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt16_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_Int32_Trace()
    {
        _log.Trace($"foo {TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Trace_nullable()
    {
        _log.Trace($"foo {(int?)TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int32_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(int?)TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int32_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_UInt32_Trace()
    {
        _log.Trace($"foo {TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Trace_nullable()
    {
        _log.Trace($"foo {(uint?)TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt32_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(uint?)TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt32_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_Int64_Trace()
    {
        _log.Trace($"foo {TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Trace_nullable()
    {
        _log.Trace($"foo {(long?)TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int64_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(long?)TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int64_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_UInt64_Trace()
    {
        _log.Trace($"foo {TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Trace_nullable()
    {
        _log.Trace($"foo {(ulong?)TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt64_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(ulong?)TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt64_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_IntPtr_Trace()
    {
        _log.Trace($"foo {TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Trace_nullable()
    {
        _log.Trace($"foo {(nint?)TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_IntPtr_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(nint?)TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_IntPtr_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Trace()
    {
        _log.Trace($"foo {TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Trace_nullable()
    {
        _log.Trace($"foo {(nuint?)TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UIntPtr_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(nuint?)TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UIntPtr_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_Single_Trace()
    {
        _log.Trace($"foo {TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Trace_nullable()
    {
        _log.Trace($"foo {(float?)TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Single_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(float?)TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Single_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Double_Trace()
    {
        _log.Trace($"foo {TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Trace_nullable()
    {
        _log.Trace($"foo {(double?)TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Double_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(double?)TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Double_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Decimal_Trace()
    {
        _log.Trace($"foo {TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Trace_nullable()
    {
        _log.Trace($"foo {(decimal?)TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Decimal_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(decimal?)TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Decimal_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Guid_Trace()
    {
        _log.Trace($"foo {TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Trace_nullable()
    {
        _log.Trace($"foo {(Guid?)TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Guid_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(Guid?)TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Guid_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_DateTime_Trace()
    {
        _log.Trace($"foo {TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Trace_nullable()
    {
        _log.Trace($"foo {(DateTime?)TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_DateTime_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(DateTime?)TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_DateTime_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Trace()
    {
        _log.Trace($"foo {TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Trace_nullable()
    {
        _log.Trace($"foo {(TimeSpan?)TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Trace_formatted()
    {
        _log.Trace($"foo {TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_TimeSpan_Trace_nullable_formatted()
    {
        _log.Trace($"foo {(TimeSpan?)TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_TimeSpan_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_Enum_Trace()
    {
        _log.Trace($"foo {DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_log_Enum_Trace_nullable()
    {
        _log.Trace($"foo {(DayOfWeek?)DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_not_allocate_Enum_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {DayOfWeek.Friday} bar"));
    }

    [Test]
    public void should_log_indirect_Trace()
    {
        _log.Trace($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Trace);
        message.ToString().ShouldEqual("foo 42 bar");
    }

    [Test]
    public void should_not_allocate_indirect_Trace()
    {
        GcTester.ShouldNotAllocate(() => _log.Trace($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar"));
    }

    [Test]
    public void should_not_log_above_level_Debug()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Debug + 1));
        _log.Debug("Foo");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_not_log_above_level_Debug_interpolated()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Debug + 1));
        _log.Debug($"Foo {42}");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_log_with_append_Debug()
    {
        _log.Debug().Append("Foo").Log();

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_String_Debug()
    {
        _log.Debug("foo");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual("foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_Exception_Debug()
    {
        var exception = new InvalidOperationException();
        _log.Debug("Foo", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_log_interpolated_String_Debug()
    {
        _log.Debug($"foo {NoInline("bar")} baz {NoInline("foobar")}");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual("foo bar baz foobar");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_interpolated_Exception_Debug()
    {
        var exception = new InvalidOperationException();
        _log.Debug($"Foo {42}", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo 42");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_not_allocate_String_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug("Foo"));
    }

    [Test]
    public void should_not_allocate_interpolated_String_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {NoInline("bar")} baz {NoInline("foobar")}"));
    }

    [Test]
    public void should_log_Boolean_Debug()
    {
        _log.Debug($"foo {TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Boolean_Debug_nullable()
    {
        _log.Debug($"foo {(bool?)TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_not_allocate_Boolean_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Byte_Debug()
    {
        _log.Debug($"foo {TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Debug_nullable()
    {
        _log.Debug($"foo {(byte?)TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Byte_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(byte?)TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Byte_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_SByte_Debug()
    {
        _log.Debug($"foo {TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Debug_nullable()
    {
        _log.Debug($"foo {(sbyte?)TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_SByte_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(sbyte?)TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_SByte_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_Char_Debug()
    {
        _log.Debug($"foo {TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Char_Debug_nullable()
    {
        _log.Debug($"foo {(char?)TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_not_allocate_Char_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Int16_Debug()
    {
        _log.Debug($"foo {TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Debug_nullable()
    {
        _log.Debug($"foo {(short?)TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int16_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(short?)TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int16_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_UInt16_Debug()
    {
        _log.Debug($"foo {TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Debug_nullable()
    {
        _log.Debug($"foo {(ushort?)TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt16_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(ushort?)TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt16_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_Int32_Debug()
    {
        _log.Debug($"foo {TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Debug_nullable()
    {
        _log.Debug($"foo {(int?)TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int32_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(int?)TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int32_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_UInt32_Debug()
    {
        _log.Debug($"foo {TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Debug_nullable()
    {
        _log.Debug($"foo {(uint?)TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt32_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(uint?)TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt32_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_Int64_Debug()
    {
        _log.Debug($"foo {TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Debug_nullable()
    {
        _log.Debug($"foo {(long?)TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int64_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(long?)TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int64_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_UInt64_Debug()
    {
        _log.Debug($"foo {TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Debug_nullable()
    {
        _log.Debug($"foo {(ulong?)TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt64_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(ulong?)TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt64_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_IntPtr_Debug()
    {
        _log.Debug($"foo {TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Debug_nullable()
    {
        _log.Debug($"foo {(nint?)TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_IntPtr_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(nint?)TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_IntPtr_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Debug()
    {
        _log.Debug($"foo {TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Debug_nullable()
    {
        _log.Debug($"foo {(nuint?)TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UIntPtr_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(nuint?)TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UIntPtr_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_Single_Debug()
    {
        _log.Debug($"foo {TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Debug_nullable()
    {
        _log.Debug($"foo {(float?)TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Single_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(float?)TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Single_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Double_Debug()
    {
        _log.Debug($"foo {TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Debug_nullable()
    {
        _log.Debug($"foo {(double?)TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Double_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(double?)TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Double_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Decimal_Debug()
    {
        _log.Debug($"foo {TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Debug_nullable()
    {
        _log.Debug($"foo {(decimal?)TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Decimal_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(decimal?)TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Decimal_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Guid_Debug()
    {
        _log.Debug($"foo {TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Debug_nullable()
    {
        _log.Debug($"foo {(Guid?)TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Guid_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(Guid?)TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Guid_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_DateTime_Debug()
    {
        _log.Debug($"foo {TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Debug_nullable()
    {
        _log.Debug($"foo {(DateTime?)TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_DateTime_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(DateTime?)TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_DateTime_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Debug()
    {
        _log.Debug($"foo {TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Debug_nullable()
    {
        _log.Debug($"foo {(TimeSpan?)TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Debug_formatted()
    {
        _log.Debug($"foo {TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_TimeSpan_Debug_nullable_formatted()
    {
        _log.Debug($"foo {(TimeSpan?)TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_TimeSpan_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_Enum_Debug()
    {
        _log.Debug($"foo {DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_log_Enum_Debug_nullable()
    {
        _log.Debug($"foo {(DayOfWeek?)DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_not_allocate_Enum_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {DayOfWeek.Friday} bar"));
    }

    [Test]
    public void should_log_indirect_Debug()
    {
        _log.Debug($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Debug);
        message.ToString().ShouldEqual("foo 42 bar");
    }

    [Test]
    public void should_not_allocate_indirect_Debug()
    {
        GcTester.ShouldNotAllocate(() => _log.Debug($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar"));
    }

    [Test]
    public void should_not_log_above_level_Info()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Info + 1));
        _log.Info("Foo");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_not_log_above_level_Info_interpolated()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Info + 1));
        _log.Info($"Foo {42}");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_log_with_append_Info()
    {
        _log.Info().Append("Foo").Log();

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_String_Info()
    {
        _log.Info("foo");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual("foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_Exception_Info()
    {
        var exception = new InvalidOperationException();
        _log.Info("Foo", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_log_interpolated_String_Info()
    {
        _log.Info($"foo {NoInline("bar")} baz {NoInline("foobar")}");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual("foo bar baz foobar");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_interpolated_Exception_Info()
    {
        var exception = new InvalidOperationException();
        _log.Info($"Foo {42}", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo 42");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_not_allocate_String_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info("Foo"));
    }

    [Test]
    public void should_not_allocate_interpolated_String_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {NoInline("bar")} baz {NoInline("foobar")}"));
    }

    [Test]
    public void should_log_Boolean_Info()
    {
        _log.Info($"foo {TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Boolean_Info_nullable()
    {
        _log.Info($"foo {(bool?)TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_not_allocate_Boolean_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Byte_Info()
    {
        _log.Info($"foo {TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Info_nullable()
    {
        _log.Info($"foo {(byte?)TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Info_formatted()
    {
        _log.Info($"foo {TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Byte_Info_nullable_formatted()
    {
        _log.Info($"foo {(byte?)TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Byte_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_SByte_Info()
    {
        _log.Info($"foo {TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Info_nullable()
    {
        _log.Info($"foo {(sbyte?)TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Info_formatted()
    {
        _log.Info($"foo {TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_SByte_Info_nullable_formatted()
    {
        _log.Info($"foo {(sbyte?)TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_SByte_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_Char_Info()
    {
        _log.Info($"foo {TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Char_Info_nullable()
    {
        _log.Info($"foo {(char?)TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_not_allocate_Char_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Int16_Info()
    {
        _log.Info($"foo {TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Info_nullable()
    {
        _log.Info($"foo {(short?)TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Info_formatted()
    {
        _log.Info($"foo {TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int16_Info_nullable_formatted()
    {
        _log.Info($"foo {(short?)TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int16_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_UInt16_Info()
    {
        _log.Info($"foo {TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Info_nullable()
    {
        _log.Info($"foo {(ushort?)TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Info_formatted()
    {
        _log.Info($"foo {TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt16_Info_nullable_formatted()
    {
        _log.Info($"foo {(ushort?)TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt16_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_Int32_Info()
    {
        _log.Info($"foo {TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Info_nullable()
    {
        _log.Info($"foo {(int?)TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Info_formatted()
    {
        _log.Info($"foo {TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int32_Info_nullable_formatted()
    {
        _log.Info($"foo {(int?)TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int32_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_UInt32_Info()
    {
        _log.Info($"foo {TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Info_nullable()
    {
        _log.Info($"foo {(uint?)TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Info_formatted()
    {
        _log.Info($"foo {TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt32_Info_nullable_formatted()
    {
        _log.Info($"foo {(uint?)TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt32_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_Int64_Info()
    {
        _log.Info($"foo {TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Info_nullable()
    {
        _log.Info($"foo {(long?)TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Info_formatted()
    {
        _log.Info($"foo {TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int64_Info_nullable_formatted()
    {
        _log.Info($"foo {(long?)TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int64_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_UInt64_Info()
    {
        _log.Info($"foo {TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Info_nullable()
    {
        _log.Info($"foo {(ulong?)TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Info_formatted()
    {
        _log.Info($"foo {TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt64_Info_nullable_formatted()
    {
        _log.Info($"foo {(ulong?)TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt64_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_IntPtr_Info()
    {
        _log.Info($"foo {TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Info_nullable()
    {
        _log.Info($"foo {(nint?)TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Info_formatted()
    {
        _log.Info($"foo {TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_IntPtr_Info_nullable_formatted()
    {
        _log.Info($"foo {(nint?)TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_IntPtr_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Info()
    {
        _log.Info($"foo {TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Info_nullable()
    {
        _log.Info($"foo {(nuint?)TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Info_formatted()
    {
        _log.Info($"foo {TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UIntPtr_Info_nullable_formatted()
    {
        _log.Info($"foo {(nuint?)TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UIntPtr_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_Single_Info()
    {
        _log.Info($"foo {TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Info_nullable()
    {
        _log.Info($"foo {(float?)TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Info_formatted()
    {
        _log.Info($"foo {TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Single_Info_nullable_formatted()
    {
        _log.Info($"foo {(float?)TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Single_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Double_Info()
    {
        _log.Info($"foo {TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Info_nullable()
    {
        _log.Info($"foo {(double?)TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Info_formatted()
    {
        _log.Info($"foo {TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Double_Info_nullable_formatted()
    {
        _log.Info($"foo {(double?)TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Double_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Decimal_Info()
    {
        _log.Info($"foo {TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Info_nullable()
    {
        _log.Info($"foo {(decimal?)TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Info_formatted()
    {
        _log.Info($"foo {TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Decimal_Info_nullable_formatted()
    {
        _log.Info($"foo {(decimal?)TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Decimal_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Guid_Info()
    {
        _log.Info($"foo {TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Info_nullable()
    {
        _log.Info($"foo {(Guid?)TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Info_formatted()
    {
        _log.Info($"foo {TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Guid_Info_nullable_formatted()
    {
        _log.Info($"foo {(Guid?)TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Guid_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_DateTime_Info()
    {
        _log.Info($"foo {TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Info_nullable()
    {
        _log.Info($"foo {(DateTime?)TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Info_formatted()
    {
        _log.Info($"foo {TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_DateTime_Info_nullable_formatted()
    {
        _log.Info($"foo {(DateTime?)TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_DateTime_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Info()
    {
        _log.Info($"foo {TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Info_nullable()
    {
        _log.Info($"foo {(TimeSpan?)TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Info_formatted()
    {
        _log.Info($"foo {TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_TimeSpan_Info_nullable_formatted()
    {
        _log.Info($"foo {(TimeSpan?)TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_TimeSpan_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_Enum_Info()
    {
        _log.Info($"foo {DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_log_Enum_Info_nullable()
    {
        _log.Info($"foo {(DayOfWeek?)DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_not_allocate_Enum_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {DayOfWeek.Friday} bar"));
    }

    [Test]
    public void should_log_indirect_Info()
    {
        _log.Info($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Info);
        message.ToString().ShouldEqual("foo 42 bar");
    }

    [Test]
    public void should_not_allocate_indirect_Info()
    {
        GcTester.ShouldNotAllocate(() => _log.Info($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar"));
    }

    [Test]
    public void should_not_log_above_level_Warn()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Warn + 1));
        _log.Warn("Foo");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_not_log_above_level_Warn_interpolated()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Warn + 1));
        _log.Warn($"Foo {42}");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_log_with_append_Warn()
    {
        _log.Warn().Append("Foo").Log();

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_String_Warn()
    {
        _log.Warn("foo");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual("foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_Exception_Warn()
    {
        var exception = new InvalidOperationException();
        _log.Warn("Foo", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_log_interpolated_String_Warn()
    {
        _log.Warn($"foo {NoInline("bar")} baz {NoInline("foobar")}");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual("foo bar baz foobar");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_interpolated_Exception_Warn()
    {
        var exception = new InvalidOperationException();
        _log.Warn($"Foo {42}", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo 42");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_not_allocate_String_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn("Foo"));
    }

    [Test]
    public void should_not_allocate_interpolated_String_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {NoInline("bar")} baz {NoInline("foobar")}"));
    }

    [Test]
    public void should_log_Boolean_Warn()
    {
        _log.Warn($"foo {TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Boolean_Warn_nullable()
    {
        _log.Warn($"foo {(bool?)TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_not_allocate_Boolean_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Byte_Warn()
    {
        _log.Warn($"foo {TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Warn_nullable()
    {
        _log.Warn($"foo {(byte?)TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Byte_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(byte?)TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Byte_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_SByte_Warn()
    {
        _log.Warn($"foo {TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Warn_nullable()
    {
        _log.Warn($"foo {(sbyte?)TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_SByte_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(sbyte?)TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_SByte_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_Char_Warn()
    {
        _log.Warn($"foo {TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Char_Warn_nullable()
    {
        _log.Warn($"foo {(char?)TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_not_allocate_Char_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Int16_Warn()
    {
        _log.Warn($"foo {TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Warn_nullable()
    {
        _log.Warn($"foo {(short?)TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int16_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(short?)TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int16_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_UInt16_Warn()
    {
        _log.Warn($"foo {TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Warn_nullable()
    {
        _log.Warn($"foo {(ushort?)TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt16_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(ushort?)TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt16_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_Int32_Warn()
    {
        _log.Warn($"foo {TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Warn_nullable()
    {
        _log.Warn($"foo {(int?)TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int32_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(int?)TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int32_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_UInt32_Warn()
    {
        _log.Warn($"foo {TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Warn_nullable()
    {
        _log.Warn($"foo {(uint?)TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt32_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(uint?)TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt32_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_Int64_Warn()
    {
        _log.Warn($"foo {TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Warn_nullable()
    {
        _log.Warn($"foo {(long?)TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int64_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(long?)TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int64_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_UInt64_Warn()
    {
        _log.Warn($"foo {TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Warn_nullable()
    {
        _log.Warn($"foo {(ulong?)TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt64_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(ulong?)TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt64_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_IntPtr_Warn()
    {
        _log.Warn($"foo {TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Warn_nullable()
    {
        _log.Warn($"foo {(nint?)TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_IntPtr_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(nint?)TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_IntPtr_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Warn()
    {
        _log.Warn($"foo {TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Warn_nullable()
    {
        _log.Warn($"foo {(nuint?)TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UIntPtr_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(nuint?)TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UIntPtr_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_Single_Warn()
    {
        _log.Warn($"foo {TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Warn_nullable()
    {
        _log.Warn($"foo {(float?)TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Single_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(float?)TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Single_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Double_Warn()
    {
        _log.Warn($"foo {TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Warn_nullable()
    {
        _log.Warn($"foo {(double?)TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Double_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(double?)TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Double_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Decimal_Warn()
    {
        _log.Warn($"foo {TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Warn_nullable()
    {
        _log.Warn($"foo {(decimal?)TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Decimal_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(decimal?)TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Decimal_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Guid_Warn()
    {
        _log.Warn($"foo {TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Warn_nullable()
    {
        _log.Warn($"foo {(Guid?)TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Guid_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(Guid?)TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Guid_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_DateTime_Warn()
    {
        _log.Warn($"foo {TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Warn_nullable()
    {
        _log.Warn($"foo {(DateTime?)TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_DateTime_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(DateTime?)TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_DateTime_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Warn()
    {
        _log.Warn($"foo {TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Warn_nullable()
    {
        _log.Warn($"foo {(TimeSpan?)TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Warn_formatted()
    {
        _log.Warn($"foo {TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_TimeSpan_Warn_nullable_formatted()
    {
        _log.Warn($"foo {(TimeSpan?)TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_TimeSpan_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_Enum_Warn()
    {
        _log.Warn($"foo {DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_log_Enum_Warn_nullable()
    {
        _log.Warn($"foo {(DayOfWeek?)DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_not_allocate_Enum_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {DayOfWeek.Friday} bar"));
    }

    [Test]
    public void should_log_indirect_Warn()
    {
        _log.Warn($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Warn);
        message.ToString().ShouldEqual("foo 42 bar");
    }

    [Test]
    public void should_not_allocate_indirect_Warn()
    {
        GcTester.ShouldNotAllocate(() => _log.Warn($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar"));
    }

    [Test]
    public void should_not_log_above_level_Error()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Error + 1));
        _log.Error("Foo");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_not_log_above_level_Error_interpolated()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Error + 1));
        _log.Error($"Foo {42}");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_log_with_append_Error()
    {
        _log.Error().Append("Foo").Log();

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_String_Error()
    {
        _log.Error("foo");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual("foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_Exception_Error()
    {
        var exception = new InvalidOperationException();
        _log.Error("Foo", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_log_interpolated_String_Error()
    {
        _log.Error($"foo {NoInline("bar")} baz {NoInline("foobar")}");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual("foo bar baz foobar");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_interpolated_Exception_Error()
    {
        var exception = new InvalidOperationException();
        _log.Error($"Foo {42}", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo 42");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_not_allocate_String_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error("Foo"));
    }

    [Test]
    public void should_not_allocate_interpolated_String_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {NoInline("bar")} baz {NoInline("foobar")}"));
    }

    [Test]
    public void should_log_Boolean_Error()
    {
        _log.Error($"foo {TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Boolean_Error_nullable()
    {
        _log.Error($"foo {(bool?)TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_not_allocate_Boolean_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Byte_Error()
    {
        _log.Error($"foo {TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Error_nullable()
    {
        _log.Error($"foo {(byte?)TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Error_formatted()
    {
        _log.Error($"foo {TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Byte_Error_nullable_formatted()
    {
        _log.Error($"foo {(byte?)TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Byte_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_SByte_Error()
    {
        _log.Error($"foo {TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Error_nullable()
    {
        _log.Error($"foo {(sbyte?)TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Error_formatted()
    {
        _log.Error($"foo {TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_SByte_Error_nullable_formatted()
    {
        _log.Error($"foo {(sbyte?)TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_SByte_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_Char_Error()
    {
        _log.Error($"foo {TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Char_Error_nullable()
    {
        _log.Error($"foo {(char?)TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_not_allocate_Char_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Int16_Error()
    {
        _log.Error($"foo {TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Error_nullable()
    {
        _log.Error($"foo {(short?)TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Error_formatted()
    {
        _log.Error($"foo {TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int16_Error_nullable_formatted()
    {
        _log.Error($"foo {(short?)TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int16_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_UInt16_Error()
    {
        _log.Error($"foo {TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Error_nullable()
    {
        _log.Error($"foo {(ushort?)TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Error_formatted()
    {
        _log.Error($"foo {TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt16_Error_nullable_formatted()
    {
        _log.Error($"foo {(ushort?)TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt16_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_Int32_Error()
    {
        _log.Error($"foo {TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Error_nullable()
    {
        _log.Error($"foo {(int?)TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Error_formatted()
    {
        _log.Error($"foo {TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int32_Error_nullable_formatted()
    {
        _log.Error($"foo {(int?)TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int32_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_UInt32_Error()
    {
        _log.Error($"foo {TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Error_nullable()
    {
        _log.Error($"foo {(uint?)TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Error_formatted()
    {
        _log.Error($"foo {TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt32_Error_nullable_formatted()
    {
        _log.Error($"foo {(uint?)TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt32_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_Int64_Error()
    {
        _log.Error($"foo {TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Error_nullable()
    {
        _log.Error($"foo {(long?)TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Error_formatted()
    {
        _log.Error($"foo {TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int64_Error_nullable_formatted()
    {
        _log.Error($"foo {(long?)TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int64_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_UInt64_Error()
    {
        _log.Error($"foo {TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Error_nullable()
    {
        _log.Error($"foo {(ulong?)TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Error_formatted()
    {
        _log.Error($"foo {TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt64_Error_nullable_formatted()
    {
        _log.Error($"foo {(ulong?)TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt64_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_IntPtr_Error()
    {
        _log.Error($"foo {TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Error_nullable()
    {
        _log.Error($"foo {(nint?)TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Error_formatted()
    {
        _log.Error($"foo {TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_IntPtr_Error_nullable_formatted()
    {
        _log.Error($"foo {(nint?)TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_IntPtr_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Error()
    {
        _log.Error($"foo {TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Error_nullable()
    {
        _log.Error($"foo {(nuint?)TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Error_formatted()
    {
        _log.Error($"foo {TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UIntPtr_Error_nullable_formatted()
    {
        _log.Error($"foo {(nuint?)TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UIntPtr_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_Single_Error()
    {
        _log.Error($"foo {TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Error_nullable()
    {
        _log.Error($"foo {(float?)TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Error_formatted()
    {
        _log.Error($"foo {TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Single_Error_nullable_formatted()
    {
        _log.Error($"foo {(float?)TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Single_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Double_Error()
    {
        _log.Error($"foo {TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Error_nullable()
    {
        _log.Error($"foo {(double?)TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Error_formatted()
    {
        _log.Error($"foo {TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Double_Error_nullable_formatted()
    {
        _log.Error($"foo {(double?)TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Double_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Decimal_Error()
    {
        _log.Error($"foo {TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Error_nullable()
    {
        _log.Error($"foo {(decimal?)TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Error_formatted()
    {
        _log.Error($"foo {TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Decimal_Error_nullable_formatted()
    {
        _log.Error($"foo {(decimal?)TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Decimal_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Guid_Error()
    {
        _log.Error($"foo {TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Error_nullable()
    {
        _log.Error($"foo {(Guid?)TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Error_formatted()
    {
        _log.Error($"foo {TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Guid_Error_nullable_formatted()
    {
        _log.Error($"foo {(Guid?)TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Guid_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_DateTime_Error()
    {
        _log.Error($"foo {TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Error_nullable()
    {
        _log.Error($"foo {(DateTime?)TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Error_formatted()
    {
        _log.Error($"foo {TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_DateTime_Error_nullable_formatted()
    {
        _log.Error($"foo {(DateTime?)TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_DateTime_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Error()
    {
        _log.Error($"foo {TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Error_nullable()
    {
        _log.Error($"foo {(TimeSpan?)TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Error_formatted()
    {
        _log.Error($"foo {TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_TimeSpan_Error_nullable_formatted()
    {
        _log.Error($"foo {(TimeSpan?)TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_TimeSpan_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_Enum_Error()
    {
        _log.Error($"foo {DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_log_Enum_Error_nullable()
    {
        _log.Error($"foo {(DayOfWeek?)DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_not_allocate_Enum_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {DayOfWeek.Friday} bar"));
    }

    [Test]
    public void should_log_indirect_Error()
    {
        _log.Error($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Error);
        message.ToString().ShouldEqual("foo 42 bar");
    }

    [Test]
    public void should_not_allocate_indirect_Error()
    {
        GcTester.ShouldNotAllocate(() => _log.Error($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar"));
    }

    [Test]
    public void should_not_log_above_level_Fatal()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Fatal + 1));
        _log.Fatal("Foo");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_not_log_above_level_Fatal_interpolated()
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Fatal + 1));
        _log.Fatal($"Foo {42}");
        _provider.ShouldNotBeLogged();
    }

    [Test]
    public void should_log_with_append_Fatal()
    {
        _log.Fatal().Append("Foo").Log();

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_String_Fatal()
    {
        _log.Fatal("foo");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual("foo");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_Exception_Fatal()
    {
        var exception = new InvalidOperationException();
        _log.Fatal("Foo", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_log_interpolated_String_Fatal()
    {
        _log.Fatal($"foo {NoInline("bar")} baz {NoInline("foobar")}");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual("foo bar baz foobar");
        message.Exception.ShouldBeNull();
    }

    [Test]
    public void should_log_interpolated_Exception_Fatal()
    {
        var exception = new InvalidOperationException();
        _log.Fatal($"Foo {42}", exception);

        var message = _provider.GetSubmittedMessage();
        message.ToString().ShouldEqual("Foo 42");
        message.Exception.ShouldBeTheSameAs(exception);
    }

    [Test]
    public void should_not_allocate_String_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal("Foo"));
    }

    [Test]
    public void should_not_allocate_interpolated_String_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {NoInline("bar")} baz {NoInline("foobar")}"));
    }

    [Test]
    public void should_log_Boolean_Fatal()
    {
        _log.Fatal($"foo {TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Boolean_Fatal_nullable()
    {
        _log.Fatal($"foo {(bool?)TestValues.Boolean} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_not_allocate_Boolean_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Boolean} bar"));
    }

    [Test]
    public void should_log_Byte_Fatal()
    {
        _log.Fatal($"foo {TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Fatal_nullable()
    {
        _log.Fatal($"foo {(byte?)TestValues.Byte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_Byte_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Byte_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(byte?)TestValues.Byte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Byte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Byte_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Byte} bar"));
    }

    [Test]
    public void should_log_SByte_Fatal()
    {
        _log.Fatal($"foo {TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Fatal_nullable()
    {
        _log.Fatal($"foo {(sbyte?)TestValues.SByte} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_SByte_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_SByte_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(sbyte?)TestValues.SByte:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.SByte.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_SByte_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.SByte} bar"));
    }

    [Test]
    public void should_log_Char_Fatal()
    {
        _log.Fatal($"foo {TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Char_Fatal_nullable()
    {
        _log.Fatal($"foo {(char?)TestValues.Char} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_not_allocate_Char_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Char} bar"));
    }

    [Test]
    public void should_log_Int16_Fatal()
    {
        _log.Fatal($"foo {TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Fatal_nullable()
    {
        _log.Fatal($"foo {(short?)TestValues.Int16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_Int16_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int16_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(short?)TestValues.Int16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Int16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int16_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Int16} bar"));
    }

    [Test]
    public void should_log_UInt16_Fatal()
    {
        _log.Fatal($"foo {TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Fatal_nullable()
    {
        _log.Fatal($"foo {(ushort?)TestValues.UInt16} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_UInt16_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt16_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(ushort?)TestValues.UInt16:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UInt16.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt16_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.UInt16} bar"));
    }

    [Test]
    public void should_log_Int32_Fatal()
    {
        _log.Fatal($"foo {TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Fatal_nullable()
    {
        _log.Fatal($"foo {(int?)TestValues.Int32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_Int32_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int32_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(int?)TestValues.Int32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Int32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int32_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Int32} bar"));
    }

    [Test]
    public void should_log_UInt32_Fatal()
    {
        _log.Fatal($"foo {TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Fatal_nullable()
    {
        _log.Fatal($"foo {(uint?)TestValues.UInt32} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_UInt32_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt32_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(uint?)TestValues.UInt32:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UInt32.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt32_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.UInt32} bar"));
    }

    [Test]
    public void should_log_Int64_Fatal()
    {
        _log.Fatal($"foo {TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Fatal_nullable()
    {
        _log.Fatal($"foo {(long?)TestValues.Int64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_Int64_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Int64_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(long?)TestValues.Int64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Int64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Int64_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Int64} bar"));
    }

    [Test]
    public void should_log_UInt64_Fatal()
    {
        _log.Fatal($"foo {TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Fatal_nullable()
    {
        _log.Fatal($"foo {(ulong?)TestValues.UInt64} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_UInt64_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UInt64_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(ulong?)TestValues.UInt64:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UInt64.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UInt64_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.UInt64} bar"));
    }

    [Test]
    public void should_log_IntPtr_Fatal()
    {
        _log.Fatal($"foo {TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Fatal_nullable()
    {
        _log.Fatal($"foo {(nint?)TestValues.IntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_IntPtr_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_IntPtr_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(nint?)TestValues.IntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.IntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_IntPtr_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.IntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Fatal()
    {
        _log.Fatal($"foo {TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Fatal_nullable()
    {
        _log.Fatal($"foo {(nuint?)TestValues.UIntPtr} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_UIntPtr_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_UIntPtr_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(nuint?)TestValues.UIntPtr:X} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.UIntPtr.ToString("X", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_UIntPtr_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.UIntPtr} bar"));
    }

    [Test]
    public void should_log_Single_Fatal()
    {
        _log.Fatal($"foo {TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Fatal_nullable()
    {
        _log.Fatal($"foo {(float?)TestValues.Single} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Single_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Single_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(float?)TestValues.Single:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Single.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Single_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Single} bar"));
    }

    [Test]
    public void should_log_Double_Fatal()
    {
        _log.Fatal($"foo {TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Fatal_nullable()
    {
        _log.Fatal($"foo {(double?)TestValues.Double} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Double_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Double_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(double?)TestValues.Double:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Double.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Double_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Double} bar"));
    }

    [Test]
    public void should_log_Decimal_Fatal()
    {
        _log.Fatal($"foo {TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Fatal_nullable()
    {
        _log.Fatal($"foo {(decimal?)TestValues.Decimal} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Decimal_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Decimal_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(decimal?)TestValues.Decimal:F4} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Decimal.ToString("F4", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Decimal_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Decimal} bar"));
    }

    [Test]
    public void should_log_Guid_Fatal()
    {
        _log.Fatal($"foo {TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Fatal_nullable()
    {
        _log.Fatal($"foo {(Guid?)TestValues.Guid} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_Guid_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_Guid_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(Guid?)TestValues.Guid:B} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.Guid.ToString("B", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_Guid_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.Guid} bar"));
    }

    [Test]
    public void should_log_DateTime_Fatal()
    {
        _log.Fatal($"foo {TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Fatal_nullable()
    {
        _log.Fatal($"foo {(DateTime?)TestValues.DateTime} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_DateTime_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_DateTime_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(DateTime?)TestValues.DateTime:yyyy-MM-dd} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_DateTime_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.DateTime} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Fatal()
    {
        _log.Fatal($"foo {TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Fatal_nullable()
    {
        _log.Fatal($"foo {(TimeSpan?)TestValues.TimeSpan} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual(FormattableString.Invariant($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_TimeSpan_Fatal_formatted()
    {
        _log.Fatal($"foo {TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_log_TimeSpan_Fatal_nullable_formatted()
    {
        _log.Fatal($"foo {(TimeSpan?)TestValues.TimeSpan:g} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual($"foo {TestValues.TimeSpan.ToString("g", CultureInfo.InvariantCulture)} bar");
    }

    [Test]
    public void should_not_allocate_TimeSpan_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {TestValues.TimeSpan} bar"));
    }

    [Test]
    public void should_log_Enum_Fatal()
    {
        _log.Fatal($"foo {DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_log_Enum_Fatal_nullable()
    {
        _log.Fatal($"foo {(DayOfWeek?)DayOfWeek.Friday} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual("foo Friday bar");
    }

    [Test]
    public void should_not_allocate_Enum_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {DayOfWeek.Friday} bar"));
    }

    [Test]
    public void should_log_indirect_Fatal()
    {
        _log.Fatal($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar");

        var message = _provider.GetSubmittedMessage();
        message.Level.ShouldEqual(LogLevel.Fatal);
        message.ToString().ShouldEqual("foo 42 bar");
    }

    [Test]
    public void should_not_allocate_indirect_Fatal()
    {
        GcTester.ShouldNotAllocate(() => _log.Fatal($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar"));
    }

}
