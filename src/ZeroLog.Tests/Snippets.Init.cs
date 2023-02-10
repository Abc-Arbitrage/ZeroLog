using NUnit.Framework;
using ZeroLog.Configuration;

// ReSharper disable once CheckNamespace
namespace ZeroLog.Tests.IsolatedNamespace;

// This initializer is not supposed to be executed, it only defines a snippet for the readme.
// Do not put unit tests in this namespace.

#region NUnitInitializer

[SetUpFixture]
public class Initializer
{
    [OneTimeSetUp]
    public void SetUp()
        => LogManager.Initialize(ZeroLogConfiguration.CreateTestConfiguration());

    [OneTimeTearDown]
    public void TearDown()
        => LogManager.Shutdown();
}

#endregion
