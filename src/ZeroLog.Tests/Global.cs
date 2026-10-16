using DiffEngine;
using NUnit.Framework;

namespace ZeroLog.Tests;

[SetUpFixture]
public static class Global
{
    [OneTimeSetUp]
    public static void GlobalSetup()
    {
        LogManager.IgnoreInternalErrors = true;

        DiffRunner.Disabled = true;
    }
}
