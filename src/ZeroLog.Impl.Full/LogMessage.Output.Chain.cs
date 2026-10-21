using System;
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
        return false;
    }

    private bool TryWriteChainLink(string chain,
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
}
