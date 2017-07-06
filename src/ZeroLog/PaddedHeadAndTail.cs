using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ZeroLog
{
    /// <summary>Padded head and tail indices, to avoid false sharing between producers and consumers.</summary>
    [DebuggerDisplay("Head = {Head}, Tail = {Tail}")]
    [StructLayout(LayoutKind.Explicit, Size = 192)] // padding before/between/after fields based on typical cache line size of 64
    internal struct PaddedHeadAndTail
    {
        [FieldOffset(64)] public int Head;
        [FieldOffset(128)] public int Tail;
    }
}