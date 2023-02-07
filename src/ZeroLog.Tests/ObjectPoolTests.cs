using NUnit.Framework;
using ObjectLayoutInspector;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class ObjectPoolTests
{
    [Test]
    public void should_provide_items()
    {
        var pool = new ObjectPool<Item>(3, () => new Item());
        pool.Count.ShouldEqual(3);

        pool.TryAcquire(out var a).ShouldBeTrue();
        pool.Count.ShouldEqual(2);
        a.ShouldNotBeNull();

        pool.TryAcquire(out var b).ShouldBeTrue();
        pool.Count.ShouldEqual(1);
        b.ShouldNotBeNull();

        pool.TryAcquire(out var c).ShouldBeTrue();
        pool.Count.ShouldEqual(0);
        c.ShouldNotBeNull();

        pool.TryAcquire(out var d).ShouldBeFalse();
        pool.Count.ShouldEqual(0);
        d.ShouldBeNull();

        a.ShouldNotBeTheSameAs(b);
        b.ShouldNotBeTheSameAs(c);
        c.ShouldNotBeTheSameAs(a);
    }

    [Test]
    public void should_return_items()
    {
        var pool = new ObjectPool<Item>(2, () => new Item());
        pool.Count.ShouldEqual(2);

        pool.TryAcquire(out var a).ShouldBeTrue();
        pool.Count.ShouldEqual(1);
        a.ShouldNotBeNull();

        pool.TryAcquire(out var b).ShouldBeTrue();
        pool.Count.ShouldEqual(0);
        b.ShouldNotBeNull();

        pool.TryAcquire(out _).ShouldBeFalse();
        pool.Count.ShouldEqual(0);

        pool.Release(a);
        pool.Count.ShouldEqual(1);

        pool.TryAcquire(out var c).ShouldBeTrue();
        pool.Count.ShouldEqual(0);
        c.ShouldBeTheSameAs(a);
    }

    [Test]
    public void should_not_exceed_capacity()
    {
        var pool = new ObjectPool<Item>(2, () => new Item());
        pool.Count.ShouldEqual(2);

        pool.Release(new Item());
        pool.Count.ShouldEqual(3); // Allow one cached instance to exceed the capacity

        pool.Release(new Item());
        pool.Count.ShouldEqual(3);
    }

    [Test, Explicit]
    public void show_layout()
    {
        TypeLayout.PrintLayout<ObjectPool<Item>>(false);
    }

    private class Item
    {
    }
}
