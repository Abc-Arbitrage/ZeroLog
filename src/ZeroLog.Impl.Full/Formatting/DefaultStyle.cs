using System;

namespace ZeroLog.Formatting;

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
        public static DefaultStyle FullLine => field ??= new("%{resetColor}%{levelColor}%{time} - %{level:pad} - %logger (%thread) || %{message}%{resetColor}");

        /// <summary>
        /// A style which highlights the level, logger, and message.
        /// </summary>
        public static DefaultStyle BlueAndWhite => field ??= new(
            new PatternWriter(
                $$"""
                %{resetColor}%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%{resetColor}  %{level:pad}  {{D.Logger}}%{logger}%{resetColor} (%thread) || {{D.HighlightedMessage}}%{message}%{resetColor}
                """
            ) { LogLevels = Defaults.BoldLogLevelNames }
        );

        /// <summary>
        /// Similar to <see cref="BlueAndWhite"/>, but also colors the message text.
        /// </summary>
        public static DefaultStyle ColoredMessage => field ??= new(
            new PatternWriter(
                $$"""
                %{resetColor}%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%{resetColor}  %{level:pad}  {{D.Logger}}%{logger}%{resetColor} (%thread) || %{levelColor}%{message}%{resetColor}
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
                    %{resetColor}%{time:hh\:mm\:ss}{{D.DarkTimestamp}}%{time:\.fff}%{resetColor}  %{level:pad}  {{D.Logger}}%{loggerCompact}%{resetColor} %{column:60}|| %{levelColor}%{message}%{resetColor}
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

        public const string ColorTrace = "%{color:gray}";
        public const string ColorDebug = "%{color:default foreground}";
        public const string ColorInfo = "%{color:bright white}";
        public const string ColorWarn = "%{color:bright yellow}";
        public const string ColorError = "%{color:bright red}";
        public const string ColorFatal = "%{color:bright magenta}";

        public const string DarkTimestamp = "%{color:gray}";
        public const string Logger = "%{color:reset, bright blue}";
        public const string HighlightedMessage = "%{color:bold, bright white}";

        public static readonly PatternWriter.LogLevelNames LogLevelNames = new(NameTrace, NameDebug, NameInfo, NameWarn, NameError, NameFatal);
        public static readonly PatternWriter.LogLevelColorCodes LogLevelColorCodes = new(ColorTrace, ColorDebug, ColorInfo, ColorWarn, ColorError, ColorFatal);

        public static readonly PatternWriter.LogLevelNames BoldLogLevelNames = new(
            $"{ColorTrace}{NameTrace}",
            $"{Bold(ColorDebug)}{NameDebug}",
            $"{Bold(ColorInfo)}{NameInfo}",
            $"{Bold(ColorWarn)}{NameWarn}",
            $"{Bold(ColorError)}{NameError}",
            $"{Bold(ColorFatal)}{NameFatal}"
        );

        public static readonly PatternWriter.LogLevelColorCodes BoldLogLevelColorCodes = new(
            ColorTrace,
            ColorDebug,
            Bold(ColorInfo),
            Bold(ColorWarn),
            Bold(ColorError),
            Bold(ColorFatal)
        );

        private static string Bold(string placeholder)
            => placeholder.Contains("bold", StringComparison.OrdinalIgnoreCase)
                ? placeholder
                : placeholder.Replace("%{color:", "%{color:bold, ", StringComparison.OrdinalIgnoreCase);
    }
}
