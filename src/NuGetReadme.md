# ZeroLog

**ZeroLog is a high-performance, zero-allocation .NET logging library**.

It provides logging capabilities to be used in latency-sensitive applications, where garbage collections are undesirable. ZeroLog can be used in a complete zero-allocation manner, meaning that after the initialization phase, it will not allocate any managed object on the heap, thus preventing any GC from being triggered.

.NET 6 and C# 10 or later are required to use this library.

See the [GitHub repository](https://github.com/Abc-Arbitrage/ZeroLog) for more information.
