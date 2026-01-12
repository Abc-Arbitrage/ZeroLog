using System;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Examples;

internal static class Program
{
    private static readonly Log _log = LogManager.GetLogger(typeof(Program));
    private static readonly Log _logOther = LogManager.GetLogger(typeof(Program).FullName + ".SomeOtherLog");

    private static readonly ConsoleAppender _appender = new() { ColorOutput = true };

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Thread.CurrentThread.Name = "Main thread";

        using var session = InitializeLogManager();
        Utils.WriteHeader();

        ShowDefaultStyles();
        ShowCustomStyle();
    }

    private static IDisposable InitializeLogManager()
    {
        // Example of a simple configuration

        return LogManager.Initialize(new ZeroLogConfiguration
        {
            AutoRegisterEnums = true,
            RootLogger =
            {
                Level = LogLevel.Trace,
                Appenders = { _appender }
            }
        });
    }

    private static void ShowDefaultStyles()
    {
        Utils.SectionTitle($"Styles provided in the {nameof(DefaultStyle)} class");

        foreach (var (style, name) in Utils.GetDefaultStyles())
        {
            Utils.ItemTitle(name);

            _appender.Formatter = style.Formatter;
            LogExampleMessages();
        }
    }

    private static void ShowCustomStyle()
    {
        Utils.SectionTitle("Custom styles");

        ShowCustomStyleWithTheDefaultFormatter();
        ShowCustomStyleWithACustomFormatter();
    }

    private static void ShowCustomStyleWithTheDefaultFormatter()
    {
        Utils.ItemTitle($"Custom pattern with the {nameof(DefaultFormatter)}");

        _appender.Formatter = new DefaultFormatter
        {
            // See: https://en.wikipedia.org/wiki/ANSI_escape_code#Colors
            MessagePatternWriter = new PatternWriter("%resetColor%level %{localDate:HH:mm}\e[90m%{localDate::ss.ffff} %levelColor\e[1m%message%{column:50}\e[0;90m from %loggerCompact")
            {
                LogLevels = new PatternWriter.LogLevelNames(
                    "🔎",
                    "🐛",
                    "ℹ️",
                    "⚠️",
                    "❌",
                    "💀"
                ),
                LogLevelColors = new PatternWriter.LogLevelColorCodes(
                    ConsoleColor.Gray,
                    ConsoleColor.DarkGray,
                    ConsoleColor.Blue,
                    ConsoleColor.DarkYellow,
                    ConsoleColor.DarkRed,
                    ConsoleColor.Magenta
                )
            },
            JsonSeparator = " with data: "
        };

        LogExampleMessages();
    }

    private static void ShowCustomStyleWithACustomFormatter()
    {
        Utils.ItemTitle("Entirely custom formatter");

        _appender.Formatter = new CustomFormatter();
        LogExampleMessages();
    }

    private static void LogExampleMessages()
    {
        _log.Trace().Append("Example trace message").AppendKeyValue("Foo", "Bar").Log();
        _log.Debug().Append("Example debug message").AppendKeyValue("Foo", "Bar").AppendKeyValue("Hello", "World").Log();
        _log.Info().Append("Example information message").AppendKeyValue("Foo", "Bar").Log();
        _logOther.Warn().Append("Example warning message").AppendKeyValue("Foo", "Bar").Log();
        _log.Error().Append("Example error message").AppendKeyValue("Foo", "Bar").Log();
        _log.Fatal().Append("Example fatal message").AppendKeyValue("Foo", "Bar").Log();

        Utils.NewLine();
        _log.Info("Example information message without structured data");
        _log.Error("Example error message with exception", Utils.GetExceptionWithStackTrace());
        Utils.NewLine();
    }

    private sealed class CustomFormatter : Formatter
    {
        private static readonly PatternWriter.LogLevelNames _levelIcons = new(
            "🔎",
            "🐛",
            "ℹ️",
            "⚠️",
            "❌",
            "💀"
        );

        protected override void WriteMessage(LoggedMessage message)
        {
            // This allows for full output customization

            Write(_levelIcons[message.Level]);
            Write(": ");
            Write(message.Message);

            if (message.KeyValues.Count != 0)
            {
                foreach (var (key, value) in message.KeyValues)
                {
                    WriteLine();
                    Write("      - ");
                    Write(key);
                    Write(" = ");
                    Write(value);
                }
            }

            if (message.Exception != null)
            {
                WriteLine();
                Write("    Exception: ");
                Write(message.Exception.Message);
            }

            WriteLine();
        }
    }
}
