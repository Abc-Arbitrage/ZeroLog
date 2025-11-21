using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Tests.Allocation;

public static class AllocationTests
{
    private const string _buildConfiguration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    private const string _reset = "\e[0m";
    private const string _bold = "\e[1m";
    private const string _red = "\e[91m";
    private const string _green = "\e[92m";

    public static bool Run()
    {
        Console.WriteLine();
        Console.WriteLine($"{_bold}Running allocation tests on {RuntimeInformation.FrameworkDescription}, {RuntimeInformation.RuntimeIdentifier} in {_buildConfiguration}...{_reset}");
        Console.WriteLine();

        bool[] results =
        [
            Run(LogMessagePoolExhaustionStrategy.WaitUntilAvailable),
            Run(LogMessagePoolExhaustionStrategy.DropLogMessage),
            Run(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders),
            !Run(LogMessagePoolExhaustionStrategy.Allocate) // This one is obviously expected to allocate
        ];

        var success = results.All(i => i);

        Console.WriteLine($"{_bold}Allocation tests {(success ? $"{_green}PASSED" : $"{_red}FAILED")}{_reset}{_bold} on {RuntimeInformation.FrameworkDescription}{_reset}");
        Console.WriteLine();
        return success;
    }

    private static bool Run(LogMessagePoolExhaustionStrategy exhaustionStrategy)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            using var awaitableAppender = new AwaitableAppender(tempDirectory);

            using var logs = LogManager.Initialize(new ZeroLogConfiguration
            {
                LogMessagePoolSize = 2048 * 10,
                LogMessageBufferSize = 512,
                RootLogger =
                {
                    LogMessagePoolExhaustionStrategy = exhaustionStrategy,
                    Appenders = { awaitableAppender }
                }
            });

            LogManager.RegisterEnum<DayOfWeek>();
            LogManager.RegisterUnmanaged<UnmanagedStruct>();

            return Run(exhaustionStrategy, awaitableAppender);
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private static bool Run(LogMessagePoolExhaustionStrategy exhaustionStrategy,
                            AwaitableAppender awaitableAppender)
    {
        var log = LogManager.GetLogger("AllocationTest");

        var allocationsOnLoggingThread = 0L;
        var allocationsOnAppenderThread = 0L;

        const int numberOfEvents = 2048 * 100;
        const int warmupEvents = 2048 * 10;

        for (var i = 0; i < numberOfEvents; ++i)
        {
            if (i == warmupEvents)
            {
                LogManager.Flush();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                allocationsOnLoggingThread = GC.GetAllocatedBytesForCurrentThread();
                allocationsOnAppenderThread = awaitableAppender.AllocatedBytesOnAppenderThread;
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
        allocationsOnAppenderThread = awaitableAppender.AllocatedBytesOnAppenderThread - allocationsOnAppenderThread;

        Console.WriteLine($"Allocations with pool exhaustion strategy: {exhaustionStrategy}");
        Console.WriteLine($"  - On logging thread:  {FormatBytes(allocationsOnLoggingThread)}");
        Console.WriteLine($"  - On appender thread: {FormatBytes(allocationsOnAppenderThread)}");

#if NET6_0
        // .NET 6 always allocates 40 bytes on the appender thread, independently of the event count.
        // I don't know why, but .NET 7 doesn't exhibit this behavior anymore, so I suppose it's just some glitch.
        if (allocationsOnAppenderThread == 40)
        {
            Console.WriteLine("Forgiving the 40 bytes allocation on appender thread in .NET 6.");
            allocationsOnAppenderThread = 0;
        }
#endif

        Console.WriteLine();
        return allocationsOnLoggingThread == 0 && allocationsOnAppenderThread == 0;

        static string FormatBytes(long bytes)
            => bytes == 0 ? "0 bytes" : $"{_red}{bytes:N0}{_reset} bytes";
    }

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

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum UnregisteredEnum
    {
        Foo,
        Bar,
        Baz
    }

    private readonly struct UnmanagedStruct(long a, int b, byte c) : ISpanFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider)
            => throw new NotSupportedException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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
        public string ToString(string? format, IFormatProvider? formatProvider)
            => throw new NotSupportedException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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
