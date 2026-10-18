using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZeroLog;

#pragma warning disable CS1591

partial class LogMessage
{
    /// <summary>
    /// This class is for internal use only and should not be used directly by users.
    /// It may cause breaking changes or be removed in future versions without notice.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class InternalApi
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartChain(LogMessage message, string chain)
            => message.InternalStartChain(chain);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalStartChain(string chain);

#if NETSTANDARD

    internal partial void InternalStartChain(string chain)
    { }

#endif
}
