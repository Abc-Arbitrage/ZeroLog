using System;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.NetStandard;

[TestFixture]
public class LogTests
{
    private Log _log;

    [SetUp]
    public void SetUp()
    {
        _log = new Log("Foo");
    }

    [Test]
    public void should_not_throw_fatal()
    {
        _log.IsFatalEnabled.ShouldBeFalse();
        _log.IsEnabled(Level.Fatal).ShouldBeFalse();

        _log.Fatal("Message");
        _log.Fatal("Message", new InvalidOperationException());

        _log.Fatal($"Message {42}");
        _log.Fatal($"Message {42}", new InvalidOperationException());

        _log.Fatal()
            .Append("Message")
            .Append($"Other {42}")
            .Append(42)
            .Log();

        _log.Fatal().ShouldBeTheSameAs(LogMessage.Empty);
        _log.ForLevel(Level.Fatal).ShouldBeTheSameAs(LogMessage.Empty);
    }

    [Test]
    public void should_not_throw_trace()
    {
        _log.IsTraceEnabled.ShouldBeFalse();
        _log.IsEnabled(Level.Trace).ShouldBeFalse();

        _log.Trace("Message");
        _log.Trace("Message", new InvalidOperationException());

        _log.Trace($"Message {42}");
        _log.Trace($"Message {42}", new InvalidOperationException());

        _log.Trace()
            .Append("Message")
            .Append($"Other {42}")
            .Append(42)
            .Log();

        _log.Trace().ShouldBeTheSameAs(LogMessage.Empty);
        _log.ForLevel(Level.Trace).ShouldBeTheSameAs(LogMessage.Empty);
    }
}
