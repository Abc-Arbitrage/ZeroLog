using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZeroLog.Formatting;

internal class PrefixWriter
{
    private static readonly string[] _levelStrings = Enum.GetNames(typeof(LogLevel)).Select(x => x.ToUpperInvariant()).ToArray();

    private readonly PatternPart[] _parts;

    public string Pattern { get; }

    public PrefixWriter(string pattern)
    {
        Pattern = pattern;
        _parts = OptimizeParts(ParsePattern(pattern)).ToArray();
    }

    private static IEnumerable<PatternPart> ParsePattern(string pattern)
    {
        var position = 0;

        foreach (Match? match in Regex.Matches(pattern, @"%(?:(?<part>\w+)|\{\s*(?<part>\w+)\s*\})", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
        {
            if (position < match!.Index)
                yield return new PatternPart(pattern.Substring(position, match.Index - position));

            yield return match.Groups["part"].Value.ToLowerInvariant() switch
            {
                "date"   => new PatternPart(PatternPartType.Date),
                "time"   => new PatternPart(PatternPartType.Time),
                "thread" => new PatternPart(PatternPartType.Thread),
                "level"  => new PatternPart(PatternPartType.Level),
                "logger" => new PatternPart(PatternPartType.Logger),
                _        => new PatternPart(match.Value)
            };

            position = match.Index + match.Length;
        }

        if (position < pattern.Length)
            yield return new PatternPart(pattern.Substring(position, pattern.Length - position));
    }

    private static IEnumerable<PatternPart> OptimizeParts(IEnumerable<PatternPart> parts)
    {
        var currentString = string.Empty;

        foreach (var part in parts)
        {
            if (part.Type == PatternPartType.String)
            {
                currentString += part.Value;
            }
            else
            {
                if (currentString.Length != 0)
                {
                    yield return new PatternPart(currentString);
                    currentString = string.Empty;
                }

                yield return part;
            }
        }

        if (currentString.Length != 0)
            yield return new PatternPart(currentString);
    }

    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    public int WritePrefix(FormattedLogMessage message, Span<char> buffer)
    {
        var builder = new CharBufferBuilder(buffer);

        foreach (var part in _parts)
        {
            switch (part.Type)
            {
                case PatternPartType.String:
                {
                    if (!builder.TryAppendPartial(part.Value))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.Date:
                {
                    if (!builder.TryAppend(message.Timestamp, "yyyy-MM-dd"))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.Time:
                {
                    if (!builder.TryAppend(message.Timestamp.TimeOfDay, @"hh\:mm\:ss\.fffffff"))
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
                    if (!builder.TryAppendWhole(_levelStrings[(byte)message.Level]))
                        goto endOfLoop;

                    break;
                }

                case PatternPartType.Logger:
                {
                    if (!builder.TryAppendPartial(message.LoggerName))
                        goto endOfLoop;

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        endOfLoop:

        return builder.Length;
    }

    private enum PatternPartType
    {
        String,
        Date,
        Time,
        Thread,
        Level,
        Logger
    }

    private readonly struct PatternPart
    {
        public PatternPartType Type { get; }
        public string? Value { get; }

        public PatternPart(PatternPartType type)
        {
            Type = type;
            Value = null;
        }

        public PatternPart(string value)
        {
            Type = PatternPartType.String;
            Value = value;
        }
    }
}
