using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZeroLog.Formatting;

/// <summary>
/// Writes log message data based on the provided pattern.
/// </summary>
/// <remarks>
/// <para>
/// The pattern is a string containing placeholders:
/// <list type="table">
/// <item><term><c>%date</c></term><description>The message UTC date (recommended, default format: <c>yyyy-MM-dd</c>).</description></item>
/// <item><term><c>%localDate</c></term><description>The message local date (default format: <c>yyyy-MM-dd</c>).</description></item>
/// <item><term><c>%time</c></term><description>The message time of day in UTC (default format: <c>hh\:mm\:ss\.fffffff</c>).</description></item>
/// <item><term><c>%localTime</c></term><description>The message time of day converted to the local time zone (default format: <c>hh\:mm\:ss\.fffffff</c>).</description></item>
/// <item><term><c>%thread</c></term><description>The thread name (or ID) which logged the message.</description></item>
/// <item><term><c>%threadId</c></term><description>The thread ID which logged the message.</description></item>
/// <item><term><c>%threadName</c></term><description>The thread name which logged the message, or empty if the thread was unnamed.</description></item>
/// <item><term><c>%level</c></term><description>The log level from <see cref="LogLevels"/> (a short uppercase label by default, specify the <c>pad</c> format to make every label the same width).</description></item>
/// <item><term><c>%levelColor</c></term><description>The log level ANSI color code from <see cref="LogLevelColors"/>.</description></item>
/// <item><term><c>%logger</c></term><description>The logger name.</description></item>
/// <item><term><c>%loggerCompact</c></term><description>The logger name, with the namespace shortened to its initials.</description></item>
/// <item><term><c>%message</c></term><description>The log message.</description></item>
/// <item><term><c>%exceptionMessage</c></term><description>The exception message, if any.</description></item>
/// <item><term><c>%exceptionType</c></term><description>The exception type name, if any.</description></item>
/// <item><term><c>%newline</c></term><description>Equivalent to <c>Environment.NewLine</c>.</description></item>
/// <item><term><c>%column</c></term><description>Inserts padding spaces until the column index specified in the format string is reached.</description></item>
/// <item><term><c>%color</c></term><description>An ANSI color code (SGR).</description></item>
/// <item><term><c>%resetColor</c></term><description>The reset ANSI code.</description></item>
/// <item><term><c>%%</c></term><description>Inserts a single '%' character (escaping).</description></item>
/// </list>
/// </para>
/// <para>
/// Placeholders can be surrounded with braces and specify an optional format string:
/// <c>%{date:yyyy-MM-dd HH:mm:ss}</c> for instance.
/// </para>
/// <para>
/// Format strings can also be used to set a minimum field length: <c>%{logger:20}</c> will always be at least 20 characters wide.
/// </para>
/// </remarks>
public sealed partial class PatternWriter
{
    // NOTE: This class is immutable after initialization.

    // lang=regex
    private const string _placeholderRegexPattern =
        """
        %(?:
            (?<type>\w+|%)
            |
            \{
                \s* (?<type>\w+) \s*
                (?:
                    : \s* (?<format>.*?) \s*
                )?
            \}
        )
        """;

    private const RegexOptions _placeholderRegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;

#if NET7_0_OR_GREATER
    [GeneratedRegex(_placeholderRegexPattern, _placeholderRegexOptions)]
    private static partial Regex PlaceholderRegex();
#else
    private static readonly Regex _placeholderRegex = new(_placeholderRegexPattern, RegexOptions.Compiled | _placeholderRegexOptions);
    private static Regex PlaceholderRegex() => _placeholderRegex;
#endif

#if NET
    private static readonly LogLevelNames _defaultLogLevelNames = DefaultStyle.Defaults.LogLevelNames;
    private static readonly LogLevelColorCodes _defaultLogLevelColorCodes = DefaultStyle.Defaults.LogLevelColorCodes;
#else
    private static readonly LogLevelNames _defaultLogLevelNames = default;
    private static readonly LogLevelColorCodes _defaultLogLevelColorCodes = default;
#endif

    private readonly PatternPart[] _parts;

    private readonly LogLevelNames _levelNames;
    private readonly LogLevelNames _levelNamesWithPadding;
    private readonly LogLevelColorCodes _levelColors;

    /// <summary>
    /// The pattern to be rendered.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// The log level names to use for the <c>%level</c> placeholder.
    /// </summary>
    public LogLevelNames LogLevels
    {
        get => _levelNames;
        init
        {
            _levelNames = !value.IsEmpty ? value : _defaultLogLevelNames;
            _levelNamesWithPadding = _levelNames.ToPadded();
            HasAnsiCodes = EvaluateHasAnsiCodes();
        }
    }

    /// <summary>
    /// The log level color codes to use for the <c>%levelColor</c> placeholder.
    /// </summary>
    public LogLevelColorCodes LogLevelColors
    {
        get => _levelColors;
        init
        {
            _levelColors = value;
            HasAnsiCodes = EvaluateHasAnsiCodes();
        }
    }

    internal bool HasMessage { get; }
    internal bool HasAnsiCodes { get; private init; }

    internal TimeZoneInfo? LocalTimeZone { get; init; } // For unit tests

    /// <summary>
    /// Creates a pattern writer for the provided pattern.
    /// </summary>
    /// <param name="pattern">The pattern to use.</param>
    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
    public PatternWriter([Pattern] string? pattern)
    {
        Pattern = pattern ?? string.Empty;
        _parts = OptimizeParts(ParsePattern(Pattern)).ToArray();

        LogLevels = _defaultLogLevelNames;
        LogLevelColors = _defaultLogLevelColorCodes;

        HasMessage = _parts.Any(p => p.Type == PatternPartType.Message);
    }

    /// <summary>
    /// Returns true if the provided pattern is valid.
    /// </summary>
    /// <remarks>
    /// This method allocates.
    /// </remarks>
    /// <param name="pattern">The pattern to validate.</param>
    public static bool IsValidPattern(string? pattern)
    {
        if (pattern is null)
            return false;

        try
        {
            _ = new PatternWriter(pattern);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Escapes the provided value so it can be used in a pattern without triggering placeholders.
    /// </summary>
    /// <remarks>
    /// This replaces <c>%</c> with <c>%%</c>.
    /// This method allocates.
    /// </remarks>
    /// <param name="value">The value to escape.</param>
    public static string EscapePattern(string? value)
        => value?.Replace("%", "%%") ?? string.Empty;

    private static IEnumerable<PatternPart> ParsePattern(string pattern)
    {
        var position = 0;
        var matches = PlaceholderRegex().Matches(pattern);

        foreach (Match? match in matches)
        {
            if (position < match!.Index)
                yield return new PatternPart(pattern.Substring(position, match.Index - position));

            var placeholderType = match.Groups["type"].Value;

            var format = match.Groups["format"] is { Success: true } formatGroup
                ? formatGroup.Value
                : null;

            // ReSharper disable StringLiteralTypo

            var part = placeholderType.ToLowerInvariant() switch
            {
                "date"             => new PatternPart(PatternPartType.Date, format),
                "localdate"        => new PatternPart(PatternPartType.LocalDate, format),
                "time"             => new PatternPart(PatternPartType.Time, format),
                "localtime"        => new PatternPart(PatternPartType.LocalTime, format),
                "thread"           => new PatternPart(PatternPartType.Thread, format),
                "threadid"         => new PatternPart(PatternPartType.ThreadId, format),
                "threadname"       => new PatternPart(PatternPartType.ThreadName, format),
                "level"            => new PatternPart(PatternPartType.Level, format),
                "levelcolor"       => new PatternPart(PatternPartType.LevelColor, format),
                "logger"           => new PatternPart(PatternPartType.Logger, format),
                "loggercompact"    => new PatternPart(PatternPartType.LoggerCompact, format),
                "message"          => new PatternPart(PatternPartType.Message, format),
                "exceptionmessage" => new PatternPart(PatternPartType.ExceptionMessage, format),
                "exceptiontype"    => new PatternPart(PatternPartType.ExceptionType, format),
                "newline"          => new PatternPart(PatternPartType.NewLine, format),
                "resetcolor"       => new PatternPart(PatternPartType.ResetColor, format),
                "column"           => new PatternPart(PatternPartType.Column, format),
                "color"            => new PatternPart(PatternPartType.Color, format),
                "%"                => new PatternPart("%"),
                _                  => throw new FormatException($"Invalid placeholder type: %{placeholderType}")
            };

            // ReSharper restore StringLiteralTypo

            yield return ValidatePart(part, placeholderType);

            position = match.Index + match.Length;
        }

        if (position < pattern.Length)
            yield return new PatternPart(pattern.Substring(position, pattern.Length - position));
    }

    private static string ConvertConstantPlaceholders(string? pattern)
        => string.Join("", ParsePattern(pattern ?? string.Empty).Where(i => i.Type == PatternPartType.String).Select(i => i.Format));

    private static PatternPart ValidatePart(PatternPart part, string placeholderType)
    {
        switch (part.Type)
        {
            case PatternPartType.String:
            {
                break;
            }

            case PatternPartType.Date:
            case PatternPartType.LocalDate:
            {
                if (part.Format is not null)
                {
                    _ = new DateTime(2020, 01, 01, 03, 04, 05, 06).ToString(part.Format, CultureInfo.InvariantCulture);
                    return part;
                }

                return new PatternPart(part.Type, "yyyy-MM-dd", null);
            }

            case PatternPartType.Time:
            case PatternPartType.LocalTime:
            {
                if (part.Format is not null)
                {
                    _ = new TimeSpan(0, 01, 02, 03, 04).ToString(part.Format, CultureInfo.InvariantCulture);
                    return part;
                }

                return new PatternPart(part.Type, @"hh\:mm\:ss\.fffffff", null);
            }

            case PatternPartType.Level:
            {
                if (part.Format?.ToLowerInvariant() is "pad" or "padded")
                    return new PatternPart(PatternPartType.LevelPadded);

                goto default;
            }

            case PatternPartType.NewLine:
            {
                RequireFormat(false);
                return new PatternPart(Environment.NewLine);
            }

            case PatternPartType.ResetColor:
            {
                RequireFormat(false);
                return new PatternPart(AnsiColorCodes.Reset);
            }

            case PatternPartType.Column:
            {
                RequireFormat(true);
                goto default;
            }

            case PatternPartType.Color:
            {
                RequireFormat(true);

                if (!AnsiColorCodes.TryParseSGR(part.Format, out var code))
                    throw new FormatException($"Could not parse color field format: {part.Format}");

                return new PatternPart(code);
            }

            default:
            {
                if (part is { Format: not null, FormatInt: null })
                    throw new FormatException($"Invalid format string for the %{placeholderType} placeholder: {part.Format}");

                break;
            }
        }

        return part;

        void RequireFormat(bool shouldHaveFormat)
        {
            switch (shouldHaveFormat)
            {
                case true when part.Format is null:
                    throw new FormatException($"The %{placeholderType} placeholder requires a format string.");

                case false when part.Format is not null:
                    throw new FormatException($"The %{placeholderType} placeholder does not support format strings.");
            }
        }
    }

    private static IEnumerable<PatternPart> OptimizeParts(IEnumerable<PatternPart> parts)
    {
        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            switch (part.Type)
            {
                case PatternPartType.String:
                    sb.Append(part.Format);
                    break;

                default:
                    if (sb.Length != 0)
                    {
                        yield return new PatternPart(sb.ToString());
                        sb.Clear();
                    }

                    yield return part;
                    break;
            }
        }

        if (sb.Length != 0)
            yield return new PatternPart(sb.ToString());
    }

#if NET
    /// <summary>
    /// Writes the log message data to the provided destination.
    /// </summary>
    /// <param name="message">The message to use.</param>
    /// <param name="destination">The destination span to render to.</param>
    /// <param name="charsWritten">Returns the number of chars written to the <paramref name="destination"/>.</param>
    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    public void Write(LoggedMessage message, Span<char> destination, out int charsWritten)
    {
        var builder = new CharBufferBuilder(destination);
        var visibleLength = 0; // Used for the %column placeholder

        foreach (ref readonly var part in _parts.AsSpan())
        {
            var partStartOffset = builder.Length;
            var partVisibleStartOffset = visibleLength;

            switch (part.Type)
            {
                case PatternPartType.String:
                {
                    if (!builder.TryAppendPartial(part.Format))
                        goto endOfLoop;

                    visibleLength += part.FormatInt.GetValueOrDefault();
                    continue;
                }

                case PatternPartType.Date:
                {
                    if (!builder.TryAppend(message.Timestamp, part.Format))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.LocalDate:
                {
                    if (!builder.TryAppend(ToLocalDate(message.Timestamp), part.Format))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.Time:
                {
                    if (!builder.TryAppend(message.Timestamp.TimeOfDay, part.Format))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.LocalTime:
                {
                    if (!builder.TryAppend(ToLocalDate(message.Timestamp).TimeOfDay, part.Format))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.Thread:
                {
                    var thread = message.Thread;

                    if (thread != null)
                    {
                        if (thread.Name != null)
                        {
                            if (!builder.TryAppendPartial(thread.Name))
                                goto endOfLoop;
                        }
                        else
                        {
                            if (!builder.TryAppend(thread.ManagedThreadId))
                                goto endOfLoop;
                        }
                    }
                    else
                    {
                        if (!builder.TryAppend('0'))
                            goto endOfLoop;
                    }

                    break;
                }

                case PatternPartType.ThreadId:
                {
                    if (!builder.TryAppend(message.Thread?.ManagedThreadId ?? 0))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.ThreadName:
                {
                    if (message.Thread?.Name is { } threadName)
                    {
                        if (!builder.TryAppendPartial(threadName))
                            goto endOfLoop;
                    }

                    break;
                }

                case PatternPartType.Level:
                {
                    if (!TryWriteLevel(message.Level, _levelNames, ref builder, ref visibleLength))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.LevelColor:
                {
                    if (!TryWriteLevel(message.Level, _levelColors.Values, ref builder, ref visibleLength))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.LevelPadded:
                {
                    if (!TryWriteLevel(message.Level, _levelNamesWithPadding, ref builder, ref visibleLength))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.Logger:
                {
                    if (!builder.TryAppendPartial(message.LoggerName))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.LoggerCompact:
                {
                    if (!builder.TryAppendPartial(message.LoggerCompactName))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.Message:
                {
                    if (!builder.TryAppendPartial(message.Message))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.ExceptionMessage:
                {
                    if (message.Exception?.Message is { } exceptionMessage)
                    {
                        if (!builder.TryAppendPartial(exceptionMessage))
                            goto endOfLoop;
                    }

                    break;
                }

                case PatternPartType.ExceptionType:
                {
                    if (message.Exception is { } exception)
                    {
                        if (!builder.TryAppendPartial(exception.GetType().Name))
                            goto endOfLoop;
                    }

                    break;
                }

                case PatternPartType.Column:
                {
                    if (part.FormatInt is { } column && column > visibleLength)
                        visibleLength += builder.TryAppendPartial(' ', column - visibleLength);

                    continue;
                }
            }

            // Most of the part types don't output ANSI codes, so we add the full output length to the visible length by default.
            // Parts which can contain ANSI codes adjust the visible length accordingly, so adding this value gives the correct result.
            visibleLength += builder.Length - partStartOffset;
            var visiblePartLength = visibleLength - partVisibleStartOffset;

            if (part.FormatInt is { } fieldLength && fieldLength > visiblePartLength)
                visibleLength += builder.TryAppendPartial(' ', fieldLength - visiblePartLength);
        }

        endOfLoop:

        charsWritten = builder.Length;
        return;

        static bool TryWriteLevel(LogLevel level,
                                  in LogLevelNames values,
                                  ref CharBufferBuilder builder,
                                  ref int visibleLength)
        {
            var value = values[level];

            if (!builder.TryAppendWhole(value))
                return false;

            // This adds an offset which will be adjusted later.
            visibleLength += values.GetVisibleLength(level) - value.Length;
            return true;
        }
    }

    private DateTime ToLocalDate(DateTime value)
        => LocalTimeZone is not null ? TimeZoneInfo.ConvertTimeFromUtc(value, LocalTimeZone) : value.ToLocalTime();

#endif

    private bool EvaluateHasAnsiCodes()
        => AnsiColorCodes.HasAnsiCode(Pattern)
           || _parts.Any(p => p.Type is PatternPartType.Color or PatternPartType.ResetColor)
           || _parts.Any(p => p.Type == PatternPartType.LevelColor) && LogLevelColors.Values.HasAnsiColorCodes()
           || _parts.Any(p => p.Type is PatternPartType.Level or PatternPartType.LevelPadded) && LogLevels.HasAnsiColorCodes();

    /// <summary>
    /// Returns this writer with any ANSI color codes removed.
    /// </summary>
    public PatternWriter WithoutAnsiColorCodes()
    {
        var pattern = PlaceholderRegex().Replace(
            AnsiColorCodes.RemoveAnsiCodes(Pattern),
            match => match.Groups["type"].Value.ToLowerInvariant() is "resetcolor" or "levelcolor" or "color"
                ? string.Empty
                : match.Value
        );

        return new PatternWriter(pattern)
        {
            LogLevels = LogLevels.WithoutAnsiColorCodes(),
            LogLevelColors = LogLevelColors.WithoutAnsiColorCodes(),
            LocalTimeZone = LocalTimeZone
        };
    }

    private enum PatternPartType
    {
        String,
        Date,
        LocalDate,
        Time,
        LocalTime,
        Thread,
        ThreadId,
        ThreadName,
        Level,
        LevelColor,
        LevelPadded,
        Logger,
        LoggerCompact,
        Message,
        ExceptionMessage,
        ExceptionType,
        NewLine,
        ResetColor,
        Color,
        Column
    }

    private readonly struct PatternPart
    {
        public PatternPartType Type { get; }
        public string? Format { get; }
        public int? FormatInt { get; }

        public PatternPart(PatternPartType type, string? format = null)
        {
            Type = type;
            Format = format;
            FormatInt = int.TryParse(Format, NumberStyles.Integer, CultureInfo.InvariantCulture, out var formatInt) && formatInt >= 0 ? formatInt : null;
        }

        public PatternPart(PatternPartType type, string? format, int? formatInt)
        {
            Type = type;
            Format = format;
            FormatInt = formatInt;
        }

        public PatternPart(string value)
        {
            Type = PatternPartType.String;
            Format = value;
            FormatInt = AnsiColorCodes.GetVisibleTextLength(value);
        }
    }

    /// <summary>
    /// The log level names to use for the <c>%level</c> placeholder.
    /// </summary>
    public readonly struct LogLevelNames
    {
        private readonly string[]? _names;
        private readonly int[]? _visibleLengths;

        internal bool IsEmpty => _names is null;

        /// <summary>The name of the Trace level.</summary>
        public string Trace => this[LogLevel.Trace];

        /// <summary>The name of the Debug level.</summary>
        public string Debug => this[LogLevel.Debug];

        /// <summary>The name of the Info level.</summary>
        public string Info => this[LogLevel.Info];

        /// <summary>The name of the Warn level.</summary>
        public string Warn => this[LogLevel.Warn];

        /// <summary>The name of the Error level.</summary>
        public string Error => this[LogLevel.Error];

        /// <summary>The name of the Fatal level.</summary>
        public string Fatal => this[LogLevel.Fatal];

        /// <summary>
        /// Creates a new instance of <see cref="LogLevelNames"/>.
        /// </summary>
        public LogLevelNames(string trace, string debug, string info, string warn, string error, string fatal)
        {
            _names = [trace, debug, info, warn, error, fatal];
            _visibleLengths = new int[_names.Length];

            for (var i = 0; i < _names.Length; ++i)
            {
                _names[i] = ConvertConstantPlaceholders(_names[i]);
                _visibleLengths[i] = AnsiColorCodes.GetVisibleTextLength(_names[i]);
            }
        }

        /// <summary>
        /// Returns the name to use for the given log level.
        /// </summary>
        public string this[LogLevel level]
            => (uint)level < _names?.Length ? _names[(int)level] : string.Empty;

        internal int GetVisibleLength(LogLevel level)
            => (uint)level < _visibleLengths?.Length ? _visibleLengths[(int)level] : 0;

        internal LogLevelNames ToPadded()
        {
            if (_names is null)
                return this;

            var maxVisibleLength = _names.Max(AnsiColorCodes.GetVisibleTextLength);

            return new LogLevelNames(
                Pad(Trace),
                Pad(Debug),
                Pad(Info),
                Pad(Warn),
                Pad(Error),
                Pad(Fatal)
            );

            string Pad(string value)
            {
                var visibleLength = AnsiColorCodes.GetVisibleTextLength(value);
                var padLength = maxVisibleLength - visibleLength;
                return value.PadRight(value.Length + padLength);
            }
        }

        internal bool HasAnsiColorCodes()
            => _names is not null && _names.Any(static i => AnsiColorCodes.HasAnsiCode(i));

        internal LogLevelNames WithoutAnsiColorCodes()
        {
            if (_names is null)
                return this;

            return new LogLevelNames(
                AnsiColorCodes.RemoveAnsiCodes(Trace),
                AnsiColorCodes.RemoveAnsiCodes(Debug),
                AnsiColorCodes.RemoveAnsiCodes(Info),
                AnsiColorCodes.RemoveAnsiCodes(Warn),
                AnsiColorCodes.RemoveAnsiCodes(Error),
                AnsiColorCodes.RemoveAnsiCodes(Fatal)
            );
        }
    }

    /// <summary>
    /// The log level ANSI color code to use for the <c>%levelColor</c> placeholder.
    /// </summary>
    public readonly struct LogLevelColorCodes
    {
        internal readonly LogLevelNames Values;

        /// <summary>
        /// Creates a new instance of <see cref="LogLevelColors"/> with foreground colors from <see cref="ConsoleColor"/>.
        /// </summary>
        public LogLevelColorCodes(ConsoleColor trace, ConsoleColor debug, ConsoleColor info, ConsoleColor warn, ConsoleColor error, ConsoleColor fatal)
            : this(
                AnsiColorCodes.SGR(trace),
                AnsiColorCodes.SGR(debug),
                AnsiColorCodes.SGR(info),
                AnsiColorCodes.SGR(warn),
                AnsiColorCodes.SGR(error),
                AnsiColorCodes.SGR(fatal)
            )
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogLevelColors"/> with foreground and background colors from <see cref="ConsoleColor"/>.
        /// </summary>
        public LogLevelColorCodes(ConsoleColor traceForeground,
                                  ConsoleColor traceBackground,
                                  ConsoleColor debugForeground,
                                  ConsoleColor debugBackground,
                                  ConsoleColor infoForeground,
                                  ConsoleColor infoBackground,
                                  ConsoleColor warnForeground,
                                  ConsoleColor warnBackground,
                                  ConsoleColor errorForeground,
                                  ConsoleColor errorBackground,
                                  ConsoleColor fatalForeground,
                                  ConsoleColor fatalBackground)
            : this(
                AnsiColorCodes.SGR(traceForeground, traceBackground),
                AnsiColorCodes.SGR(debugForeground, debugBackground),
                AnsiColorCodes.SGR(infoForeground, infoBackground),
                AnsiColorCodes.SGR(warnForeground, warnBackground),
                AnsiColorCodes.SGR(errorForeground, errorBackground),
                AnsiColorCodes.SGR(fatalForeground, fatalBackground)
            )
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="LogLevelColors"/> with custom ANSI codes.
        /// </summary>
        public LogLevelColorCodes(string trace, string debug, string info, string warn, string error, string fatal)
            : this(new LogLevelNames(trace, debug, info, warn, error, fatal))
        {
        }

        private LogLevelColorCodes(LogLevelNames values)
            => Values = values;

        /// <summary>
        /// Returns the code to use for the given log level.
        /// </summary>
        public string this[LogLevel level]
            => Values[level];

        internal LogLevelColorCodes WithoutAnsiColorCodes()
            => new(Values.WithoutAnsiColorCodes());
    }

    /// <summary>
    /// Marks a parameter as a pattern string for compile-time validation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class PatternAttribute : Attribute;
}
