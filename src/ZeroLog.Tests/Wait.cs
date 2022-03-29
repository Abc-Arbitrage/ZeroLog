using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;

namespace ZeroLog.Tests;

public static class Wait
{
    public static void Until([InstantHandle] Func<bool> exitCondition, TimeSpan timeout)
    {
        var sw = Stopwatch.StartNew();

        while (sw.Elapsed < timeout)
        {
            if (exitCondition())
                return;

            Thread.Sleep(10);
        }

        throw new TimeoutException();
    }
}
