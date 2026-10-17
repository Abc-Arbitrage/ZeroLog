using NUnit.Framework;
using ObjectLayoutInspector;
using Shouldly;

namespace ZeroLog.Tests;

[TestFixture]
public class ObjectPoolTests
{
    [Test]
    public void should_provide_items()
    {
        var pool = new ObjectPool<Item>(3, () => new Item());
        pool.Count.ShouldBe(3);

        pool.TryAcquire(out var a).ShouldBeTrue();
        pool.Count.ShouldBe(2);
        a.ShouldNotBeNull();

        pool.TryAcquire(out var b).ShouldBeTrue();
        pool.Count.ShouldBe(1);
        b.ShouldNotBeNull();

        pool.TryAcquire(out var c).ShouldBeTrue();
        pool.Count.ShouldBe(0);
        c.ShouldNotBeNull();

        pool.TryAcquire(out var d).ShouldBeFalse();
        pool.Count.ShouldBe(0);
        d.ShouldBeNull();

        a.ShouldNotBeSameAs(b);
        b.ShouldNotBeSameAs(c);
        c.ShouldNotBeSameAs(a);
    }

    [Test]
    public void should_return_items()
    {
        var pool = new ObjectPool<Item>(2, () => new Item());
        pool.Count.ShouldBe(2);

        pool.TryAcquire(out var a).ShouldBeTrue();
        pool.Count.ShouldBe(1);
        a.ShouldNotBeNull();

        pool.TryAcquire(out var b).ShouldBeTrue();
        pool.Count.ShouldBe(0);
        b.ShouldNotBeNull();

        pool.TryAcquire(out _).ShouldBeFalse();
        pool.Count.ShouldBe(0);

        pool.Release(a);
        pool.Count.ShouldBe(1);

        pool.TryAcquire(out var c).ShouldBeTrue();
        pool.Count.ShouldBe(0);
        c.ShouldBeSameAs(a);
    }

    [Test]
    public void should_not_exceed_capacity()
    {
        var pool = new ObjectPool<Item>(2, () => new Item());
        pool.Count.ShouldBe(2);

        pool.Release(new Item());
        pool.Count.ShouldBe(3); // Allow one cached instance to exceed the capacity

        pool.Release(new Item());
        pool.Count.ShouldBe(3);
    }

    // [Test, Explicit] // MTP complains about explicit tests
    public void show_layout()
    {
        TypeLayout.PrintLayout<ObjectPool<Item>>(false);
    }

    private class Item;
}
