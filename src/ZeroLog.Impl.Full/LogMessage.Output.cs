using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ZeroLog.Configuration;

namespace ZeroLog;

unsafe partial class LogMessage
{
    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    internal int WriteTo(Span<char> outputBuffer, ZeroLogConfiguration config, bool skipFormat = false, KeyValuePointerBuffer? keyValueBuffer = null)
    {
        keyValueBuffer?.Init(_strings);

        if (ConstantMessage is not null)
        {
            var length = Math.Min(ConstantMessage.Length, outputBuffer.Length);
            ConstantMessage.AsSpan(0, length).CopyTo(outputBuffer);
            return length;
        }

        var dataPointer = _startOfBuffer;
        var endOfData = _dataPointer;
        var bufferIndex = 0;

        while (dataPointer < endOfData)
        {
            if (keyValueBuffer != null)
            {
                var argType = *(ArgumentType*)dataPointer;
                if (argType == ArgumentType.KeyString) // KeyString never has a format flag
                    keyValueBuffer.AddKeyPointer(dataPointer);
            }

            var isTruncated = !TryWriteArg(ref dataPointer, outputBuffer.Slice(bufferIndex), out var charsWritten, skipFormat, config);
            bufferIndex += charsWritten;

            if (isTruncated)
                goto outputTruncated;
        }

        if (_isTruncated)
            goto outputTruncated;

        return bufferIndex;

        outputTruncated:
        {
            var suffix = config.TruncatedMessageSuffix;

            if (bufferIndex + suffix.Length <= outputBuffer.Length)
            {
                // The suffix fits in the remaining buffer.
                suffix.CopyTo(outputBuffer.Slice(bufferIndex));
                return bufferIndex + suffix.Length;
            }

            var idx = outputBuffer.Length - suffix.Length;

            if (idx >= 0)
            {
                // The suffix fits at the end of the buffer, but overwrites output data.
                suffix.CopyTo(outputBuffer.Slice(idx));
            }
            else
            {
                // The suffix is too large to fit in the buffer.
                suffix.AsSpan(0, outputBuffer.Length).CopyTo(outputBuffer);
            }

            return outputBuffer.Length;
        }
    }

    private bool TryWriteArg(ref byte* dataPointer, Span<char> outputBuffer, out int charsWritten, bool skipFormat, ZeroLogConfiguration config)
    {
        var argType = *(ArgumentType*)dataPointer;
        dataPointer += sizeof(ArgumentType);

        var format = default(string);

        if ((argType & ArgumentType.FormatFlag) != 0)
        {
            argType &= ~ArgumentType.FormatFlag;

            var stringIndex = *dataPointer;
            ++dataPointer;

            if (!skipFormat)
                format = _strings[stringIndex];
        }

        switch (argType)
        {
            case ArgumentType.None:
            {
                charsWritten = 0;
                return true;
            }

            case ArgumentType.String:
            {
                var stringIndex = *dataPointer;
                ++dataPointer;

                var value = _strings[stringIndex] ?? string.Empty;

                if (value.Length <= outputBuffer.Length)
                {
                    value.CopyTo(outputBuffer);
                    charsWritten = value.Length;
                }
                else
                {
                    var length = outputBuffer.Length;
                    value.AsSpan(0, length).CopyTo(outputBuffer);
                    charsWritten = length;
                    return false;
                }

                return true;
            }

            case ArgumentType.Null:
            {
                var value = config.NullDisplayString;

                if (value.Length <= outputBuffer.Length)
                {
                    value.CopyTo(outputBuffer);
                    charsWritten = value.Length;
                }
                else
                {
                    charsWritten = 0;
                    return false;
                }

                return true;
            }

            case ArgumentType.Boolean:
            {
                var valuePtr = (bool*)dataPointer;
                dataPointer += sizeof(bool);

                return valuePtr->TryFormat(outputBuffer, out charsWritten);
            }

            case ArgumentType.Byte:
            {
                var valuePtr = dataPointer;
                dataPointer += sizeof(byte);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.SByte:
            {
                var valuePtr = (sbyte*)dataPointer;
                dataPointer += sizeof(sbyte);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Char:
            {
                var valuePtr = (char*)dataPointer;
                dataPointer += sizeof(char);

                if (outputBuffer.Length < 1)
                {
                    charsWritten = 0;
                    return false;
                }

                outputBuffer[0] = *valuePtr;
                charsWritten = 1;
                return true;
            }

            case ArgumentType.Int16:
            {
                var valuePtr = (short*)dataPointer;
                dataPointer += sizeof(short);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UInt16:
            {
                var valuePtr = (ushort*)dataPointer;
                dataPointer += sizeof(ushort);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Int32:
            {
                var valuePtr = (int*)dataPointer;
                dataPointer += sizeof(int);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UInt32:
            {
                var valuePtr = (uint*)dataPointer;
                dataPointer += sizeof(uint);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Int64:
            {
                var valuePtr = (long*)dataPointer;
                dataPointer += sizeof(long);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UInt64:
            {
                var valuePtr = (ulong*)dataPointer;
                dataPointer += sizeof(ulong);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.IntPtr:
            {
                var valuePtr = (nint*)dataPointer;
                dataPointer += sizeof(nint);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UIntPtr:
            {
                var valuePtr = (nuint*)dataPointer;
                dataPointer += sizeof(nuint);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Single:
            {
                var valuePtr = (float*)dataPointer;
                dataPointer += sizeof(float);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Double:
            {
                var valuePtr = (double*)dataPointer;
                dataPointer += sizeof(double);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Decimal:
            {
                var valuePtr = (decimal*)dataPointer;
                dataPointer += sizeof(decimal);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Guid:
            {
                var valuePtr = (Guid*)dataPointer;
                dataPointer += sizeof(Guid);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format);
            }

            case ArgumentType.DateTime:
            {
                var valuePtr = (DateTime*)dataPointer;
                dataPointer += sizeof(DateTime);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.TimeSpan:
            {
                var valuePtr = (TimeSpan*)dataPointer;
                dataPointer += sizeof(TimeSpan);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Enum:
            {
                var valuePtr = (EnumArg*)dataPointer;
                dataPointer += sizeof(EnumArg);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, config);
            }

            case ArgumentType.AsciiString:
            {
                var valueLength = *(int*)dataPointer;
                dataPointer += sizeof(int);

                var bufferLength = outputBuffer.Length;
                var lengthToCopy = Math.Min(valueLength, bufferLength);

                for (var i = 0; i < lengthToCopy; ++i)
                    outputBuffer[i] = (char)dataPointer[i];

                charsWritten = lengthToCopy;
                dataPointer += valueLength;

                return valueLength <= bufferLength;
            }

            case ArgumentType.Unmanaged:
            {
                var headerPtr = (UnmanagedArgHeader*)dataPointer;
                dataPointer += sizeof(UnmanagedArgHeader);

                if (!skipFormat)
                {
                    if (!headerPtr->TryAppendTo(dataPointer, outputBuffer, out charsWritten, format, config))
                        return false;
                }
                else
                {
                    if (!headerPtr->TryAppendUnformattedTo(dataPointer, outputBuffer, out charsWritten))
                        return false;
                }

                dataPointer += headerPtr->Size;
                return true;
            }

            case ArgumentType.KeyString:
            {
                ++dataPointer;

                if (dataPointer < _dataPointer)
                    SkipArg(ref dataPointer);

                charsWritten = 0;
                return true;
            }

            case ArgumentType.EndOfTruncatedMessage:
            {
                charsWritten = 0;
                return false;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void SkipArg(ref byte* dataPointer)
    {
        var argType = *(ArgumentType*)dataPointer;
        dataPointer += sizeof(ArgumentType);

        if ((argType & ArgumentType.FormatFlag) != 0)
        {
            argType &= ~ArgumentType.FormatFlag;
            ++dataPointer;
        }

        switch (argType)
        {
            case ArgumentType.None:
            {
                return;
            }

            case ArgumentType.String:
            {
                ++dataPointer;
                return;
            }

            case ArgumentType.Null:
            {
                return;
            }

            case ArgumentType.Boolean:
            {
                dataPointer += sizeof(bool);
                return;
            }

            case ArgumentType.Byte:
            {
                dataPointer += sizeof(byte);
                return;
            }

            case ArgumentType.SByte:
            {
                dataPointer += sizeof(sbyte);
                return;
            }

            case ArgumentType.Char:
            {
                dataPointer += sizeof(char);
                return;
            }

            case ArgumentType.Int16:
            {
                dataPointer += sizeof(short);
                return;
            }

            case ArgumentType.UInt16:
            {
                dataPointer += sizeof(ushort);
                return;
            }

            case ArgumentType.Int32:
            {
                dataPointer += sizeof(int);
                return;
            }

            case ArgumentType.UInt32:
            {
                dataPointer += sizeof(uint);
                return;
            }

            case ArgumentType.Int64:
            {
                dataPointer += sizeof(long);
                return;
            }

            case ArgumentType.UInt64:
            {
                dataPointer += sizeof(ulong);
                return;
            }

            case ArgumentType.IntPtr:
            {
                dataPointer += sizeof(nint);
                return;
            }

            case ArgumentType.UIntPtr:
            {
                dataPointer += sizeof(nuint);
                return;
            }

            case ArgumentType.Single:
            {
                dataPointer += sizeof(float);
                return;
            }

            case ArgumentType.Double:
            {
                dataPointer += sizeof(double);
                return;
            }

            case ArgumentType.Decimal:
            {
                dataPointer += sizeof(decimal);
                return;
            }

            case ArgumentType.Guid:
            {
                dataPointer += sizeof(Guid);
                return;
            }

            case ArgumentType.DateTime:
            {
                dataPointer += sizeof(DateTime);
                return;
            }

            case ArgumentType.TimeSpan:
            {
                dataPointer += sizeof(TimeSpan);
                return;
            }

            case ArgumentType.Enum:
            {
                dataPointer += sizeof(EnumArg);
                return;
            }

            case ArgumentType.AsciiString:
            {
                var valueLength = *(int*)dataPointer;
                dataPointer += sizeof(int);
                dataPointer += valueLength;
                return;
            }

            case ArgumentType.Unmanaged:
            {
                var headerPtr = (UnmanagedArgHeader*)dataPointer;
                dataPointer += sizeof(UnmanagedArgHeader);
                dataPointer += headerPtr->Size;
                return;
            }

            case ArgumentType.KeyString: // Should not happen
            {
                ++dataPointer;
                SkipArg(ref dataPointer);
                return;
            }

            case ArgumentType.EndOfTruncatedMessage:
            {
                return;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override string ToString()
    {
        if (ConstantMessage is not null)
            return ConstantMessage;

        var buffer = ArrayPool<char>.Shared.Rent(4 * 1024);

        var chars = WriteTo(buffer, ZeroLogConfiguration.Default);
        var value = new string(buffer.AsSpan(0, chars));

        ArrayPool<char>.Shared.Return(buffer);
        return value;
    }
}
