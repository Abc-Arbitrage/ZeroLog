using System;
using System.Text.RegularExpressions;

namespace ZeroLog.Formatting;

internal static partial class AnsiColorCodes
{
    public const string Reset = "\e[0m";

    // https://en.wikipedia.org/wiki/ANSI_escape_code#Colors

    public const string ForegroundBlack = "\e[30m";
    public const string ForegroundRed = "\e[31m";
    public const string ForegroundGreen = "\e[32m";
    public const string ForegroundYellow = "\e[33m";
    public const string ForegroundBlue = "\e[34m";
    public const string ForegroundMagenta = "\e[35m";
    public const string ForegroundCyan = "\e[36m";
    public const string ForegroundWhite = "\e[37m";

    public const string ForegroundBrightBlack = "\e[90m";
    public const string ForegroundBrightRed = "\e[91m";
    public const string ForegroundBrightGreen = "\e[92m";
    public const string ForegroundBrightYellow = "\e[93m";
    public const string ForegroundBrightBlue = "\e[94m";
    public const string ForegroundBrightMagenta = "\e[95m";
    public const string ForegroundBrightCyan = "\e[96m";
    public const string ForegroundBrightWhite = "\e[97m";

    public const string BackgroundBlack = "\e[40m";
    public const string BackgroundRed = "\e[41m";
    public const string BackgroundGreen = "\e[42m";
    public const string BackgroundYellow = "\e[43m";
    public const string BackgroundBlue = "\e[44m";
    public const string BackgroundMagenta = "\e[45m";
    public const string BackgroundCyan = "\e[46m";
    public const string BackgroundWhite = "\e[47m";

    public const string BackgroundBrightBlack = "\e[100m";
    public const string BackgroundBrightRed = "\e[101m";
    public const string BackgroundBrightGreen = "\e[102m";
    public const string BackgroundBrightYellow = "\e[103m";
    public const string BackgroundBrightBlue = "\e[104m";
    public const string BackgroundBrightMagenta = "\e[105m";
    public const string BackgroundBrightCyan = "\e[106m";
    public const string BackgroundBrightWhite = "\e[107m";

    public static bool UseByDefault { get; } = !Console.IsOutputRedirected && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

#if NET7_0_OR_GREATER
    [GeneratedRegex("""\e\[.*?m""", RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    private static partial Regex AnsiColorsRegex();

    public static string RemoveAnsiCodes(string? input)
        => AnsiColorsRegex().Replace(input ?? string.Empty, string.Empty);
#else
    private static readonly Regex _ansiColorsRegex = new("""\e\[.*?m""", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    public static string RemoveAnsiCodes(string? input)
        => _ansiColorsRegex.Replace(input ?? string.Empty, string.Empty);
#endif

    public static string GetForegroundColorCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black       => ForegroundBlack,
            ConsoleColor.DarkRed     => ForegroundRed,
            ConsoleColor.DarkGreen   => ForegroundGreen,
            ConsoleColor.DarkYellow  => ForegroundYellow,
            ConsoleColor.DarkBlue    => ForegroundBlue,
            ConsoleColor.DarkMagenta => ForegroundMagenta,
            ConsoleColor.DarkCyan    => ForegroundCyan,
            ConsoleColor.DarkGray    => ForegroundWhite,

            ConsoleColor.Gray    => ForegroundBrightBlack,
            ConsoleColor.Red     => ForegroundBrightRed,
            ConsoleColor.Green   => ForegroundBrightGreen,
            ConsoleColor.Yellow  => ForegroundBrightYellow,
            ConsoleColor.Blue    => ForegroundBrightBlue,
            ConsoleColor.Magenta => ForegroundBrightMagenta,
            ConsoleColor.Cyan    => ForegroundBrightCyan,
            ConsoleColor.White   => ForegroundBrightWhite,

            _ => ""
        };
    }

    public static string GetBackgroundColorCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black       => BackgroundBlack,
            ConsoleColor.DarkRed     => BackgroundRed,
            ConsoleColor.DarkGreen   => BackgroundGreen,
            ConsoleColor.DarkYellow  => BackgroundYellow,
            ConsoleColor.DarkBlue    => BackgroundBlue,
            ConsoleColor.DarkMagenta => BackgroundMagenta,
            ConsoleColor.DarkCyan    => BackgroundCyan,
            ConsoleColor.DarkGray    => BackgroundWhite,

            ConsoleColor.Gray    => BackgroundBrightBlack,
            ConsoleColor.Red     => BackgroundBrightRed,
            ConsoleColor.Green   => BackgroundBrightGreen,
            ConsoleColor.Yellow  => BackgroundBrightYellow,
            ConsoleColor.Blue    => BackgroundBrightBlue,
            ConsoleColor.Magenta => BackgroundBrightMagenta,
            ConsoleColor.Cyan    => BackgroundBrightCyan,
            ConsoleColor.White   => BackgroundBrightWhite,

            _ => ""
        };
    }
}
