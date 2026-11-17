using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ZeroLog.Benchmarks.Tools;

#nullable enable

internal static partial class AffinityHelper
{
    public static nint? GetPhysicalPCoreMask()
    {
        // I vibe-coded this for my machine, ignore it if it doesn't fit.
        if (!OperatingSystem.IsWindows())
            return null;

        GetSystemCpuSetInformation(null, 0, out var size, 0, 0);
        var data = new byte[size];

        if (!GetSystemCpuSetInformation(data, size, out size, 0, 0))
            return null;

        ulong mask = 0;
        var bestClass = -1;
        var cores = new HashSet<byte>();

        for (var offset = 0; offset < size;)
        {
            if (BitConverter.ToInt32(data, offset + 4) == 0)
            {
                int efficiency = data[offset + 18];

                if (efficiency > bestClass)
                {
                    bestClass = efficiency;
                    mask = 0;
                    cores.Clear();
                }

                if (efficiency == bestClass &&
                    cores.Add(data[offset + 15])) // Physical core index
                {
                    mask |= 1UL << data[offset + 14]; // Logical CPU index
                }
            }

            offset += checked((int)BitConverter.ToUInt32(data, offset));
        }

        return (nint)mask;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetSystemCpuSetInformation(
        [Out] byte[]? information,
        uint bufferLength,
        out uint returnedLength,
        nint process,
        uint flags
    );
}
