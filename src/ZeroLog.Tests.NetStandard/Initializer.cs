using DiffEngine;
using NUnit.Framework;
using VerifyTests;

namespace ZeroLog.Tests.NetStandard;

[SetUpFixture]
public static class Initializer
{
    [OneTimeSetUp]
    public static void Initialize()
    {
        DiffRunner.Disabled = true;
        VerifyDiffPlex.Initialize();
    }
}
