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
                               out int charsWritten)
    {
        if (chain is null)
            goto truncate;

        var inputData = new ReadOnlySpan<byte>(dataPointer, (int)(_dataPointer - dataPointer));
        var context = new InternalApi.ChainExecutionContext(inputData, outputBuffer, config);

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
            private readonly ZeroLogConfiguration _config;
            internal ReadOnlySpan<byte> InputData;
            internal Span<char> OutputBuffer;
            internal bool Truncated = false;
            private ReadOnlySpan<char> _format;

            internal ChainExecutionContext(ReadOnlySpan<byte> inputData, Span<char> outputBuffer, ZeroLogConfiguration config)
            {
                InputData = inputData;
                OutputBuffer = outputBuffer;
                _config = config;
            }

            public partial void OutputString()
            {
                _ = Truncated;
                _format = "";
            }

            public partial void OutputString(string? value)
            {
                value ??= _config.NullDisplayString ?? string.Empty;

                var charsToCopy = Math.Min(value.Length, OutputBuffer.Length);
                value.AsSpan().CopyTo(OutputBuffer.Slice(0, charsToCopy));

                OutputBuffer = OutputBuffer.Slice(charsToCopy);

                if (charsToCopy < value.Length)
                    Truncated = true;
            }

            public partial void OutputValueType<T>(bool nullable)
                where T : unmanaged
            {
                if (nullable)
                {
                    if (InputData.Length < sizeof(byte))
                    {
                        Truncated = true;
                        return;
                    }

                    var hasValue = InputData[0] != 0;
                    InputData = InputData.Slice(sizeof(byte));

                    if (!hasValue)
                        return;
                }

                if (InputData.Length < sizeof(T))
                {
                    Truncated = true;
                    return;
                }

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
                }
                else
                {
                    Truncated = true;
                }

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
        }
    }
}
