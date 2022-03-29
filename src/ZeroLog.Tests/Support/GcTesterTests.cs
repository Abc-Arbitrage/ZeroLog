using NUnit.Framework;

namespace ZeroLog.Tests.Support;

[TestFixture]
public class GcTesterTests
{
    [Test]
    public void should_detect_allocations()
    {
        Assert.Throws<AssertionException>(() => GcTester.ShouldNotAllocate(() => _ = new object()));
    }

    [Test]
    public void should_not_detect_warmup_allocations()
    {
        GcTester.ShouldNotAllocate(() => { }, () => _ = new object());
    }

    [Test]
    public void should_not_throw_when_there_are_no_allocations()
    {
        var callCount = 0;
        var afterWarmupCount = 0;

        GcTester.ShouldNotAllocate(() => ++callCount, () => ++afterWarmupCount);

        callCount.ShouldEqual(2);
        afterWarmupCount.ShouldEqual(1);
    }
}
