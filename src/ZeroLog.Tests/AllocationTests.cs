using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Tests;

[NonParallelizable]
[TestFixture(LogMessagePoolExhaustionStrategy.WaitUntilAvailable)]
[TestFixture(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders)]
public class AllocationTests
{
    private readonly LogMessagePoolExhaustionStrategy _exhaustionStrategy;
    private AwaitableAppender _awaitableAppender;
    private string _tempDirectory;

    public AllocationTests(LogMessagePoolExhaustionStrategy exhaustionStrategy)
    {
        _exhaustionStrategy = exhaustionStrategy;
    }

    private class AwaitableAppender : DateAndSizeRollingFileAppender
    {
        private int _writtenEventCount;

        public long AllocatedBytesOnAppenderThread { get; private set; }

        public AwaitableAppender(string directory)
            : base(directory)
        {
            MaxFileSizeInBytes = 0;
        }

        public override void WriteMessage(LoggedMessage message)
        {
            base.WriteMessage(message);

            AllocatedBytesOnAppenderThread = GC.GetAllocatedBytesForCurrentThread();
            Thread.MemoryBarrier();
            ++_writtenEventCount;
        }

        public void WaitForMessageCount(int count)
        {
            while (Volatile.Read(ref _writtenEventCount) < count)
                Thread.Yield();
        }
    }

    [SetUp]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);

        _awaitableAppender = new AwaitableAppender(_tempDirectory) { FileNamePrefix = "allocation-test" };

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 2048 * 10,
            LogMessageBufferSize = 512,
            RootLogger =
            {
                LogMessagePoolExhaustionStrategy = _exhaustionStrategy,
                Appenders = { _awaitableAppender }
            }
        });

        LogManager.RegisterEnum<DayOfWeek>();
        LogManager.RegisterUnmanaged<UnmanagedStruct>();
    }

    [TearDown]
    public void Teardown()
    {
        LogManager.Shutdown();
        Directory.Delete(_tempDirectory, true);
    }

    [Test]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    public void should_not_allocate_using_all_formats_and_file_appender_builder()
    {
        var log = LogManager.GetLogger("AllocationTest");

        var allocationsOnLoggingThread = 0L;
        var allocationsOnAppenderThread = 0L;

        const int numberOfEvents = 2048 * 10;
        const int warmupEvents = 10;

        for (var i = 0; i < numberOfEvents; ++i)
        {
            if (i == warmupEvents)
            {
                _awaitableAppender.WaitForMessageCount(warmupEvents);

                allocationsOnLoggingThread = GC.GetAllocatedBytesForCurrentThread();
                allocationsOnAppenderThread = _awaitableAppender.AllocatedBytesOnAppenderThread;
            }

            log.Info()
               .Append("Int ")
               .Append(123243)
               .Append("Double ")
               .Append(32423432.4398438, "N4")
               .Append("String ")
               .Append("Some random string")
               .Append("Bool ")
               .Append(true)
               .Append("Decimal ")
               .Append(4234324324.23423423, "N4")
               .Append("Guid ")
               .Append(Guid.NewGuid())
               .Append("Timestamp ")
               .Append(DateTime.UtcNow.TimeOfDay)
               .Append("DateTime ")
               .Append(DateTime.UtcNow)
               .Log();

            log.Info()
               .Append("Enum ")
               .AppendEnum(DayOfWeek.Friday)
               .Append("UnknownEnum ")
               .AppendEnum(UnregisteredEnum.Bar)
               .Append("NullableEnum ")
               .AppendEnum((DayOfWeek?)DayOfWeek.Monday)
               .Append("NullableNullEnum ")
               .AppendEnum((DayOfWeek?)null)
               .Append("NullableInt ")
               .Append((int?)42)
               .Append("NullableNullInt ")
               .Append((int?)null)
               .Log();

            var unmanaged = new UnmanagedStruct(1, 2, 3);
            var unregisteredUnmanaged = new UnregisteredUnmanagedStruct(4, 5, 6);
            var nullableUnmanaged = (UnmanagedStruct?)unmanaged;
            var nullNullableUnmanaged = (UnmanagedStruct?)null;
            var nullableUnregisteredUnmanaged = (UnregisteredUnmanagedStruct?)new UnregisteredUnmanagedStruct(4, 5, 6);
            var nullNullableUnregisteredUnmanaged = (UnregisteredUnmanagedStruct?)null;

            log.Info()
               .Append("Unmanaged Struct ")
               .AppendUnmanaged(unmanaged)
               .Append("Unregistered Unmanaged Struct ")
               .AppendUnmanaged(unregisteredUnmanaged)
               .Append("Unmanaged Struct byref ")
               .AppendUnmanaged(ref unmanaged)
               .Append("Unregistered Unmanaged byref ")
               .AppendUnmanaged(ref unregisteredUnmanaged)
               .Append("Nullable Unmanaged ")
               .AppendUnmanaged(nullableUnmanaged)
               .Append("Null Nullable Unmanaged ")
               .AppendUnmanaged(nullNullableUnmanaged)
               .Append("Nullable Unregistered Unmanaged ")
               .AppendUnmanaged(nullableUnregisteredUnmanaged)
               .Append("Null Nullable Unregistered Unmanaged")
               .AppendUnmanaged(nullNullableUnregisteredUnmanaged)
               .Append("Nullable Unmanaged byref ")
               .AppendUnmanaged(ref nullableUnmanaged)
               .Append("Null Nullable Unmanaged byref ")
               .AppendUnmanaged(ref nullNullableUnmanaged)
               .Append("Nullable Unregistered Unmanaged byref ")
               .AppendUnmanaged(ref nullableUnregisteredUnmanaged)
               .Append("Null Nullable Unregistered Unmanaged byref ")
               .AppendUnmanaged(ref nullNullableUnregisteredUnmanaged)
               .Log();
        }

        // Give the appender some time to finish writing to file
        _awaitableAppender.WaitForMessageCount(numberOfEvents);

        allocationsOnLoggingThread = GC.GetAllocatedBytesForCurrentThread() - allocationsOnLoggingThread;
        allocationsOnAppenderThread = _awaitableAppender.AllocatedBytesOnAppenderThread - allocationsOnAppenderThread;

        Assert.Zero(allocationsOnLoggingThread, "Allocations on logging thread");
        Assert.Zero(allocationsOnAppenderThread, "Allocations on appender thread");
    }

    private enum UnregisteredEnum
    {
        Foo,
        Bar,
        Baz
    }

    private readonly struct UnmanagedStruct : ISpanFormattable
    {
        public readonly long A;
        public readonly int B;
        public readonly byte C;

        public UnmanagedStruct(long a, int b, byte c)
        {
            A = a;
            B = b;
            C = c;
        }

        public string ToString(string format, IFormatProvider formatProvider)
            => throw new NotSupportedException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            var builder = new CharBufferBuilder(destination);
            builder.TryAppend(A);
            builder.Append('-');
            builder.TryAppend(B);
            builder.Append('-');
            builder.TryAppend(C);
            charsWritten = builder.Length;
            return true;
        }
    }

    private readonly struct UnregisteredUnmanagedStruct : ISpanFormattable
    {
        public readonly long D;
        public readonly int E;
        public readonly byte F;

        public UnregisteredUnmanagedStruct(long d, int e, byte f)
        {
            D = d;
            E = e;
            F = f;
        }

        public string ToString(string format, IFormatProvider formatProvider)
            => throw new NotSupportedException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            var builder = new CharBufferBuilder(destination);
            builder.TryAppend(D);
            builder.Append('-');
            builder.TryAppend(E);
            builder.Append('-');
            builder.TryAppend(F);
            charsWritten = builder.Length;
            return true;
        }
    }
}
