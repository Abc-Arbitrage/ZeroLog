using System;
using System.Reflection;

[assembly: AssemblyMetadata("IsAotCompatible", "True")]

namespace ZeroLog.Tests.Aot.NetStandard;

public static class NetStandardClass
{
    private const string _loggerName = "Warmed up logger from a .NET Standard dependency";

    public static bool HasWarmedUpLoggers()
    {
        var result = LogManager.IsLoggerRegistered(_loggerName);
        DoNothing();
        return result;
    }

    private static void DoNothing()
    {
        if (Guid.NewGuid() == Guid.Empty)
            _ = LogManager.GetLogger(_loggerName);
    }
}
