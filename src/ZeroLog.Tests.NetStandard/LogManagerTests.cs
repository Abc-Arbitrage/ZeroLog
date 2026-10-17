using NUnit.Framework;
using Shouldly;

namespace ZeroLog.Tests.NetStandard;

[TestFixture]
public class LogManagerTests
{
    [Test]
    public void should_return_cached_log_instance()
    {
        var fooLog = LogManager.GetLogger("Foo");
        var barLog = LogManager.GetLogger("Bar");
        var fooLog2 = LogManager.GetLogger("Foo");

        fooLog.ShouldNotBeNull();
        barLog.ShouldNotBeNull();

        barLog.ShouldNotBeSameAs(fooLog);
        fooLog2.ShouldBeSameAs(fooLog);
    }
}
