using System;
using System.Globalization;
using ZeroLog.Configuration;

namespace ZeroLog;

unsafe partial class LogMessage
{
    private bool TryWriteChain(string chain,
                               ref byte* dataPointer,
                               Span<char> outputBuffer,
                               ZeroLogConfiguration config,
                               out int charsWritten)
    {
        charsWritten = 0;

        if (string.IsNullOrEmpty(chain))
            return false;

        if (chain[0] != AppendChain.Version)
            return false;

        var chainIndex = 1;

        while (chainIndex < chain.Length)
        {
            var linkWritten = TryWriteChainLink(
                chain,
                ref chainIndex,
                ref dataPointer,
                outputBuffer.Slice(charsWritten),
                config,
                out var linkCharsWritten
            );

            charsWritten += linkCharsWritten;

            if (!linkWritten)
                return false;
        }

        return true;
    }

    private bool TryWriteChainLink(string chain,
                                   ref int chainIndex,
                                   ref byte* dataPointer,
                                   Span<char> outputBuffer,
                                   ZeroLogConfiguration config,
                                   out int charsWritten)
    {
        var linkType = (AppendChain.LinkType)chain[chainIndex];
        ++chainIndex;

        var underlyingType = linkType.ToBaseType();
        var endOfData = _dataPointer;
        var format = ReadOnlySpan<char>.Empty;

        if (linkType.HasFormatString)
        {
            if (chainIndex >= chain.Length)
                goto truncate;

            var lengthInChars = (int)chain[chainIndex];
            ++chainIndex;

            if (chainIndex + lengthInChars > chain.Length)
                goto truncate;

            format = chain.AsSpan(chainIndex, lengthInChars);
            chainIndex += lengthInChars;
        }

        if (linkType.IsNullableValueType)
        {
            if (dataPointer >= endOfData)
                goto truncate;

            var hasValue = *dataPointer != 0;
            ++dataPointer;

            if (!hasValue)
                underlyingType = AppendChain.LinkType.Null;
        }

        switch (underlyingType)
        {
            case AppendChain.LinkType.ConstantString:
            {
                if (chainIndex >= chain.Length)
                    goto truncate;

                var lengthInChars = (int)chain[chainIndex];
                ++chainIndex;

                if (chainIndex + lengthInChars > chain.Length)
                    goto truncate;

                var bufferLength = outputBuffer.Length;
                var charsToCopy = Math.Min(lengthInChars, bufferLength);

                chain.AsSpan(chainIndex, charsToCopy).CopyTo(outputBuffer);

                charsWritten = charsToCopy;
                chainIndex += lengthInChars;

                return lengthInChars <= bufferLength;
            }

            case AppendChain.LinkType.String:
            {
                if (dataPointer >= endOfData)
                    goto truncate;

                var stringIndex = *dataPointer;
                ++dataPointer;

                var value = _strings[stringIndex] ?? string.Empty;

                if (value.Length <= outputBuffer.Length)
                {
                    value.CopyTo(outputBuffer);
                    charsWritten = value.Length;
                    return true;
                }

                var length = outputBuffer.Length;
                value.AsSpan(0, length).CopyTo(outputBuffer);
                charsWritten = length;
                return false;
            }

            case AppendChain.LinkType.Null:
            case AppendChain.LinkType.NullString:
            {
                var value = config.NullDisplayString;

                if (value.Length <= outputBuffer.Length)
                {
                    value.CopyTo(outputBuffer);
                    charsWritten = value.Length;
                    return true;
                }

                goto truncate;
            }

            case AppendChain.LinkType.Boolean:
            {
                if (dataPointer + sizeof(bool) > endOfData)
                    goto truncate;

                var valuePtr = (bool*)dataPointer;
                dataPointer += sizeof(bool);

                return valuePtr->TryFormat(outputBuffer, out charsWritten);
            }

            case AppendChain.LinkType.Char:
            {
                if (dataPointer + sizeof(char) > endOfData)
                    goto truncate;

                var valuePtr = (char*)dataPointer;
                dataPointer += sizeof(char);

                if (outputBuffer.Length < 1)
                    goto truncate;

                outputBuffer[0] = *valuePtr;
                charsWritten = 1;
                return true;
            }

            case AppendChain.LinkType.SByte:
            {
                if (dataPointer + sizeof(sbyte) > endOfData)
                    goto truncate;

                var valuePtr = (sbyte*)dataPointer;
                dataPointer += sizeof(sbyte);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Byte:
            {
                if (dataPointer + sizeof(byte) > endOfData)
                    goto truncate;

                var valuePtr = dataPointer;
                dataPointer += sizeof(byte);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Int16:
            {
                if (dataPointer + sizeof(short) > endOfData)
                    goto truncate;

                var valuePtr = (short*)dataPointer;
                dataPointer += sizeof(short);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.UInt16:
            {
                if (dataPointer + sizeof(ushort) > endOfData)
                    goto truncate;

                var valuePtr = (ushort*)dataPointer;
                dataPointer += sizeof(ushort);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Int32:
            {
                if (dataPointer + sizeof(int) > endOfData)
                    goto truncate;

                var valuePtr = (int*)dataPointer;
                dataPointer += sizeof(int);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.UInt32:
            {
                if (dataPointer + sizeof(uint) > endOfData)
                    goto truncate;

                var valuePtr = (uint*)dataPointer;
                dataPointer += sizeof(uint);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Int64:
            {
                if (dataPointer + sizeof(long) > endOfData)
                    goto truncate;

                var valuePtr = (long*)dataPointer;
                dataPointer += sizeof(long);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.UInt64:
            {
                if (dataPointer + sizeof(ulong) > endOfData)
                    goto truncate;

                var valuePtr = (ulong*)dataPointer;
                dataPointer += sizeof(ulong);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Decimal:
            {
                if (dataPointer + sizeof(decimal) > endOfData)
                    goto truncate;

                var valuePtr = (decimal*)dataPointer;
                dataPointer += sizeof(decimal);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Single:
            {
                if (dataPointer + sizeof(float) > endOfData)
                    goto truncate;

                var valuePtr = (float*)dataPointer;
                dataPointer += sizeof(float);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Double:
            {
                if (dataPointer + sizeof(double) > endOfData)
                    goto truncate;

                var valuePtr = (double*)dataPointer;
                dataPointer += sizeof(double);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.Guid:
            {
                if (dataPointer + sizeof(Guid) > endOfData)
                    goto truncate;

                var valuePtr = (Guid*)dataPointer;
                dataPointer += sizeof(Guid);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format);
            }

            case AppendChain.LinkType.DateTime:
            {
                if (dataPointer + sizeof(DateTime) > endOfData)
                    goto truncate;

                var valuePtr = (DateTime*)dataPointer;
                dataPointer += sizeof(DateTime);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.TimeSpan:
            {
                if (dataPointer + sizeof(TimeSpan) > endOfData)
                    goto truncate;

                var valuePtr = (TimeSpan*)dataPointer;
                dataPointer += sizeof(TimeSpan);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.DateTimeOffset:
            {
                if (dataPointer + sizeof(DateTimeOffset) > endOfData)
                    goto truncate;

                var valuePtr = (DateTimeOffset*)dataPointer;
                dataPointer += sizeof(DateTimeOffset);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.DateOnly:
            {
                if (dataPointer + sizeof(DateOnly) > endOfData)
                    goto truncate;

                var valuePtr = (DateOnly*)dataPointer;
                dataPointer += sizeof(DateOnly);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }

            case AppendChain.LinkType.TimeOnly:
            {
                if (dataPointer + sizeof(TimeOnly) > endOfData)
                    goto truncate;

                var valuePtr = (TimeOnly*)dataPointer;
                dataPointer += sizeof(TimeOnly);

                return valuePtr->TryFormat(outputBuffer, out charsWritten, format, CultureInfo.InvariantCulture);
            }
        }

        truncate:
        charsWritten = 0;
        return false;
    }

    private static void SkipChain(ref byte* dataPointer)
    {
        // TODO: Implement skip
        throw new NotImplementedException();
    }
}
