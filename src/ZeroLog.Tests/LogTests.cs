using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public partial class LogTests
{
    private Log _log;
    private TestLogMessageProvider _provider;

    static LogTests()
    {
        LogManager.RegisterEnum<DayOfWeek>();
    }

    [SetUp]
    public void SetUp()
    {
        _provider = new TestLogMessageProvider();

        _log = new Log("TestLog");
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Trace));
    }

    [TearDown]
    public void Teardown()
    {
        _provider.Dispose();
    }

    [Test]
    public void should_be_disabled_at_construction()
    {
        _log = new Log("TestLog");

        _log.IsFatalEnabled.ShouldBeFalse();
        _log.IsEnabled(LogLevel.Fatal).ShouldBeFalse();

        _log.Fatal().ShouldBeTheSameAs(LogMessage.Empty);
    }

    [TestCase(LogLevel.Trace, true, true, true, true, true, true)]
    [TestCase(LogLevel.Debug, false, true, true, true, true, true)]
    [TestCase(LogLevel.Info, false, false, true, true, true, true)]
    [TestCase(LogLevel.Warn, false, false, false, true, true, true)]
    [TestCase(LogLevel.Error, false, false, false, false, true, true)]
    [TestCase(LogLevel.Fatal, false, false, false, false, false, true)]
    public void should_tell_if_log_level_is_enabled(LogLevel logLevel, bool isTrace, bool isDebug, bool isInfo, bool isWarn, bool isError, bool isFatal)
    {
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(logLevel));

        _log.IsTraceEnabled.ShouldEqual(isTrace);
        _log.IsDebugEnabled.ShouldEqual(isDebug);
        _log.IsInfoEnabled.ShouldEqual(isInfo);
        _log.IsWarnEnabled.ShouldEqual(isWarn);
        _log.IsErrorEnabled.ShouldEqual(isError);
        _log.IsFatalEnabled.ShouldEqual(isFatal);

        _log.IsEnabled(logLevel).ShouldBeTrue();
        _log.IsEnabled(logLevel - 1).ShouldBeFalse();
        _log.IsEnabled(logLevel + 1).ShouldBeTrue();
    }

    private static string NoInline(string value)
        => value;

    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    private static class TestValues
    {
        public static readonly bool Boolean = true;
        public static readonly byte Byte = 42;
        public static readonly sbyte SByte = -42;
        public static readonly char Char = 'x';
        public static readonly short Int16 = -4242;
        public static readonly ushort UInt16 = 4242;
        public static readonly int Int32 = -Random.Shared.Next();
        public static readonly uint UInt32 = (uint)Random.Shared.Next();
        public static readonly long Int64 = -(long)Random.Shared.Next() * Random.Shared.Next();
        public static readonly ulong UInt64 = (ulong)Random.Shared.Next() * (ulong)Random.Shared.Next();
        public static readonly nint IntPtr = nint.MinValue;
        public static readonly nuint UIntPtr = nuint.MaxValue;
        public static readonly float Single = 42.42f;
        public static readonly double Double = -42.42;
        public static readonly decimal Decimal = 42.42m;
        public static readonly Guid Guid = Guid.NewGuid();
        public static readonly DateTime DateTime = DateTime.UtcNow;
        public static readonly TimeSpan TimeSpan = DateTime.UtcNow.TimeOfDay;
    }
}
