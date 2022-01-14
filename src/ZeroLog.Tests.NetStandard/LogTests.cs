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
    public void should_not_throw()
    {
        _log.IsFatalEnabled.ShouldBeTrue();

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
    }
}
