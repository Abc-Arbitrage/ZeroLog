using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZeroLog.Formatting;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
internal static partial class AnsiColorCodes
{
    // https://en.wikipedia.org/wiki/ANSI_escape_code

    public const string Reset = "\e[0m";

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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static string SGR(params Code[] codes)
        => $"\e[{string.Join(";", codes.Select(i => i.Value))}m";

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static string SGR(ColorType colorType, Color color)
        => SGR((colorType, color));

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static string SGR(ConsoleColor foreground, ConsoleColor? background = null)
    {
        return background is null
            ? SGR(GetCode(foreground, true))
            : SGR(GetCode(foreground, true), GetCode(background.GetValueOrDefault(), false));

        static Code GetCode(ConsoleColor consoleColor, bool foreground)
        {
            return consoleColor switch
            {
                ConsoleColor.Black       => Get(Color.Black, false),
                ConsoleColor.DarkRed     => Get(Color.Red, false),
                ConsoleColor.DarkGreen   => Get(Color.Green, false),
                ConsoleColor.DarkYellow  => Get(Color.Yellow, false),
                ConsoleColor.DarkBlue    => Get(Color.Blue, false),
                ConsoleColor.DarkMagenta => Get(Color.Magenta, false),
                ConsoleColor.DarkCyan    => Get(Color.Cyan, false),
                ConsoleColor.DarkGray    => Get(Color.White, false),

                ConsoleColor.Gray    => Get(Color.Gray, true),
                ConsoleColor.Red     => Get(Color.Red, true),
                ConsoleColor.Green   => Get(Color.Green, true),
                ConsoleColor.Yellow  => Get(Color.Yellow, true),
                ConsoleColor.Blue    => Get(Color.Blue, true),
                ConsoleColor.Magenta => Get(Color.Magenta, true),
                ConsoleColor.Cyan    => Get(Color.Cyan, true),
                ConsoleColor.White   => Get(Color.White, true),

                _ => new Code(0)
            };

            Code Get(Color color, bool bright) => (GetColorType(foreground, bright), color);
        }
    }

    internal static bool TryParseSgrCode(string? input, out Code value)
    {
        if (input is null or "")
        {
            value = default;
            return false;
        }

        var parts = input.Trim().ToLowerInvariant().Split([" "], StringSplitOptions.RemoveEmptyEntries);

        if (parts is [var singlePart])
        {
            if (byte.TryParse(singlePart, out var byteValue))
            {
                value = new Code(byteValue);
                return true;
            }

            if (Enum.TryParse<Attribute>(singlePart, true, out var attribute))
            {
                value = attribute;
                return true;
            }
        }

        bool? bright = null;
        bool? foreground = null;
        Color? color = null;

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
                    if (color is not null)
                        goto invalid;

                    if (!Enum.TryParse<Color>(part, true, out var parsedColor))
                        goto invalid;

                    color = parsedColor;
                    break;
            }
        }

        if (color is null)
            goto invalid;

        var colorType = GetColorType(foreground ?? true, bright ?? false);
        value = Combine(colorType, color.GetValueOrDefault());
        return true;

        invalid:
        value = default;
        return false;
    }

    private static ColorType GetColorType(bool foreground, bool bright)
        => (foreground, bright) switch
        {
            (true, false)  => ColorType.Foreground,
            (true, true)   => ColorType.ForegroundBright,
            (false, false) => ColorType.Background,
            (false, true)  => ColorType.BackgroundBright
        };

    public static Code Combine(ColorType colorType, Color color)
        => new((byte)((byte)colorType + (byte)color));

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Attribute : byte
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

    public enum ColorType : byte
    {
        Foreground = 30,
        ForegroundBright = 90,

        Background = 40,
        BackgroundBright = 100
    }

    public enum Color : byte
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

    public readonly struct Code(byte value)
    {
        public byte Value => value;

        public static implicit operator Code(Attribute i) => new((byte)i);
        public static implicit operator Code((ColorType colorType, Color color) i) => Combine(i.colorType, i.color);

        public override string ToString() => string.Empty; // Avoid accidental output
    }
}
