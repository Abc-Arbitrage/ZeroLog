using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ZeroLog;

unsafe partial class LogMessage
{
    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    internal int WriteTo(Span<char> outputBuffer)
    {
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
            var argType = *(ArgumentType*)dataPointer;
            dataPointer += sizeof(ArgumentType);

            var format = default(string);

            if ((argType & ArgumentType.FormatFlag) != 0)
            {
                argType &= ~ArgumentType.FormatFlag;

                var stringIndex = *dataPointer;
                ++dataPointer;

                format = _strings[stringIndex];
            }

            switch (argType)
            {
                case ArgumentType.None:
                    break;

                case ArgumentType.String:
                {
                    var stringIndex = *dataPointer;
                    ++dataPointer;

                    var value = _strings[stringIndex] ?? string.Empty;

                    if (value.Length <= outputBuffer.Length - bufferIndex)
                    {
                        value.CopyTo(outputBuffer.Slice(bufferIndex));
                        bufferIndex += value.Length;
                    }
                    else
                    {
                        var length = outputBuffer.Length - bufferIndex;
                        value.AsSpan(0, length).CopyTo(outputBuffer.Slice(bufferIndex));
                        bufferIndex += length;
                        goto outputTruncated;
                    }

                    break;
                }

                case ArgumentType.Null:
                {
                    var value = LogManager.Config.NullDisplayString;

                    if (value.Length <= outputBuffer.Length - bufferIndex)
                    {
                        value.CopyTo(outputBuffer.Slice(bufferIndex));
                        bufferIndex += value.Length;
                    }
                    else
                    {
                        goto outputTruncated;
                    }

                    break;
                }

                case ArgumentType.Boolean:
                {
                    var valuePtr = (bool*)dataPointer;
                    dataPointer += sizeof(bool);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Byte:
                {
                    var valuePtr = dataPointer;
                    dataPointer += sizeof(byte);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.SByte:
                {
                    var valuePtr = (sbyte*)dataPointer;
                    dataPointer += sizeof(sbyte);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Char:
                {
                    var valuePtr = (char*)dataPointer;
                    dataPointer += sizeof(char);

                    if (bufferIndex >= outputBuffer.Length)
                        goto outputTruncated;

                    outputBuffer[bufferIndex] = *valuePtr;
                    ++bufferIndex;
                    break;
                }

                case ArgumentType.Int16:
                {
                    var valuePtr = (short*)dataPointer;
                    dataPointer += sizeof(short);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.UInt16:
                {
                    var valuePtr = (ushort*)dataPointer;
                    dataPointer += sizeof(ushort);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Int32:
                {
                    var valuePtr = (int*)dataPointer;
                    dataPointer += sizeof(int);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.UInt32:
                {
                    var valuePtr = (uint*)dataPointer;
                    dataPointer += sizeof(uint);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Int64:
                {
                    var valuePtr = (long*)dataPointer;
                    dataPointer += sizeof(long);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.UInt64:
                {
                    var valuePtr = (ulong*)dataPointer;
                    dataPointer += sizeof(ulong);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.IntPtr:
                {
                    var valuePtr = (nint*)dataPointer;
                    dataPointer += sizeof(nint);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.UIntPtr:
                {
                    var valuePtr = (nuint*)dataPointer;
                    dataPointer += sizeof(nuint);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Single:
                {
                    var valuePtr = (float*)dataPointer;
                    dataPointer += sizeof(float);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Double:
                {
                    var valuePtr = (double*)dataPointer;
                    dataPointer += sizeof(double);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Decimal:
                {
                    var valuePtr = (decimal*)dataPointer;
                    dataPointer += sizeof(decimal);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Guid:
                {
                    var valuePtr = (Guid*)dataPointer;
                    dataPointer += sizeof(Guid);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.DateTime:
                {
                    var valuePtr = (DateTime*)dataPointer;
                    dataPointer += sizeof(DateTime);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.TimeSpan:
                {
                    var valuePtr = (TimeSpan*)dataPointer;
                    dataPointer += sizeof(TimeSpan);

                    if (!valuePtr->TryFormat(outputBuffer.Slice(bufferIndex), out var charsWritten, format, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Enum:
                {
                    var valuePtr = (EnumArg*)dataPointer;
                    dataPointer += sizeof(EnumArg);

                    if (!valuePtr->TryFormat(outputBuffer[bufferIndex..], out var charsWritten))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.AsciiString:
                case ArgumentType.Unmanaged:
                case ArgumentType.KeyString:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (_isTruncated)
            goto outputTruncated;

        return bufferIndex;

        outputTruncated:
        {
            var suffix = LogManager.Config.TruncatedMessageSuffix;

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
                suffix.CopyTo(outputBuffer[idx..]);
            }
            else
            {
                // The suffix is too large to fit in the buffer.
                suffix.AsSpan(0, outputBuffer.Length).CopyTo(outputBuffer);
            }

            return outputBuffer.Length;
        }
    }

    public override string ToString()
    {
        if (ConstantMessage is not null)
            return ConstantMessage;

        var buffer = ArrayPool<char>.Shared.Rent(4 * 1024);

        var chars = WriteTo(buffer);
        var value = new string(buffer.AsSpan(0, chars));

        ArrayPool<char>.Shared.Return(buffer);
        return value;
    }
}
