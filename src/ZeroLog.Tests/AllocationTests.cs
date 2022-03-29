using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture, NonParallelizable]
public class AllocationTests
{
    private WaitableAppender _waitableAppender;
    private string _tempDirectory;

    public class WaitableAppender : DateAndSizeRollingFileAppender
    {
        public int WrittenEventCount { get; private set; }

        public WaitableAppender(string directory)
            : base(directory)
        {
        }

        public override void WriteMessage(LoggedMessage message)
        {
            WrittenEventCount++;
            base.WriteMessage(message);
        }
    }

    [SetUp]
    public void Setup()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);

        _waitableAppender = new WaitableAppender(_tempDirectory) { FileNamePrefix = "allocation-test" };

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 2048 * 10,
            LogMessageBufferSize = 512,
            RootLogger =
            {
                Appenders = { _waitableAppender }
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

        GC.Collect(2, GCCollectionMode.Forced, true);
        var gcCountBefore = GC.CollectionCount(0);

        var numberOfEvents = 2048 * 10;

        for (var i = 0; i < numberOfEvents; i++)
        {
            log
                .Info()
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

            log
                .Info()
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

            log
                .Info()
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
        while (_waitableAppender.WrittenEventCount < numberOfEvents)
            Thread.Sleep(1);

        var gcCountAfter = GC.CollectionCount(0);

        gcCountAfter.ShouldEqual(gcCountBefore);
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
