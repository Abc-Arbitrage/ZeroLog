using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog.Tests.Aot;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "RawStringCanBeSimplified")]
internal static class Program
{
    private const string _buildConfiguration =
#if DEBUG
        "Debug";
#else
        "Release";
#endif

    private static readonly Log _log = LogManager.GetLogger(typeof(Program));

    private const string _reset = "\e[0m";
    private const string _bold = "\e[1m";
    private const string _red = "\e[91m";
    private const string _green = "\e[92m";

    private static int Main()
    {
        Console.WriteLine();
        Console.WriteLine($"{_bold}Running AOT tests on {RuntimeInformation.FrameworkDescription}, {RuntimeInformation.RuntimeIdentifier} in {_buildConfiguration}...{_reset}");
        Console.WriteLine();

        var stringBuilder = new StringBuilder();
        var expectations = new List<string>();
        RunSession(stringBuilder, expectations);
        var stringOutput = stringBuilder.ToString();

        Console.WriteLine();
        Console.WriteLine($"{_bold}Results:{_reset}");
        Console.WriteLine();

        var success = true;

        CheckExpectation(!RuntimeFeature.IsDynamicCodeSupported, "RuntimeFeature.IsDynamicCodeSupported is false");
        CheckExpectation(string.IsNullOrEmpty(GetAssemblyLocation()), "typeof(Program).Assembly.Location is empty");

        foreach (var expectation in expectations)
            CheckSubstring(expectation);

        CheckSubstring("Goodbye!");

        Console.WriteLine();
        Console.WriteLine($"{_bold}AOT tests: {(success ? $"{_green}PASSED" : $"{_red}FAILED")}{_reset}");
        Console.WriteLine();

        return success ? 0 : 1;

        void CheckSubstring(string expected)
            => CheckExpectation(stringOutput.Contains(expected), expected);

        void CheckExpectation(bool result, string label)
        {
            Console.WriteLine($"  - {(result ? $"{_green}PASSED" : $"{_red}FAILED")}{_reset}: {label}");

            if (!result)
                success = false;
        }

        [UnconditionalSuppressMessage("SingleFile", "IL3000")]
        static string GetAssemblyLocation()
            => typeof(Program).Assembly.Location;
    }

    private static void RunSession(StringBuilder stringBuilder, List<string> expectations)
    {
        using var logManager = (LogManager)LogManager.Initialize(new ZeroLogConfiguration
        {
            AutoRegisterEnums = false,
            RootLogger =
            {
                Level = LogLevel.Trace,
                Appenders =
                {
                    new ConsoleAppender(),
                    new TextWriterAppender(new StringWriter(stringBuilder))
                }
            }
        });

        _log.Info("Hello, world!");
        _log.Info("-----");

        _log.Trace().Append("Default trace message").AppendKeyValue("Foo", "Bar").Log();
        _log.Debug().Append("Default debug message").AppendKeyValue("Foo", "Bar").Log();
        _log.Info().Append("Default info message").AppendKeyValue("Foo", "Bar").Log();
        _log.Warn().Append("Default warn message").AppendKeyValue("Foo", "Bar").Log();
        _log.Error().Append("Default error message").AppendKeyValue("Foo", "Bar").Log();
        _log.Fatal().Append("Default fatal message").AppendKeyValue("Foo", "Bar").Log();

        _log.Error("Default exception", new InvalidOperationException("Something went wrong"));

        _log.Info("-----");

        _log.Info()
            .Append(nameof(RuntimeInformation))
            .AppendKeyValue(nameof(RuntimeInformation.FrameworkDescription), RuntimeInformation.FrameworkDescription)
            .AppendKeyValue(nameof(RuntimeInformation.RuntimeIdentifier), RuntimeInformation.RuntimeIdentifier)
            .AppendKeyValue(nameof(RuntimeInformation.OSDescription), RuntimeInformation.OSDescription)
            .Log();

        _log.Info()
            .Append(nameof(RuntimeFeature))
            .AppendKeyValue(nameof(RuntimeFeature.IsDynamicCodeSupported), RuntimeFeature.IsDynamicCodeSupported)
            .AppendKeyValue(nameof(RuntimeFeature.IsDynamicCodeCompiled), RuntimeFeature.IsDynamicCodeCompiled)
            .Log();

        _log.Info("-----");

        LogManager.Flush();
        EnumCache.Clear();

        WriteEnumInfo("""Enum by default: DayOfWeek""", DayOfWeek.Friday);
        AddExpectation("""Enum by default: DayOfWeek: 5""");
        AddExpectation("""Enum by default: DayOfWeek (structured) ~~ { "DayOfWeek": "5" }""");

        WriteEnumInfo("""Enum by default: UnregisteredEnum""", UnregisteredEnum.Bar);
        AddExpectation("""Enum by default: UnregisteredEnum: 1""");
        AddExpectation("""Enum by default: UnregisteredEnum (structured) ~~ { "UnregisteredEnum": "1" }""");

        _log.Info("-----");

        LogManager.Flush();
        EnumCache.Clear();
        LogManager.RegisterEnum<RegisteredEnumWithGeneric>();
        LogManager.RegisterEnum(typeof(RegisteredEnumWithTypeof));

        WriteEnumInfo("""Enum registered: DayOfWeek""", DayOfWeek.Friday);
        AddExpectation("""Enum registered: DayOfWeek: 5""");
        AddExpectation("""Enum registered: DayOfWeek (structured) ~~ { "DayOfWeek": "5" }""");

        WriteEnumInfo("""Enum registered: RegisteredEnumWithGeneric""", RegisteredEnumWithGeneric.Bar);
        AddExpectation("""Enum registered: RegisteredEnumWithGeneric: Bar""");
        AddExpectation("""Enum registered: RegisteredEnumWithGeneric (structured) ~~ { "RegisteredEnumWithGeneric": "Bar" }""");

        WriteEnumInfo("""Enum registered: RegisteredEnumWithTypeof""", RegisteredEnumWithTypeof.FizzBuzz);
        AddExpectation("""Enum registered: RegisteredEnumWithTypeof: FizzBuzz""");
        AddExpectation("""Enum registered: RegisteredEnumWithTypeof (structured) ~~ { "RegisteredEnumWithTypeof": "FizzBuzz" }""");

        WriteEnumInfo("""Enum registered: UnregisteredEnum""", UnregisteredEnum.Bar);
        AddExpectation("""Enum registered: UnregisteredEnum: 1""");
        AddExpectation("""Enum registered: UnregisteredEnum (structured) ~~ { "UnregisteredEnum": "1" }""");

        _log.Info("-----");

        LogManager.Flush();
        EnumCache.Clear();
        LogManager.RegisterAllEnumsFrom(typeof(Program).Assembly);

        WriteEnumInfo("""Enum registered from assembly: DayOfWeek""", DayOfWeek.Friday);
        AddExpectation("""Enum registered from assembly: DayOfWeek: 5""");
        AddExpectation("""Enum registered from assembly: DayOfWeek (structured) ~~ { "DayOfWeek": "5" }""");

        WriteEnumInfo("""Enum registered from assembly: RegisteredEnumWithGeneric""", RegisteredEnumWithGeneric.Bar);
        AddExpectation("""Enum registered from assembly: RegisteredEnumWithGeneric: Bar""");
        AddExpectation("""Enum registered from assembly: RegisteredEnumWithGeneric (structured) ~~ { "RegisteredEnumWithGeneric": "Bar" }""");

        WriteEnumInfo("""Enum registered from assembly: RegisteredEnumWithTypeof""", RegisteredEnumWithTypeof.FizzBuzz);
        AddExpectation("""Enum registered from assembly: RegisteredEnumWithTypeof: FizzBuzz""");
        AddExpectation("""Enum registered from assembly: RegisteredEnumWithTypeof (structured) ~~ { "RegisteredEnumWithTypeof": "FizzBuzz" }""");

        WriteEnumInfo("""Enum registered from assembly: UnregisteredEnum""", UnregisteredEnum.Bar);
        AddExpectation("""Enum registered from assembly: UnregisteredEnum: Bar""");
        AddExpectation("""Enum registered from assembly: UnregisteredEnum (structured) ~~ { "UnregisteredEnum": "Bar" }""");

        _log.Info("-----");

        LogManager.Flush();
        EnumCache.Clear();
        LogManager.Configuration!.AutoRegisterEnums = true;
        LogManager.Configuration.ApplyChanges();
        logManager.WaitUntilNewConfigurationIsApplied();

        WriteEnumInfo("""Enum auto-registered: DayOfWeek""", DayOfWeek.Friday);
        AddExpectation("""Enum auto-registered: DayOfWeek: Friday""");
        AddExpectation("""Enum auto-registered: DayOfWeek (structured) ~~ { "DayOfWeek": "Friday" }""");

        WriteEnumInfo("""Enum auto-registered: RegisteredEnumWithGeneric""", RegisteredEnumWithGeneric.Bar);
        AddExpectation("""Enum auto-registered: RegisteredEnumWithGeneric: Bar""");
        AddExpectation("""Enum auto-registered: RegisteredEnumWithGeneric (structured) ~~ { "RegisteredEnumWithGeneric": "Bar" }""");

        WriteEnumInfo("""Enum auto-registered: RegisteredEnumWithTypeof""", RegisteredEnumWithTypeof.FizzBuzz);
        AddExpectation("""Enum auto-registered: RegisteredEnumWithTypeof: FizzBuzz""");
        AddExpectation("""Enum auto-registered: RegisteredEnumWithTypeof (structured) ~~ { "RegisteredEnumWithTypeof": "FizzBuzz" }""");

        WriteEnumInfo("""Enum auto-registered: UnregisteredEnum""", UnregisteredEnum.Bar);
        AddExpectation("""Enum auto-registered: UnregisteredEnum: Bar""");
        AddExpectation("""Enum auto-registered: UnregisteredEnum (structured) ~~ { "UnregisteredEnum": "Bar" }""");

        _log.Info("-----");

        LogManager.RegisterUnmanaged<RegisteredUnmanagedType>();

        _log.Warn()
            .Append("Registered unmanaged type: ")
            .AppendUnmanaged(new RegisteredUnmanagedType { Value = 42 })
            .Log();

        _log.Warn()
            .Append("Unregistered unmanaged type: ")
            .AppendUnmanaged(new UnregisteredUnmanagedType { Value = 42 })
            .Log();

        AddExpectation("Registered unmanaged type: foo 42 bar");
        AddExpectation("Unregistered unmanaged type: Unmanaged(0x2a00000000000000)");

        _log.Info("-----");

        UseSpanOverloadWithEncoding(Encoding.Default);
        UseSpanOverloadWithEncoding(Encoding.UTF8);
        UseSpanOverloadWithEncoding(Encoding.Unicode);
        UseSpanOverloadWithEncoding(Encoding.BigEndianUnicode);
        UseSpanOverloadWithEncoding(Encoding.UTF32);
        UseSpanOverloadWithEncoding(Encoding.ASCII);
        UseSpanOverloadWithEncoding(Encoding.Latin1);

        AddExpectation("Uses Span overload with Encoding.Default: True");
        AddExpectation("Uses Span overload with Encoding.UTF8: True");
        AddExpectation("Uses Span overload with Encoding.ASCII: True");
        AddExpectation("Uses Span overload with Encoding.Latin1: True");

        _log.Info("-----");

        UseSpanOverloadWithEncoding(new UTF8Encoding());
        UseSpanOverloadWithEncoding(new UnicodeEncoding());
        UseSpanOverloadWithEncoding(new UTF32Encoding());
        UseSpanOverloadWithEncoding(new ASCIIEncoding());

        AddExpectation("Uses Span overload with new UTF8Encoding(): True");
        AddExpectation("Uses Span overload with new ASCIIEncoding(): True");

        _log.Info("-----");

        UseSpanOverloadWithTextWriter(new StringWriter());
        UseSpanOverloadWithTextWriter(new StreamWriter(new MemoryStream()));

        _log.Info("-----");
        _log.Info("Goodbye!");

        return;

        static void WriteEnumInfo<TEnum>(string label, TEnum value)
            where TEnum : struct, Enum
        {
            _log.Warn($"{label}: {value}");

            _log.Warn()
                .Append(label)
                .Append(" (structured)")
                .AppendKeyValue(typeof(TEnum).Name, value)
                .Log();
        }

        static void UseSpanOverloadWithEncoding(Encoding encoding, [CallerArgumentExpression(nameof(encoding))] string? arg = null)
            => _log.Warn($"Uses Span overload with {arg}: {StreamAppender.UseSpanGetBytesOverload(encoding)}");

        static void UseSpanOverloadWithTextWriter(TextWriter textWriter, [CallerArgumentExpression(nameof(textWriter))] string? arg = null)
            => _log.Warn($"Uses Span overload with {arg}: {TextWriterAppender.UseSpanGetBytesOverload(textWriter)}");

        void AddExpectation(string expectation)
            => expectations.Add(expectation);
    }

    private enum RegisteredEnumWithGeneric
    {
        Foo,
        Bar,
        Baz
    }

    private enum RegisteredEnumWithTypeof
    {
        Fizz,
        Buzz,
        FizzBuzz
    }

    private enum UnregisteredEnum
    {
        Foo,
        Bar,
        Baz
    }

    private struct RegisteredUnmanagedType : ISpanFormattable
    {
        public long Value;

        public string ToString(string? format, IFormatProvider? formatProvider)
            => throw new InvalidOperationException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            => Format(Value, destination, out charsWritten, format, provider);

        public static bool Format(long value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            "foo ".CopyTo(destination);
            destination = destination[4..];
            value.TryFormat(destination, out charsWritten, format, provider);
            destination = destination[charsWritten..];
            " bar".CopyTo(destination);
            charsWritten += 8;
            return true;
        }
    }

    private struct UnregisteredUnmanagedType : ISpanFormattable
    {
        public long Value;

        public string ToString(string? format, IFormatProvider? formatProvider)
            => throw new InvalidOperationException();

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
            => RegisteredUnmanagedType.Format(Value, destination, out charsWritten, format, provider);
    }
}
