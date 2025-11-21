using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog.Tests.Aot;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
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
        RunSession(stringBuilder);
        var stringOutput = stringBuilder.ToString();

        Console.WriteLine();
        Console.WriteLine($"{_bold}Results:{_reset}");
        Console.WriteLine();

        var success = true;

        CheckExpectation(!RuntimeFeature.IsDynamicCodeSupported, "RuntimeFeature.IsDynamicCodeSupported is false");
        CheckExpectation(string.IsNullOrEmpty(GetAssemblyLocation()), "typeof(Program).Assembly.Location is empty");

        CheckSubstring("Registered enum: Bar");
        CheckSubstring("""Structured value ~~ { "RegisteredEnum": "Bar" }""");

        CheckSubstring("Unregistered enum: 1");
        CheckSubstring("""Structured value ~~ { "UnregisteredEnum": "1" }""");

        CheckSubstring("Registered unmanaged type: foo 42 bar");
        CheckSubstring("Unregistered unmanaged type: Unmanaged(0x2a00000000000000)");

        CheckSubstring("Uses Span overload with Encoding.Default: True");
        CheckSubstring("Uses Span overload with Encoding.UTF8: True");
        CheckSubstring("Uses Span overload with Encoding.ASCII: True");
        CheckSubstring("Uses Span overload with Encoding.Latin1: True");

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

    private static void RunSession(StringBuilder stringBuilder)
    {
        using var logs = LogManager.Initialize(new ZeroLogConfiguration
        {
            AutoRegisterEnums = true, // Won't work in AOT
            RootLogger =
            {
                Level = LogLevel.Debug,
                Appenders =
                {
                    new ConsoleAppender(),
                    new TextWriterAppender(new StringWriter(stringBuilder))
                }
            }
        });

        LogManager.RegisterEnum<RegisteredEnum>();
        LogManager.RegisterUnmanaged<RegisteredUnmanagedType>();

        _log.Info("Hello, world!");
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

        WriteEnumInfo("System enum", DayOfWeek.Friday);
        WriteEnumInfo("Registered enum", RegisteredEnum.Bar);
        WriteEnumInfo("Unregistered enum", UnregisteredEnum.Bar);

        _log.Info("-----");

        _log.Warn()
            .Append("Registered unmanaged type: ")
            .AppendUnmanaged(new RegisteredUnmanagedType { Value = 42 })
            .Log();

        _log.Warn()
            .Append("Unregistered unmanaged type: ")
            .AppendUnmanaged(new UnregisteredUnmanagedType { Value = 42 })
            .Log();

        _log.Info("-----");

        UseSpanOverloadWithEncoding(Encoding.Default);
        UseSpanOverloadWithEncoding(Encoding.UTF8);
        UseSpanOverloadWithEncoding(Encoding.Unicode);
        UseSpanOverloadWithEncoding(Encoding.BigEndianUnicode);
        UseSpanOverloadWithEncoding(Encoding.UTF32);
        UseSpanOverloadWithEncoding(Encoding.ASCII);
        UseSpanOverloadWithEncoding(Encoding.Latin1);

        _log.Info("-----");

        UseSpanOverloadWithEncoding(new UTF8Encoding());
        UseSpanOverloadWithEncoding(new UnicodeEncoding());
        UseSpanOverloadWithEncoding(new UTF32Encoding());
        UseSpanOverloadWithEncoding(new ASCIIEncoding());

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
                .Append("  Structured value")
                .AppendKeyValue(typeof(TEnum).Name, value)
                .Log();
        }

        static void UseSpanOverloadWithEncoding(Encoding encoding, [CallerArgumentExpression(nameof(encoding))] string? arg = null)
            => _log.Warn($"Uses Span overload with {arg}: {StreamAppender.UseSpanGetBytesOverload(encoding)}");

        static void UseSpanOverloadWithTextWriter(TextWriter textWriter, [CallerArgumentExpression(nameof(textWriter))] string? arg = null)
            => _log.Warn($"Uses Span overload with {arg}: {TextWriterAppender.UseSpanGetBytesOverload(textWriter)}");
    }

    private enum RegisteredEnum
    {
        Foo,
        Bar,
        Baz
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
