namespace ZeroLog.Formatting;

using C = AnsiColorCodes;
using D = DefaultStyle.Defaults;

/// <summary>
/// A default output style shipped with ZeroLog.
/// See the inner types to choose an instance.
/// </summary>
public sealed class DefaultStyle
{
    /// <summary>
    /// The formatter to use for this style.
    /// </summary>
    public Formatter Formatter { get; }

    private DefaultStyle([PatternWriter.Pattern] string pattern)
        : this(new DefaultFormatter(pattern))
    {
    }

    private DefaultStyle(PatternWriter patternWriter)
        : this(new DefaultFormatter(patternWriter))
    {
    }

    private DefaultStyle(DefaultFormatter formatter)
    {
        Formatter = formatter;
    }

    /// <inheritdoc/>
    public override string ToString()
        => Formatter.ToString() ?? string.Empty;

    /// <summary>
    /// Default styles without colors. Good for logging to files.
    /// </summary>
    public static class NoColor
    {
        /// <summary>
        /// A simple default style: timestamp, level, logger name, and message.
        /// </summary>
        public static DefaultStyle Simple => field ??= new("%time - %{level:pad} - %logger || %message");
    }

    /// <summary>
    /// Default styles with ANSI color codes.
    /// Good for logging to consoles supporting ANSI colors.
    /// </summary>
    public static class Colored
    {
        /// <summary>
        /// A simple style which colors the full line based on the log level.
        /// </summary>
        public static DefaultStyle FullLine => field ??= new("%resetColor%levelColor%time - %{level:pad} - %logger || %message");

        /// <summary>
        /// A style which highlights the level, logger, and message.
        /// </summary>
        public static DefaultStyle Highlighted => field ??= new(
            new PatternWriter(
                $$"""
                %resetColor%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%resetColor - %{level:pad}%resetColor - {{D.Logger}}%logger%resetColor || {{D.HighlightedMessage}}%message%resetColor
                """
            ) { LogLevels = Defaults.HighlightedLogLevelNames }
        );

        /// <summary>
        /// Similar to <see cref="Highlighted"/>, but also highlights the message text.
        /// </summary>
        public static DefaultStyle HighlightedWithMessage => field ??= new(
            new PatternWriter(
                $$"""
                %resetColor%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%resetColor - %{level:pad}%resetColor - {{D.Logger}}%logger%resetColor || %levelColor{{C.Bold}}%message%resetColor
                """
            ) { LogLevels = Defaults.HighlightedLogLevelNames }
        );
    }

    internal static class Defaults
    {
        public const string NameTrace = "TRACE";
        public const string NameDebug = "DEBUG";
        public const string NameInfo = "INFO";
        public const string NameWarn = "WARN";
        public const string NameError = "ERROR";
        public const string NameFatal = "FATAL";

        public const string ColorTrace = C.ForegroundBrightBlack;
        public const string ColorDebug = C.ForegroundWhite;
        public const string ColorInfo = C.ForegroundBrightWhite;
        public const string ColorWarn = C.ForegroundBrightYellow;
        public const string ColorError = C.ForegroundBrightRed;
        public const string ColorFatal = C.ForegroundBrightMagenta;

        public const string DarkTimestamp = C.ForegroundBrightBlack;
        public const string Logger = C.ForegroundCyan;
        public const string HighlightedMessage = "\e[1;97m";

        public static readonly PatternWriter.LogLevelNames LogLevelNames = new(NameTrace, NameDebug, NameInfo, NameWarn, NameError, NameFatal);
        public static readonly PatternWriter.LogLevelColorCodes LogLevelColorCodes = new(ColorTrace, ColorDebug, ColorInfo, ColorWarn, ColorError, ColorFatal);

        public static readonly PatternWriter.LogLevelNames HighlightedLogLevelNames = new(
            $"{ColorTrace}{NameTrace}",
            $"{ColorDebug}{NameDebug}",
            $"{C.Bold}{ColorInfo}{NameInfo}",
            $"{C.Bold}{ColorWarn}{NameWarn}",
            $"{C.Bold}{ColorError}{NameError}",
            $"{C.Bold}{ColorFatal}{NameFatal}"
        );
    }
}
