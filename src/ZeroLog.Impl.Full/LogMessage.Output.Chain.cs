using System;
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

        var context = new InternalApi.ChainExecutionContext(
            inputData,
            outputBuffer
        );

        chain.CallExecute(ref context);

        charsWritten = context._charsWritten;
        dataPointer += inputData.Length - context._inputData.Length;
        return !context._truncated;

        truncate:
        charsWritten = 0;
        return false;
    }

    private bool TryWriteChainLink(InternalApi.Chain chain,
                                   ref int chainIndex,
                                   ref byte* dataPointer,
                                   Span<char> outputBuffer,
                                   ZeroLogConfiguration config,
                                   out int charsWritten)
    {
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
            internal ReadOnlySpan<byte> _inputData;
            private Span<char> _outputBuffer;
            internal int _charsWritten = 0;
            internal bool _truncated = false;

            internal ChainExecutionContext(ReadOnlySpan<byte> inputData, Span<char> outputBuffer)
            {
                _inputData = inputData;
                _outputBuffer = outputBuffer;
            }

            public partial void OutputString()
            {
                _ = _truncated;
            }

            public partial void OutputString(string? value)
            {
                if (value is null)
                    return; // TODO: NULL handling

                var charsToCopy = Math.Min(value.Length, _outputBuffer.Length);
                value.AsSpan().CopyTo(_outputBuffer.Slice(0, charsToCopy));

                _outputBuffer = _outputBuffer.Slice(charsToCopy);
                _charsWritten += charsToCopy;

                if (charsToCopy < value.Length)
                    _truncated = true;
            }

            public partial void OutputValueType<T>()
                where T : unmanaged
            {
                _inputData = _inputData.Slice(sizeof(T));
            }
        }
    }
}
