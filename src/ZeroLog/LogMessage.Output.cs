using System;
using System.Buffers;
using System.Globalization;

namespace ZeroLog;

unsafe partial class LogMessage
{
    internal int WriteTo(Span<char> outputBuffer)
    {
        var dataPointer = _startOfBuffer;
        var endOfData = _dataPointer;
        var bufferIndex = 0;

        while (dataPointer < endOfData)
        {
            var argType = *(ArgumentType*)dataPointer;
            dataPointer += sizeof(ArgumentType);

            switch (argType)
            {
                case ArgumentType.String:
                {
                    var stringIndex = *dataPointer;
                    ++dataPointer;

                    var value = _strings[stringIndex] ?? string.Empty;

                    if (value.Length <= outputBuffer.Length - bufferIndex)
                    {
                        value.CopyTo(outputBuffer[bufferIndex..]);
                        bufferIndex += value.Length;
                    }
                    else
                    {
                        var length = outputBuffer.Length - bufferIndex;
                        value.AsSpan(0, length).CopyTo(outputBuffer[bufferIndex..]);
                        bufferIndex += length;
                        goto outputTruncated;
                    }

                    break;
                }

                case ArgumentType.Boolean:
                {
                    var value = *(bool*)dataPointer;
                    dataPointer += sizeof(bool);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Byte:
                {
                    var value = *dataPointer;
                    dataPointer += sizeof(byte);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Char:
                {
                    var value = *(char*)dataPointer;
                    dataPointer += sizeof(char);

                    if (bufferIndex >= outputBuffer.Length)
                        goto outputTruncated;

                    outputBuffer[bufferIndex] = value;
                    ++bufferIndex;
                    break;
                }

                case ArgumentType.Int16:
                {
                    var value = *(short*)dataPointer;
                    dataPointer += sizeof(short);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Int32:
                {
                    var value = *(int*)dataPointer;
                    dataPointer += sizeof(int);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Int64:
                {
                    var value = *(long*)dataPointer;
                    dataPointer += sizeof(long);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Single:
                {
                    var value = *(float*)dataPointer;
                    dataPointer += sizeof(float);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Double:
                {
                    var value = *(double*)dataPointer;
                    dataPointer += sizeof(double);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Decimal:
                {
                    var value = *(decimal*)dataPointer;
                    dataPointer += sizeof(decimal);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Guid:
                {
                    var value = *(Guid*)dataPointer;
                    dataPointer += sizeof(Guid);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.DateTime:
                {
                    var value = *(DateTime*)dataPointer;
                    dataPointer += sizeof(DateTime);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.TimeSpan:
                {
                    var value = *(TimeSpan*)dataPointer;
                    dataPointer += sizeof(TimeSpan);

                    if (!value.TryFormat(outputBuffer[bufferIndex..], out var charsWritten, default, CultureInfo.InvariantCulture))
                        goto outputTruncated;

                    bufferIndex += charsWritten;
                    break;
                }

                case ArgumentType.Null:
                {
                    var value = LogManager.Config.NullDisplayString;

                    if (value.Length <= outputBuffer.Length - bufferIndex)
                    {
                        value.CopyTo(outputBuffer[bufferIndex..]);
                        bufferIndex += value.Length;
                    }
                    else
                    {
                        goto outputTruncated;
                    }

                    break;
                }

                case ArgumentType.AsciiString:
                case ArgumentType.Enum:
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
                suffix.CopyTo(outputBuffer[bufferIndex..]);
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
        var buffer = ArrayPool<char>.Shared.Rent(4 * 1024);

        var chars = WriteTo(buffer);
        var value = new string(buffer.AsSpan(0, chars));

        ArrayPool<char>.Shared.Return(buffer);
        return value;
    }
}
