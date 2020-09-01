using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Formatting;
using System.Text.RegularExpressions;

namespace ZeroLog.Appenders
{
    internal class PrefixWriter
    {
        private const int _maxLength = 512;
        private const string _dateFormat = "yyyy-MM-dd";

        private readonly StringBuffer _stringBuffer = new StringBuffer(_maxLength);
        private readonly byte[] _buffer = new byte[_maxLength * sizeof(char)];
        private readonly char[] _strings;

        private readonly Action<PrefixWriter, ILogEventHeader> _appendMethod;

        public PrefixWriter(string pattern)
        {
            var parts = ParsePattern(pattern).ToList();
            _strings = BuildStrings(parts, out var stringMap);
            _appendMethod = BuildAppendMethod(parts, stringMap);
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

        private static char[] BuildStrings(IEnumerable<PatternPart> parts, out Dictionary<string, (int offset, int length)> map)
        {
            var stringOffsets = new Dictionary<string, (int offset, int length)>();
            var stringsBuilder = new StringBuilder();

            foreach (var part in parts)
            {
                switch (part.Type)
                {
                    case PatternPartType.String:
                        AddString(part.Value!);
                        break;

                    case PatternPartType.Date:
                        AddString(_dateFormat);
                        break;
                }
            }

            void AddString(string value)
            {
                if (stringOffsets.ContainsKey(value))
                    return;

                var offset = stringsBuilder.Length;
                stringsBuilder.Append(value);
                stringOffsets[value] = (offset, value.Length);
            }

            if (stringsBuilder.Length == 0)
                AddString(" ");

            var strings = new char[stringsBuilder.Length];
            stringsBuilder.CopyTo(0, strings, 0, stringsBuilder.Length);
            map = stringOffsets;
            return strings;
        }

        private static Action<PrefixWriter, ILogEventHeader> BuildAppendMethod(ICollection<PatternPart> parts, Dictionary<string, (int offset, int length)> stringMap)
        {
            var method = new DynamicMethod("WritePrefix", typeof(void), new[] { typeof(PrefixWriter), typeof(ILogEventHeader) }, typeof(PrefixWriter), false)
            {
                InitLocals = false
            };

            var il = method.GetILGenerator();

            var stringBufferLocal = il.DeclareLocal(typeof(StringBuffer));
            var stringsLocal = il.DeclareLocal(typeof(char).MakeByRefType(), true);
            var dateTimeLocal = default(LocalBuilder);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(PrefixWriter).GetField(nameof(_stringBuffer), BindingFlags.Instance | BindingFlags.NonPublic)!);
            il.Emit(OpCodes.Stloc, stringBufferLocal);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(PrefixWriter).GetField(nameof(_strings), BindingFlags.Instance | BindingFlags.NonPublic)!);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ldelema, typeof(char));
            il.Emit(OpCodes.Stloc, stringsLocal);

            foreach (var part in parts)
            {
                switch (part.Type)
                {
                    case PatternPartType.String:
                    {
                        // _stringBuffer.Append(&_strings[0] + offset * sizeof(char), length);

                        var (offset, length) = stringMap[part.Value!];

                        il.Emit(OpCodes.Ldloc, stringBufferLocal);

                        il.Emit(OpCodes.Ldloc, stringsLocal);
                        il.Emit(OpCodes.Conv_U);
                        il.Emit(OpCodes.Ldc_I4, offset * sizeof(char));
                        il.Emit(OpCodes.Add);

                        il.Emit(OpCodes.Ldc_I4, length);

                        il.Emit(OpCodes.Call, typeof(StringBuffer).GetMethod(nameof(StringBuffer.Append), new[] { typeof(char*), typeof(int) })!);
                        break;
                    }

                    case PatternPartType.Date:
                    {
                        // _stringBuffer.Append(logEventHeader.Timestamp, new StringView(&_strings[0] + offset * sizeof(char), length));

                        var (offset, length) = stringMap[_dateFormat];

                        il.Emit(OpCodes.Ldloc, stringBufferLocal);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Callvirt, typeof(ILogEventHeader).GetProperty(nameof(ILogEventHeader.Timestamp))?.GetGetMethod()!);

                        il.Emit(OpCodes.Ldloc, stringsLocal);
                        il.Emit(OpCodes.Conv_U);
                        il.Emit(OpCodes.Ldc_I4, offset * sizeof(char));
                        il.Emit(OpCodes.Add);

                        il.Emit(OpCodes.Ldc_I4, length);

                        il.Emit(OpCodes.Newobj, typeof(StringView).GetConstructor(new[] { typeof(char*), typeof(int) })!);

                        il.Emit(OpCodes.Call, typeof(StringBuffer).GetMethod(nameof(StringBuffer.Append), new[] { typeof(DateTime), typeof(StringView) })!);
                        break;
                    }

                    case PatternPartType.Time:
                    {
                        // _stringBuffer.Append(logEventHeader.Timestamp.TimeOfDay, StringView.Empty);

                        il.Emit(OpCodes.Ldloc, stringBufferLocal);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Callvirt, typeof(ILogEventHeader).GetProperty(nameof(ILogEventHeader.Timestamp))?.GetGetMethod()!);
                        il.Emit(OpCodes.Stloc, dateTimeLocal ??= il.DeclareLocal(typeof(DateTime)));
                        il.Emit(OpCodes.Ldloca, dateTimeLocal);
                        il.Emit(OpCodes.Call, typeof(DateTime).GetProperty(nameof(DateTime.TimeOfDay))?.GetGetMethod()!);

                        il.Emit(OpCodes.Ldsfld, typeof(StringView).GetField(nameof(StringView.Empty))!);

                        il.Emit(OpCodes.Call, typeof(StringBuffer).GetMethod(nameof(StringBuffer.Append), new[] { typeof(TimeSpan), typeof(StringView) })!);
                        break;
                    }

                    case PatternPartType.Thread:
                    {
                        // _stringBuffer.Append(logEventHeader.ThreadId, StringView.Empty);

                        il.Emit(OpCodes.Ldloc, stringBufferLocal);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Callvirt, typeof(ILogEventHeader).GetProperty(nameof(ILogEventHeader.ThreadId))?.GetGetMethod()!);

                        il.Emit(OpCodes.Ldsfld, typeof(StringView).GetField(nameof(StringView.Empty))!);

                        il.Emit(OpCodes.Call, typeof(StringBuffer).GetMethod(nameof(StringBuffer.Append), new[] { typeof(int), typeof(StringView) })!);
                        break;
                    }

                    case PatternPartType.Level:
                    {
                        // _stringBuffer.Append(LevelStringCache.GetLevelString(logEventHeader.Level));

                        il.Emit(OpCodes.Ldloc, stringBufferLocal);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Callvirt, typeof(ILogEventHeader).GetProperty(nameof(ILogEventHeader.Level))?.GetGetMethod()!);
                        il.Emit(OpCodes.Call, typeof(LevelStringCache).GetMethod(nameof(LevelStringCache.GetLevelString))!);

                        il.Emit(OpCodes.Call, typeof(StringBuffer).GetMethod(nameof(StringBuffer.Append), new[] { typeof(string) })!);
                        break;
                    }

                    case PatternPartType.Logger:
                    {
                        // _stringBuffer.Append(logEventHeader.Name);

                        il.Emit(OpCodes.Ldloc, stringBufferLocal);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Callvirt, typeof(ILogEventHeader).GetProperty(nameof(ILogEventHeader.Name))?.GetGetMethod()!);

                        il.Emit(OpCodes.Call, typeof(StringBuffer).GetMethod(nameof(StringBuffer.Append), new[] { typeof(string) })!);
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            il.Emit(OpCodes.Ret);

            return (Action<PrefixWriter, ILogEventHeader>)method.CreateDelegate(typeof(Action<PrefixWriter, ILogEventHeader>));
        }

        public unsafe int WritePrefix(Stream stream, ILogEventHeader logEventHeader, Encoding encoding)
        {
            _stringBuffer.Clear();
            _appendMethod(this, logEventHeader);

            int bytesWritten;
            fixed (byte* buf = &_buffer[0])
                bytesWritten = _stringBuffer.CopyTo(buf, _buffer.Length, 0, _stringBuffer.Count, encoding);

            stream.Write(_buffer, 0, bytesWritten);
            return bytesWritten;
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

        private struct PatternPart
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
}
