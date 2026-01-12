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
        internal static DefaultStyle Default => Simple;

        /// <summary>
        /// A simple default style: timestamp, level, logger name, thread, and message.
        /// </summary>
        public static DefaultStyle Simple => field ??= new("%time - %{level:pad} - %logger || %message");

        /// <summary>
        /// A simple default style: timestamp, level, logger name, thread, and message.
        /// </summary>
        public static DefaultStyle SimpleWithThread => field ??= new("%time - %{level:pad} - %logger (%thread) || %message");
    }

    /// <summary>
    /// Default styles with ANSI color codes.
    /// Good for logging to consoles supporting ANSI colors.
    /// </summary>
    public static class Colored
    {
        internal static DefaultStyle Default => BlueAndWhite;

        /// <summary>
        /// A simple style similar to <see cref="NoColor.SimpleWithThread"/> which colors the full line based on the log level.
        /// </summary>
        public static DefaultStyle FullLine => field ??= new("%resetColor%levelColor%time - %{level:pad} - %logger (%thread) || %message");

        /// <summary>
        /// A style which highlights the level, logger, and message.
        /// </summary>
        public static DefaultStyle BlueAndWhite => field ??= new(
            new PatternWriter(
                $$"""
                %resetColor%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%resetColor  %{level:pad}%resetColor  {{D.Logger}}%logger%resetColor (%thread) || {{D.HighlightedMessage}}%message%resetColor
                """
            ) { LogLevels = Defaults.BoldLogLevelNames }
        );

        /// <summary>
        /// Similar to <see cref="BlueAndWhite"/>, but also colors the message text.
        /// </summary>
        public static DefaultStyle ColoredMessage => field ??= new(
            new PatternWriter(
                $$"""
                %resetColor%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%resetColor  %{level:pad}%resetColor  {{D.Logger}}%logger%resetColor (%thread) || %levelColor%message%resetColor
                """
            )
            {
                LogLevels = Defaults.BoldLogLevelNames,
                LogLevelColors = Defaults.BoldLogLevelColorCodes
            }
        );

        /// <summary>
        /// Compact logger name, with a padded message column.
        /// </summary>
        public static DefaultStyle ShortLoggerWithPadding => field ??= new(
            new PatternWriter(
                    $$"""
                    %resetColor%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%resetColor  %{level:pad}%resetColor  {{D.Logger}}%loggerCompact%resetColor %{column:60}|| %levelColor%message%resetColor
                    """
                )
                { LogLevels = Defaults.BoldLogLevelNames }
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
        public const string ColorDebug = C.DefaultForeground;
        public const string ColorInfo = C.ForegroundBrightWhite;
        public const string ColorWarn = C.ForegroundBrightYellow;
        public const string ColorError = C.ForegroundBrightRed;
        public const string ColorFatal = C.ForegroundBrightMagenta;

        public const string DarkTimestamp = C.ForegroundBrightBlack;
        public const string Logger = C.ForegroundBrightBlue;
        public const string HighlightedMessage = "\e[1;97m";

        public static readonly PatternWriter.LogLevelNames LogLevelNames = new(NameTrace, NameDebug, NameInfo, NameWarn, NameError, NameFatal);
        public static readonly PatternWriter.LogLevelColorCodes LogLevelColorCodes = new(ColorTrace, ColorDebug, ColorInfo, ColorWarn, ColorError, ColorFatal);

        public static readonly PatternWriter.LogLevelNames BoldLogLevelNames = new(
            $"{ColorTrace}{NameTrace}",
            $"{C.Bold}{ColorDebug}{NameDebug}",
            $"{C.Bold}{ColorInfo}{NameInfo}",
            $"{C.Bold}{ColorWarn}{NameWarn}",
            $"{C.Bold}{ColorError}{NameError}",
            $"{C.Bold}{ColorFatal}{NameFatal}"
        );

        public static readonly PatternWriter.LogLevelColorCodes BoldLogLevelColorCodes = new(
            $"{ColorTrace}",
            $"{ColorDebug}",
            $"{C.Bold}{ColorInfo}",
            $"{C.Bold}{ColorWarn}",
            $"{C.Bold}{ColorError}",
            $"{C.Bold}{ColorFatal}"
        );
    }
}
