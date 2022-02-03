using System;

namespace ZeroLog.Tests;

public static class Program
{
    public static void Main()
    {
        var test = new PerformanceTests();

        test.SetUp();

        try
        {
            test.should_run_test();
        }
        finally
        {
            test.Teardown();
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
