using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Unicode;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog;

#if NET8_0_OR_GREATER

unsafe partial class LogMessage
{
    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    internal int WriteTo(Span<byte> outputBuffer,
                         ZeroLogConfiguration config,
                         FormatType formatType,
                         KeyValueList? keyValueList)
    {
        keyValueList?.Clear();

        if (ConstantMessageUtf8 is not null)
        {
            var length = Math.Min(ConstantMessageUtf8.Length, outputBuffer.Length);
            ConstantMessageUtf8.AsSpan(0, length).CopyTo(outputBuffer);
            return length;
        }

        var dataPointer = _startOfBuffer;
        var endOfData = _dataPointer;
        var bufferIndex = 0;

        while (dataPointer < endOfData)
        {
            if (keyValueList != null)
            {
                var argType = *(ArgumentType*)dataPointer;
                if (argType == ArgumentType.KeyString) // KeyString never has a format flag
                {
                    if (!TryWriteKeyValue(ref dataPointer, keyValueList, config)) // TODO UTF-8
                        goto outputTruncated;

                    continue;
                }
            }

            var isTruncated = !TryWriteArg(ref dataPointer, outputBuffer.Slice(bufferIndex), out var charsWritten, formatType, config);
            bufferIndex += charsWritten;

            if (isTruncated)
                goto outputTruncated;
        }

        if (_isTruncated)
            goto outputTruncated;

        return bufferIndex;

        outputTruncated:
        {
            var suffix = config.TruncatedMessageSuffixUtf8;

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

    private bool TryWriteArg(ref byte* dataPointer, Span<byte> outputBuffer, out int bytesWritten, FormatType formatType, ZeroLogConfiguration config)
    {
        var argType = *(ArgumentType*)dataPointer;
        dataPointer += sizeof(ArgumentType);

        var format = default(string);

        if ((argType & ArgumentType.FormatFlag) != 0)
        {
            argType &= ~ArgumentType.FormatFlag;

            var stringIndex = *dataPointer;
            ++dataPointer;

            if (formatType == FormatType.Formatted)
                format = _strings[stringIndex];
        }

        if (formatType == FormatType.KeyValue)
        {
            format = argType switch
            {
                ArgumentType.DateTime       => "yyyy-MM-dd HH:mm:ss",
                ArgumentType.TimeSpan       => @"hh\:mm\:ss\.fffffff",
                ArgumentType.DateOnly       => @"yyyy-MM-dd",
                ArgumentType.TimeOnly       => @"HH\:mm\:ss\.fffffff",
                ArgumentType.DateTimeOffset => @"yyyy-MM-dd HH:mm:ss zzz",
                _                           => null
            };
        }

        switch (argType)
        {
            case ArgumentType.None:
            {
                bytesWritten = 0;
                return true;
            }

            case ArgumentType.String:
            {
                var stringIndex = *dataPointer;
                ++dataPointer;

                var value = _strings[stringIndex] ?? string.Empty;
                var status = Utf8.FromUtf16(value, outputBuffer, out _, out bytesWritten);
                return status == OperationStatus.Done;
            }

            case ArgumentType.Null:
            {
                var value = config.NullDisplayStringUtf8;

                if (value.Length <= outputBuffer.Length)
                {
                    value.CopyTo(outputBuffer);
                    bytesWritten = value.Length;
                }
                else
                {
                    bytesWritten = 0;
                    return false;
                }

                return true;
            }

            case ArgumentType.Boolean:
            {
                var valuePtr = (bool*)dataPointer;
                dataPointer += sizeof(bool);

                var status = Utf8.FromUtf16(*valuePtr ? bool.TrueString : bool.FalseString, outputBuffer, out _, out var valueBytesWritten);
                if (status == OperationStatus.Done)
                {
                    bytesWritten = valueBytesWritten;
                    return true;
                }

                bytesWritten = 0;
                return false;
            }

            case ArgumentType.Byte:
            {
                var valuePtr = dataPointer;
                dataPointer += sizeof(byte);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.SByte:
            {
                var valuePtr = (sbyte*)dataPointer;
                dataPointer += sizeof(sbyte);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Char:
            {
                var valuePtr = (char*)dataPointer;
                dataPointer += sizeof(char);

                var status = Utf8.FromUtf16(new ReadOnlySpan<char>(in *valuePtr), outputBuffer, out _, out var valueBytesWritten);
                if (status == OperationStatus.Done)
                {
                    bytesWritten = valueBytesWritten;
                    return true;
                }

                bytesWritten = 0;
                return false;
            }

            case ArgumentType.Int16:
            {
                var valuePtr = (short*)dataPointer;
                dataPointer += sizeof(short);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UInt16:
            {
                var valuePtr = (ushort*)dataPointer;
                dataPointer += sizeof(ushort);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Int32:
            {
                var valuePtr = (int*)dataPointer;
                dataPointer += sizeof(int);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UInt32:
            {
                var valuePtr = (uint*)dataPointer;
                dataPointer += sizeof(uint);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Int64:
            {
                var valuePtr = (long*)dataPointer;
                dataPointer += sizeof(long);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UInt64:
            {
                var valuePtr = (ulong*)dataPointer;
                dataPointer += sizeof(ulong);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.IntPtr:
            {
                var valuePtr = (nint*)dataPointer;
                dataPointer += sizeof(nint);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.UIntPtr:
            {
                var valuePtr = (nuint*)dataPointer;
                dataPointer += sizeof(nuint);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Single:
            {
                var valuePtr = (float*)dataPointer;
                dataPointer += sizeof(float);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Double:
            {
                var valuePtr = (double*)dataPointer;
                dataPointer += sizeof(double);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Decimal:
            {
                var valuePtr = (decimal*)dataPointer;
                dataPointer += sizeof(decimal);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Guid:
            {
                var valuePtr = (Guid*)dataPointer;
                dataPointer += sizeof(Guid);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format);
            }

            case ArgumentType.DateTime:
            {
                var valuePtr = (DateTime*)dataPointer;
                dataPointer += sizeof(DateTime);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.TimeSpan:
            {
                var valuePtr = (TimeSpan*)dataPointer;
                dataPointer += sizeof(TimeSpan);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.DateOnly:
            {
                var valuePtr = (DateOnly*)dataPointer;
                dataPointer += sizeof(DateOnly);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.TimeOnly:
            {
                var valuePtr = (TimeOnly*)dataPointer;
                dataPointer += sizeof(TimeOnly);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.DateTimeOffset:
            {
                var valuePtr = (DateTimeOffset*)dataPointer;
                dataPointer += sizeof(DateTimeOffset);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, format, CultureInfo.InvariantCulture);
            }

            case ArgumentType.Enum:
            {
                var valuePtr = (EnumArg*)dataPointer;
                dataPointer += sizeof(EnumArg);

                return valuePtr->TryFormat(outputBuffer, out bytesWritten, config);
            }

            case ArgumentType.StringSpan:
            {
                var lengthInChars = *(int*)dataPointer;
                dataPointer += sizeof(int);

                var status = Utf8.FromUtf16(new ReadOnlySpan<char>(dataPointer, lengthInChars), outputBuffer, out _, out bytesWritten);
                dataPointer += lengthInChars * sizeof(char);
                return status == OperationStatus.Done;
            }

            case ArgumentType.Utf8StringSpan:
            {
                var lengthInBytes = *(int*)dataPointer;
                dataPointer += sizeof(int);

                var valueBytes = new ReadOnlySpan<byte>(dataPointer, lengthInBytes);
                var lengthToCopy = Math.Min(lengthInBytes, outputBuffer.Length);

                valueBytes.CopyTo(outputBuffer.Slice(0, lengthToCopy));
                dataPointer += lengthInBytes;
                bytesWritten = lengthToCopy;
                return lengthInBytes == lengthToCopy;
            }

            case ArgumentType.Unmanaged:
            {
                var headerPtr = (UnmanagedArgHeader*)dataPointer;
                dataPointer += sizeof(UnmanagedArgHeader);

                // TODO UTF-8
                bytesWritten = 0;

                // if (formatType == FormatType.Formatted)
                // {
                //     if (!headerPtr->TryAppendTo(dataPointer, outputBuffer, out bytesWritten, format, config))
                //         return false;
                // }
                // else
                // {
                //     if (!headerPtr->TryAppendUnformattedTo(dataPointer, outputBuffer, out bytesWritten))
                //         return false;
                // }

                dataPointer += headerPtr->Size;
                return true;
            }

            case ArgumentType.KeyString:
            {
                ++dataPointer;

                if (dataPointer < _dataPointer)
                    SkipArg(ref dataPointer);

                bytesWritten = 0;
                return true;
            }

            case ArgumentType.EndOfTruncatedMessage:
            {
                bytesWritten = 0;
                return false;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

#endif
