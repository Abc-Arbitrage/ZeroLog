using System;
using NUnit.Framework;
using Shouldly;

namespace ZeroLog.Tests.Support;

#nullable enable

internal static class GcTester
{
    public static void ShouldNotAllocate(Action action, Action? afterWarmup = null)
    {
        // Warmup
        action.Invoke();
        afterWarmup?.Invoke();

        var bytesBefore = GC.GetAllocatedBytesForCurrentThread();

        action.Invoke();

        var bytesAfter = GC.GetAllocatedBytesForCurrentThread();
        var allocatedBytes = bytesAfter - bytesBefore;

        allocatedBytes.ShouldBe(0);
    }
}
