using NUnit.Framework;
using ZeroLog.Tests.Support;

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

        barLog.ShouldNotBeTheSameAs(fooLog);
        fooLog2.ShouldBeTheSameAs(fooLog);
    }
}
