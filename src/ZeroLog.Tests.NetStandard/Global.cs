using DiffEngine;
using NUnit.Framework;

namespace ZeroLog.Tests.NetStandard;

[SetUpFixture]
public static class Global
{
    [OneTimeSetUp]
    public static void GlobalSetup()
    {
        DiffRunner.Disabled = true;
    }
}
