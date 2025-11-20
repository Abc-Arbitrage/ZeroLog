namespace ZeroLog.Tests.Allocation;

public static class Program
{
    private static int Main()
        => AllocationTests.Run() ? 0 : 1;
}
