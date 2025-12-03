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
/// <item><term><c>%level</c></term><description>The log level (a short uppercase label by default, specify the <c>pad</c> format to make every label the same width).</description></item>
/// <item><term><c>%logger</c></term><description>The logger name.</description></item>
/// <item><term><c>%loggerCompact</c></term><description>The logger name, with the namespace shortened to its initials.</description></item>
/// <item><term><c>%newline</c></term><description>Equivalent to <c>Environment.NewLine</c>.</description></item>
/// <item><term><c>%column</c></term><description>Inserts padding spaces until the column index specified in the format string is reached.</description></item>
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
public sealed class PatternWriter
{
    private static readonly Regex _patternRegex = new(
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
        """,
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
    );

    private readonly PatternPart[] _parts;

    private string[] _levels = [];
    private string[] _levelsWithPadding = [];

    /// <summary>
    /// The pattern to be rendered.
    /// </summary>
    public string Pattern { get; }

    internal TimeZoneInfo? LocalTimeZone { get; init; } // For unit tests

    /// <summary>
    /// Creates a pattern writer for the provided pattern.
    /// </summary>
    /// <param name="pattern">The pattern to use.</param>
    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
    public PatternWriter(string pattern)
    {
        Pattern = pattern;
        _parts = OptimizeParts(ParsePattern(pattern)).ToArray();

        SetLevelNames("TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL");
    }

    /// <summary>
    /// Returns true if the provided pattern is valid.
    /// </summary>
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
    /// </remarks>
    /// <param name="value">The value to escape.</param>
    public static string EscapePattern(string? value)
        => value?.Replace("%", "%%") ?? string.Empty;

    /// <summary>
    /// Sets the labels to use for each log level.
    /// </summary>
    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract")]
    public void SetLevelNames(string trace, string debug, string info, string warn, string error, string fatal)
    {
        _levels = [trace, debug, info, warn, error, fatal];
        for (var i = 0; i < _levels.Length; ++i)
            _levels[i] ??= string.Empty;

        var maxLength = _levels.Max(i => i.Length);
        _levelsWithPadding = _levels.Select(i => i.PadRight(maxLength)).ToArray();
    }

    private static IEnumerable<PatternPart> ParsePattern(string pattern)
    {
        var position = 0;

        var matches = _patternRegex.Matches(pattern);

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
                "date"          => new PatternPart(PatternPartType.Date, format),
                "localdate"     => new PatternPart(PatternPartType.LocalDate, format),
                "time"          => new PatternPart(PatternPartType.Time, format),
                "localtime"     => new PatternPart(PatternPartType.LocalTime, format),
                "thread"        => new PatternPart(PatternPartType.Thread, format),
                "threadid"      => new PatternPart(PatternPartType.ThreadId, format),
                "threadname"    => new PatternPart(PatternPartType.ThreadName, format),
                "level"         => new PatternPart(PatternPartType.Level, format),
                "logger"        => new PatternPart(PatternPartType.Logger, format),
                "loggercompact" => new PatternPart(PatternPartType.LoggerCompact, format),
                "newline"       => new PatternPart(PatternPartType.NewLine, format),
                "column"        => new PatternPart(PatternPartType.Column, format),
                "%"             => new PatternPart("%"),
                _               => throw new FormatException($"Invalid placeholder type: %{placeholderType}")
            };

            // ReSharper restore StringLiteralTypo

            yield return ValidatePart(part, placeholderType);

            position = match.Index + match.Length;
        }

        if (position < pattern.Length)
            yield return new PatternPart(pattern.Substring(position, pattern.Length - position));
    }

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
                if (part.Format is not null)
                    throw new FormatException($"The %{placeholderType} placeholder does not support format strings.");

                break;
            }

            case PatternPartType.Column:
            {
                if (part.Format is null)
                    throw new FormatException($"The %{placeholderType} placeholder requires a format string.");

                goto default;
            }

            default:
            {
                if (part is { Format: not null, FormatInt: null })
                    throw new FormatException($"Invalid format string for the %{placeholderType} placeholder: {part.Format}");

                break;
            }
        }

        return part;
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

                case PatternPartType.NewLine:
                    sb.Append(Environment.NewLine);
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

        foreach (var part in _parts)
        {
            var partStartOffset = builder.Length;

            switch (part.Type)
            {
                case PatternPartType.String:
                {
                    if (!builder.TryAppendPartial(part.Format))
                        goto endOfLoop;

                    break;
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
                    var levels = _levels;
                    var intLevel = (int)message.Level;

                    if (intLevel < 0 || intLevel >= levels.Length)
                        break;

                    if (!builder.TryAppendWhole(levels[intLevel]))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.LevelPadded:
                {
                    var levels = _levelsWithPadding;
                    var intLevel = (int)message.Level;

                    if (intLevel < 0 || intLevel >= levels.Length)
                        break;

                    if (!builder.TryAppendWhole(levels[intLevel]))
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

                case PatternPartType.Column:
                {
                    if (part.FormatInt is { } column)
                        builder.TryAppendPartial(' ', column - builder.Length);

                    continue;
                }
            }

            if (part.FormatInt is { } fieldLength)
                builder.TryAppendPartial(' ', fieldLength - builder.Length + partStartOffset);
        }

        endOfLoop:

        charsWritten = builder.Length;
    }

    private DateTime ToLocalDate(DateTime value)
        => LocalTimeZone is not null ? TimeZoneInfo.ConvertTimeFromUtc(value, LocalTimeZone) : value.ToLocalTime();

#endif

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
        LevelPadded,
        Logger,
        LoggerCompact,
        NewLine,
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
        }
    }
}
