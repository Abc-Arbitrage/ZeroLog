using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ZeroLog.Formatting;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
internal static partial class AnsiColorCodes
{
    public const string Reset = "\e[0m";
    public const string Bold = "\e[1m";
    public const string DefaultForeground = "\e[39m";

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

    // lang=regex
    private const string _ansiColorsRegexPattern = """\e\[[0-9;]*m""";
    private const RegexOptions _ansiColorsRegexOptions = RegexOptions.CultureInvariant | RegexOptions.Singleline;

#if NET7_0_OR_GREATER
    [GeneratedRegex(_ansiColorsRegexPattern, _ansiColorsRegexOptions)]
    private static partial Regex AnsiColorsRegex();
#else
    private static readonly Regex _ansiColorsRegex = new(_ansiColorsRegexPattern, RegexOptions.Compiled | _ansiColorsRegexOptions);
    private static Regex AnsiColorsRegex() => _ansiColorsRegex;
#endif

    public static bool HasAnsiCode(string value)
        => value.Contains("\e");

    public static string RemoveAnsiCodes(string? input)
        => AnsiColorsRegex().Replace(input ?? string.Empty, string.Empty);

    public static int GetVisibleTextLength(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        return new StringInfo(RemoveAnsiCodes(input)).LengthInTextElements;
    }

    public static string ConsoleColorToAnsiCode(ConsoleColor foreground)
        => CreateSgrSequence(ConsoleColorToAttribute(foreground, true));

    public static string ConsoleColorToAnsiCode(ConsoleColor foreground, ConsoleColor background)
        => CreateSgrSequence(ConsoleColorToAttribute(foreground, true), ConsoleColorToAttribute(background, false));

    private static byte ConsoleColorToAttribute(ConsoleColor consoleColor, bool foreground)
    {
        return consoleColor switch
        {
            ConsoleColor.Black       => Get(ColorOffset.Black, false),
            ConsoleColor.DarkRed     => Get(ColorOffset.Red, false),
            ConsoleColor.DarkGreen   => Get(ColorOffset.Green, false),
            ConsoleColor.DarkYellow  => Get(ColorOffset.Yellow, false),
            ConsoleColor.DarkBlue    => Get(ColorOffset.Blue, false),
            ConsoleColor.DarkMagenta => Get(ColorOffset.Magenta, false),
            ConsoleColor.DarkCyan    => Get(ColorOffset.Cyan, false),
            ConsoleColor.DarkGray    => Get(ColorOffset.White, false),

            ConsoleColor.Gray    => Get(ColorOffset.Black, true),
            ConsoleColor.Red     => Get(ColorOffset.Red, true),
            ConsoleColor.Green   => Get(ColorOffset.Green, true),
            ConsoleColor.Yellow  => Get(ColorOffset.Yellow, true),
            ConsoleColor.Blue    => Get(ColorOffset.Blue, true),
            ConsoleColor.Magenta => Get(ColorOffset.Magenta, true),
            ConsoleColor.Cyan    => Get(ColorOffset.Cyan, true),
            ConsoleColor.White   => Get(ColorOffset.White, true),

            _ => 0
        };

        byte Get(ColorOffset color, bool bright) => GetColorAttribute(GetColorTypeOffset(foreground, bright), color);
    }

    internal static bool TryParseSgrCode(string? input, out int value)
    {
        if (input is null or "")
        {
            value = 0;
            return false;
        }

        var parts = input.Trim().ToLowerInvariant().Split([" "], StringSplitOptions.RemoveEmptyEntries);

        if (parts is [var singlePart])
        {
            if (byte.TryParse(singlePart, out var byteValue))
            {
                value = byteValue;
                return true;
            }

            if (Enum.TryParse<SgrCode>(singlePart, true, out var sgrCode))
            {
                value = (int)sgrCode;
                return true;
            }
        }

        bool? bright = null;
        bool? foreground = null;
        ColorOffset? colorOffset = null;

        foreach (var part in parts)
        {
            switch (part)
            {
                case "dark":
                    if (bright is not null)
                        goto invalid;

                    bright = false;
                    break;

                case "bright":
                    if (bright is not null)
                        goto invalid;

                    bright = true;
                    break;

                case "fg":
                case "foreground":
                    if (foreground is not null)
                        goto invalid;

                    foreground = true;
                    break;

                case "bg":
                case "background":
                    if (foreground is not null)
                        goto invalid;

                    foreground = false;
                    break;

                default:
                    if (colorOffset is not null)
                        goto invalid;

                    if (!Enum.TryParse<ColorOffset>(part, true, out var parsedColorOffset))
                        goto invalid;

                    colorOffset = parsedColorOffset;
                    break;
            }
        }

        if (colorOffset is null)
            goto invalid;

        var colorTypeOffset = GetColorTypeOffset(foreground ?? true, bright ?? false);
        value = GetColorAttribute(colorTypeOffset, colorOffset.GetValueOrDefault());
        return true;

        invalid:
        value = 0;
        return false;
    }

    private static ColorTypeOffset GetColorTypeOffset(bool foreground, bool bright)
        => (foreground, bright) switch
        {
            (true, false)  => ColorTypeOffset.Foreground,
            (true, true)   => ColorTypeOffset.ForegroundBright,
            (false, false) => ColorTypeOffset.Background,
            (false, true)  => ColorTypeOffset.BackgroundBright
        };

    private static byte GetColorAttribute(ColorTypeOffset colorType, ColorOffset color)
        => (byte)((byte)colorType + (byte)color);

    private static string CreateSgrSequence(params byte[] attributes)
        => $"\e[{string.Join(";", attributes)}m";

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum SgrCode : byte
    {
        Reset = 0,
        Normal = 0,
        Bold = 1,
        Faint = 2,
        Dim = 2,
        Italic = 3,
        Underline = 4,
        Blink = 5,
        Invert = 7,
        Reversed = 7,
        Conceal = 8,
        Hide = 8,
        CrossedOut = 9,
        Strike = 9,
        DoublyUnderlined = 21,
        NormalIntensity = 22,
        NotItalic = 23,
        NotUnderlined = 24,
        NotBlinking = 25,
        NotReversed = 27,
        Reveal = 28,
        NotCrossedOut = 29,
        DefaultForeground = 39,
        DefaultBackground = 49,
    }

    private enum ColorTypeOffset : byte
    {
        OffsetFromForegroundToBackground = 10,
        OffsetFromDarkToBright = 60,

        Foreground = 30,
        ForegroundBright = Foreground + OffsetFromDarkToBright,

        Background = Foreground + OffsetFromForegroundToBackground,
        BackgroundBright = ForegroundBright + OffsetFromForegroundToBackground
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private enum ColorOffset : byte
    {
        Black = 0,
        Red = 1,
        Green = 2,
        Yellow = 3,
        Blue = 4,
        Magenta = 5,
        Cyan = 6,
        White = 7,

        // Alias
        Gray = Black
    }
}
