using System;
using System.Globalization;
using System.Runtime.InteropServices;
using ZeroLog.Configuration;

namespace ZeroLog;

unsafe partial class LogMessage
{
    private bool TryWriteChain(InternalApi.Chain? chain,
                               ref byte* dataPointer,
                               Span<char> outputBuffer,
                               ZeroLogConfiguration config,
                               FormatType formatType,
                               out int charsWritten)
    {
        if (chain is null)
            goto truncate;

        var inputData = new ReadOnlySpan<byte>(dataPointer, (int)(_dataPointer - dataPointer));
        var context = new InternalApi.ChainExecutionContext(this, inputData, outputBuffer, formatType, config);

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
        ref partial struct ChainExecutionContext
        {
            private readonly LogMessage _logMessage;
            private readonly ZeroLogConfiguration _config;
            private readonly FormatType _formatType;

            internal ReadOnlySpan<byte> InputData;
            internal Span<char> OutputBuffer;
            internal bool Truncated = false;
            private ReadOnlySpan<char> _format;

            internal ChainExecutionContext(LogMessage logMessage,
                                           ReadOnlySpan<byte> inputData,
                                           Span<char> outputBuffer,
                                           FormatType formatType,
                                           ZeroLogConfiguration config)
            {
                _logMessage = logMessage;
                _config = config;
                _formatType = formatType;

                InputData = inputData;
                OutputBuffer = outputBuffer;
            }

            private void ResetStatus()
            {
                _format = ReadOnlySpan<char>.Empty;
            }

            public partial void OutputString()
            {
                if (Truncated)
                    return;

                if (InputData.Length < sizeof(byte))
                    goto truncate;

                var stringIndex = InputData[0];
                InputData = InputData.Slice(sizeof(byte));

                if (stringIndex >= _logMessage._objects.Length)
                    goto truncate;

                OutputString(_logMessage._objects[stringIndex] as string);
                return;

                truncate:
                Truncated = true;
            }

            public partial void OutputString(string? value)
            {
                if (Truncated)
                    return;

                value ??= _config.NullDisplayString ?? string.Empty;

                if (value.Length > 0)
                {
                    var charsToCopy = Math.Min(value.Length, OutputBuffer.Length);
                    value.AsSpan().CopyTo(OutputBuffer.Slice(0, charsToCopy));

                    OutputBuffer = OutputBuffer.Slice(charsToCopy);

                    if (charsToCopy < value.Length)
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

                // ref readonly var value = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(InputData));

                if (typeof(T) == typeof(bool) ? MemoryMarshal.Read<bool>(InputData).TryFormat(OutputBuffer, out var charsWritten)
                    : typeof(T) == typeof(byte) ? MemoryMarshal.Read<byte>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(sbyte) ? MemoryMarshal.Read<sbyte>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(short) ? MemoryMarshal.Read<short>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(ushort) ? MemoryMarshal.Read<ushort>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(int) ? MemoryMarshal.Read<int>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(uint) ? MemoryMarshal.Read<uint>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(long) ? MemoryMarshal.Read<long>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(ulong) ? MemoryMarshal.Read<ulong>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(nint) ? MemoryMarshal.Read<nint>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(nuint) ? MemoryMarshal.Read<nuint>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(float) ? MemoryMarshal.Read<float>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(double) ? MemoryMarshal.Read<double>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(decimal) ? MemoryMarshal.Read<decimal>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(Guid) ? MemoryMarshal.Read<Guid>(InputData).TryFormat(OutputBuffer, out charsWritten, _format)
                    : typeof(T) == typeof(DateTime) ? MemoryMarshal.Read<DateTime>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(TimeSpan) ? MemoryMarshal.Read<TimeSpan>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(DateOnly) ? MemoryMarshal.Read<DateOnly>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(TimeOnly) ? MemoryMarshal.Read<TimeOnly>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(DateTimeOffset) ? MemoryMarshal.Read<DateTimeOffset>(InputData).TryFormat(OutputBuffer, out charsWritten, _format, CultureInfo.InvariantCulture)
                    : typeof(T) == typeof(char) && TryFormatChar(OutputBuffer, MemoryMarshal.Read<char>(InputData), out charsWritten))
                {
                    InputData = InputData.Slice(sizeof(T));
                    OutputBuffer = OutputBuffer.Slice(charsWritten);
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

            public partial void SetFormatString(string? format)
            {
                _format = _formatType is FormatType.Formatted
                    ? format
                    : ReadOnlySpan<char>.Empty;
            }

            public partial void ReadFormatString()
            {
                if (Truncated)
                    return;

                if (InputData.Length < sizeof(byte))
                    goto truncate;

                var stringIndex = InputData[0];
                InputData = InputData.Slice(sizeof(byte));

                if (stringIndex >= _logMessage._objects.Length)
                    goto truncate;

                _format = _formatType is FormatType.Formatted
                    ? _logMessage._objects[stringIndex] as string
                    : ReadOnlySpan<char>.Empty;

                return;

                truncate:
                Truncated = true;
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

            var context = new InternalApi.ChainExecutionContext(
                logMessage,
                input,
                output,
                default,
                config
            );

            var chain = new WarmupChain();
            chain.CallExecute(ref context);
        }

        protected override void Execute(ref InternalApi.ChainExecutionContext context)
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
