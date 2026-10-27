using System;
using System.Globalization;
using System.Runtime.InteropServices;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog;

unsafe partial class LogMessage
{
    private bool TryWriteChain(InternalApi.Chain? chain,
                               ref byte* dataPointer,
                               Span<char> outputBuffer,
                               ZeroLogConfiguration config,
                               FormatType formatType,
                               KeyValueList? keyValueList,
                               out int charsWritten)
    {
        if (chain is null)
            goto truncate;

        var inputData = new ReadOnlySpan<byte>(dataPointer, (int)(_dataPointer - dataPointer));

        var context = new InternalApi.Chain.ChainExecutionContext(
            this,
            inputData,
            outputBuffer,
            formatType,
            config,
            keyValueList
        );

        chain.CallExecute(ref context);

        charsWritten = outputBuffer.Length - context.OutputBuffer.Length;
        dataPointer += inputData.Length - context.InputData.Length;
        return !context.Truncated;

        truncate:
        charsWritten = 0;
        return false;
    }

    private static void SkipChain(ref byte* dataPointer)
    {
        // TODO: Implement skip
        throw new NotImplementedException();
    }

#pragma warning disable CS1591

    partial class InternalApi
    {
        partial class Chain
        {
            ref partial struct ChainExecutionContext
            {
                private readonly LogMessage _logMessage;
                private readonly ZeroLogConfiguration _config;
                private readonly FormatType _formatType;
                private readonly KeyValueList? _keyValueList;

                internal ReadOnlySpan<byte> InputData;
                internal Span<char> OutputBuffer;
                internal bool Truncated = false;
                private ReadOnlySpan<char> _format;
                private string? _key;

                internal ChainExecutionContext(LogMessage logMessage,
                                               ReadOnlySpan<byte> inputData,
                                               Span<char> outputBuffer,
                                               FormatType formatType,
                                               ZeroLogConfiguration config,
                                               KeyValueList? keyValueList)
                {
                    _logMessage = logMessage;
                    _config = config;
                    _formatType = formatType;
                    _keyValueList = keyValueList;

                    InputData = inputData;
                    OutputBuffer = outputBuffer;
                }

                private void ResetStatus()
                {
                    _format = ReadOnlySpan<char>.Empty;
                    _key = null;
                }

                public partial void OutputString()
                {
                    if (TryReadVariableString(out var value))
                        OutputString(value);
                }

                public partial void OutputString(string? value)
                {
                    if (Truncated)
                        return;

                    if (_key is null)
                    {
                        value ??= _config.NullDisplayString ?? string.Empty;
                        var outputBuffer = OutputBuffer;

                        if (value.Length > 0)
                        {
                            var charsToCopy = Math.Min(value.Length, outputBuffer.Length);
                            value.AsSpan().CopyTo(outputBuffer.Slice(0, charsToCopy));

                            if (_key is null)
                                OutputBuffer = outputBuffer.Slice(charsToCopy);

                            if (charsToCopy < value.Length)
                                Truncated = true;
                        }
                    }
                    else if (_keyValueList is not null)
                    {
                        if (!_keyValueList.Add(_key, value))
                            Truncated = true;
                    }

                    ResetStatus();
                }

                public partial void OutputValueType<T>(bool nullable)
                    where T : unmanaged
                {
                    if (Truncated)
                        return;

                    if (nullable)
                    {
                        if (InputData.Length < sizeof(byte))
                            goto truncate;

                        var hasValue = InputData[0];
                        InputData = InputData.Slice(sizeof(byte));

                        switch (hasValue)
                        {
                            case 0:
                                OutputString(null);
                                return;

                            case 1:
                                break;

                            default:
                                goto truncate;
                        }
                    }

                    if (InputData.Length < sizeof(T))
                        goto truncate;

                    var outputBuffer = _key is null
                        ? OutputBuffer
                        : _keyValueList is not null
                            ? _keyValueList.GetRemainingBuffer()
                            : Span<char>.Empty;

                    if (_key is not null)
                    {
                        _format = typeof(T) == typeof(DateTime) ? KeyValueFormatStrings.DateTime
                            : typeof(T) == typeof(TimeSpan) ? KeyValueFormatStrings.TimeSpan
                            : typeof(T) == typeof(DateOnly) ? KeyValueFormatStrings.DateOnly
                            : typeof(T) == typeof(TimeOnly) ? KeyValueFormatStrings.TimeOnly
                            : typeof(T) == typeof(DateTimeOffset) ? KeyValueFormatStrings.DateTimeOffset
                            : ReadOnlySpan<char>.Empty;
                    }

                    if (typeof(T) == typeof(bool) ? MemoryMarshal.Read<bool>(InputData).TryFormat(outputBuffer, out var charsWritten)
                        : typeof(T) == typeof(byte) ? MemoryMarshal.Read<byte>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(sbyte) ? MemoryMarshal.Read<sbyte>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(short) ? MemoryMarshal.Read<short>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(ushort) ? MemoryMarshal.Read<ushort>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(int) ? MemoryMarshal.Read<int>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(uint) ? MemoryMarshal.Read<uint>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(long) ? MemoryMarshal.Read<long>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(ulong) ? MemoryMarshal.Read<ulong>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(nint) ? MemoryMarshal.Read<nint>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(nuint) ? MemoryMarshal.Read<nuint>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(float) ? MemoryMarshal.Read<float>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(double) ? MemoryMarshal.Read<double>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(decimal) ? MemoryMarshal.Read<decimal>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(Guid) ? MemoryMarshal.Read<Guid>(InputData).TryFormat(outputBuffer, out charsWritten, _format)
                        : typeof(T) == typeof(DateTime) ? MemoryMarshal.Read<DateTime>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(TimeSpan) ? MemoryMarshal.Read<TimeSpan>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(DateOnly) ? MemoryMarshal.Read<DateOnly>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(TimeOnly) ? MemoryMarshal.Read<TimeOnly>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(DateTimeOffset) ? MemoryMarshal.Read<DateTimeOffset>(InputData).TryFormat(outputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                        : typeof(T) == typeof(char) && TryFormatChar(outputBuffer, MemoryMarshal.Read<char>(InputData), out charsWritten))
                    {
                        var rawData = InputData.Slice(0, sizeof(T));
                        InputData = InputData.Slice(sizeof(T));

                        if (_key is null)
                        {
                            OutputBuffer = OutputBuffer.Slice(charsWritten);
                        }
                        else
                        {
                            _keyValueList?.Add(_key, ArgumentType.FromValueType<T>(), charsWritten, rawData);
                        }

                        goto success;
                    }

                    goto truncate;

                    success:
                    ResetStatus();
                    return;

                    truncate:
                    Truncated = true;
                    return;

                    static bool TryFormatChar(Span<char> outputBuffer, char value, out int charsWritten)
                    {
                        if (outputBuffer.Length > 0)
                        {
                            outputBuffer[0] = value;
                            charsWritten = 1;
                            return true;
                        }

                        charsWritten = 0;
                        return false;
                    }
                }

                public partial void SetFormatString()
                {
                    if (TryReadVariableString(out var format))
                        SetFormatString(format);
                }

                public partial void SetFormatString(string? format)
                {
                    _format = _formatType is FormatType.Formatted
                        ? format
                        : ReadOnlySpan<char>.Empty;
                }

                public partial void SetKeyString()
                {
                    if (TryReadVariableString(out var key))
                        SetKeyString(key);
                }

                public partial void SetKeyString(string? key)
                {
                    // This is only called from an AppendKeyValue call, so we always set _key to a non-null instance
                    _key = key ?? string.Empty;
                }

                private bool TryReadVariableString(out string? value)
                {
                    value = null;

                    if (Truncated)
                        return false;

                    if (InputData.Length < sizeof(byte))
                        goto truncate;

                    var stringIndex = InputData[0];
                    InputData = InputData.Slice(sizeof(byte));

                    if (stringIndex >= _logMessage._objects.Length)
                        goto truncate;

                    value = _logMessage._objects[stringIndex] as string;

                    if (value is not null)
                        return true;

                    truncate:
                    Truncated = true;
                    return false;
                }
            }
        }
    }

    internal static void Warmup()
        => WarmupChain.PerformWarmup();

    private class WarmupChain : InternalApi.Chain
    {
        public static void PerformWarmup()
        {
            var logMessage = new LogMessage("");
            var config = new ZeroLogConfiguration();

            ReadOnlySpan<byte> input = stackalloc byte[128];
            Span<char> output = stackalloc char[128];

            var context = new ChainExecutionContext(
                logMessage,
                input,
                output,
                default,
                config,
                null
            );

            var chain = new WarmupChain();
            chain.CallExecute(ref context);
        }

        protected override void Execute(ref ChainExecutionContext context)
        {
            // Pre-JIT these

            context.OutputValueType<bool>(false);
            context.OutputValueType<byte>(false);
            context.OutputValueType<sbyte>(false);
            context.OutputValueType<short>(false);
            context.OutputValueType<ushort>(false);
            context.OutputValueType<int>(false);
            context.OutputValueType<uint>(false);
            context.OutputValueType<long>(false);
            context.OutputValueType<ulong>(false);
            context.OutputValueType<nint>(false);
            context.OutputValueType<nuint>(false);
            context.OutputValueType<float>(false);
            context.OutputValueType<double>(false);
            context.OutputValueType<decimal>(false);
            context.OutputValueType<Guid>(false);
            context.OutputValueType<DateTime>(false);
            context.OutputValueType<TimeSpan>(false);
            context.OutputValueType<DateOnly>(false);
            context.OutputValueType<TimeOnly>(false);
            context.OutputValueType<DateTimeOffset>(false);
            context.OutputValueType<char>(false);

            context.OutputString();
            context.OutputString("");
        }
    }
}
