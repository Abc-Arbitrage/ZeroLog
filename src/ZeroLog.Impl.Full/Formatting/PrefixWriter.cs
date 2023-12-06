using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZeroLog.Formatting;

internal class PrefixWriter
{
    private static readonly Regex _patternRegex = new(
        """
        %(?:
            (?<type>\w+)
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

    public string Pattern { get; }

    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
    public PrefixWriter(string pattern)
    {
        Pattern = pattern;
        _parts = OptimizeParts(ParsePattern(pattern)).ToArray();
    }

    public static bool IsValidPattern(string? pattern)
    {
        if (pattern is null)
            return false;

        try
        {
            _ = new PrefixWriter(pattern);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
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
                "time"          => new PatternPart(PatternPartType.Time, format),
                "thread"        => new PatternPart(PatternPartType.Thread, format),
                "level"         => new PatternPart(PatternPartType.Level, format),
                "logger"        => new PatternPart(PatternPartType.Logger, format),
                "loggercompact" => new PatternPart(PatternPartType.LoggerCompact, format),
                "newline"       => new PatternPart(PatternPartType.NewLine, format),
                "column"        => new PatternPart(PatternPartType.Column, format),
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
            {
                if (part.Format is not null)
                {
                    _ = new DateTime(2020, 01, 01, 03, 04, 05, 06).ToString(part.Format, CultureInfo.InvariantCulture);
                    return part;
                }

                return new PatternPart(PatternPartType.Date, "yyyy-MM-dd", null);
            }

            case PatternPartType.Time:
            {
                if (part.Format is not null)
                {
                    _ = new TimeSpan(0, 01, 02, 03, 04).ToString(part.Format, CultureInfo.InvariantCulture);
                    return part;
                }

                return new PatternPart(PatternPartType.Time, @"hh\:mm\:ss\.fffffff", null);
            }

            case PatternPartType.Level:
            {
                if (part.Format?.ToLowerInvariant() is "pad" or "padded")
                    return new PatternPart(PatternPartType.Level, part.Format, 5);

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

#if NETCOREAPP

    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    public void WritePrefix(LoggedMessage message, Span<char> destination, out int charsWritten)
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

                case PatternPartType.Time:
                {
                    if (!builder.TryAppend(message.Timestamp.TimeOfDay, part.Format))
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

                case PatternPartType.Level:
                {
                    var levelString = message.Level switch
                    {
                        LogLevel.Trace => "TRACE",
                        LogLevel.Debug => "DEBUG",
                        LogLevel.Info  => "INFO",
                        LogLevel.Warn  => "WARN",
                        LogLevel.Error => "ERROR",
                        LogLevel.Fatal => "FATAL",
                        _              => "???"
                    };

                    if (!builder.TryAppendWhole(levelString))
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
                    if (!builder.TryAppendPartial(message.Logger?.CompactName))
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

#endif

    private enum PatternPartType
    {
        String,
        Date,
        Time,
        Thread,
        Level,
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
