using DiffEngine;
using NUnit.Framework;

namespace ZeroLog.Analyzers.Tests;

[SetUpFixture]
public static class Global
{
    [OneTimeSetUp]
    public static void GlobalSetup()
    {
        DiffRunner.Disabled = true;
    }
}
