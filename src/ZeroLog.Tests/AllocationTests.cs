using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Tests;

[NonParallelizable]
[TestFixture(LogMessagePoolExhaustionStrategy.WaitUntilAvailable)]
[TestFixture(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders)]
public class AllocationTests(LogMessagePoolExhaustionStrategy exhaustionStrategy)
{
    private AwaitableAppender _awaitableAppender;
    private string _tempDirectory;

    private class AwaitableAppender : DateAndSizeRollingFileAppender
    {
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
                LogMessagePoolExhaustionStrategy = exhaustionStrategy,
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
                LogManager.Flush();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

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
        LogManager.Flush();

        allocationsOnLoggingThread = GC.GetAllocatedBytesForCurrentThread() - allocationsOnLoggingThread;
        allocationsOnAppenderThread = _awaitableAppender.AllocatedBytesOnAppenderThread - allocationsOnAppenderThread;

#if NET7_0_OR_GREATER
        Assert.That(allocationsOnLoggingThread, Is.Zero, "Allocations on logging thread");
        Assert.That(allocationsOnAppenderThread, Is.Zero, "Allocations on appender thread");
#else
        // .NET 6 allocates 40 bytes on both threads, independently of the event count.
        // I don't know why, but .NET 7 doesn't exhibit this behavior anymore, so I suppose it's just some glitch.

        Assert.That(allocationsOnLoggingThread, Is.LessThanOrEqualTo(40), "Allocations on logging thread");
        Assert.That(allocationsOnAppenderThread, Is.LessThanOrEqualTo(40), "Allocations on appender thread");
#endif
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum UnregisteredEnum
    {
        Foo,
        Bar,
        Baz
    }

    private readonly struct UnmanagedStruct(long a, int b, byte c) : ISpanFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider)
            => throw new NotSupportedException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            var builder = new CharBufferBuilder(destination);
            builder.TryAppend(a);
            builder.Append('-');
            builder.TryAppend(b);
            builder.Append('-');
            builder.TryAppend(c);
            charsWritten = builder.Length;
            return true;
        }
    }

    private readonly struct UnregisteredUnmanagedStruct(long d, int e, byte f) : ISpanFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider)
            => throw new NotSupportedException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
        {
            var builder = new CharBufferBuilder(destination);
            builder.TryAppend(d);
            builder.Append('-');
            builder.TryAppend(e);
            builder.Append('-');
            builder.TryAppend(f);
            charsWritten = builder.Length;
            return true;
        }
    }
}
