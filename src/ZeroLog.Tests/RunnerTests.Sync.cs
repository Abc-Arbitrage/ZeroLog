using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class SyncRunnerTests
{
    private TestAppender _testAppender;
    private SyncRunner _runner;
    private Log _log;

    [SetUp]
    public void SetUpFixture()
    {
        _testAppender = new TestAppender(true);

        var config = new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            LogMessageBufferSize = 256,
            AppendingStrategy = AppendingStrategy.Synchronous,
            RootLogger =
            {
                Appenders = { _testAppender }
            }
        };

        _runner = new SyncRunner(config);

        _log = new Log(nameof(SyncRunnerTests));
        _log.UpdateConfiguration(_runner, config);
    }

    [TearDown]
    public void Teardown()
    {
        _runner.Dispose();
    }

    [Test]
    public void should_flush_appenders_immediately()
    {
        _log.Info("Foo");
        _log.Info("Bar");
        _log.Info("Baz");

        _testAppender.FlushCount.ShouldEqual(3);

        _log.Info("Foo");
        _testAppender.FlushCount.ShouldEqual(4);
    }

    [Test]
    public void should_apply_configuration_updates()
    {
        _runner.UpdateConfiguration(new ZeroLogConfiguration
        {
            NullDisplayString = "Foo"
        });

        _log.Info(null);
        _testAppender.LoggedMessages.ShouldHaveSingleItem().ShouldEqual("Foo");
    }
}
