using System;
using System.IO;
using System.Threading;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class AllocationTests
    {
        private WaitableAppender _waitableAppender;

        public class WaitableAppender : DateAndSizeRollingFileAppender
        {
            public int WrittenEventCount { get; private set; }

            public WaitableAppender(string filePathRoot)
                : base(filePathRoot)
            {
            }

            public override void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
            {
                WrittenEventCount++;
                base.WriteEvent(logEventHeader, messageBytes, messageLength);
            }
        }

        [SetUp]
        public void Setup()
        {
            foreach (var file in Directory.EnumerateFiles(@".\", $"allocation-test.*"))
            {
                File.Delete(file);
            }

            _waitableAppender = new WaitableAppender("allocation-test");
            BasicConfigurator.Configure(new[] { _waitableAppender }, 2048 * 10, 512);
            LogManager.RegisterEnum<DayOfWeek>();
        }

        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void should_not_allocate_using_all_formats_and_file_appender()
        {
            // Allocation tests are unreliable when run from NCrunch
            if (Environment.GetEnvironmentVariable("NCrunch") == "1")
                Assert.Inconclusive();

            var log = LogManager.GetLogger("AllocationTest");

            GC.Collect(2, GCCollectionMode.Forced, true);
            var gcCountBefore = GC.CollectionCount(0);

            var numberOfEvents = 2048 * 10;

            for (var i = 0; i < numberOfEvents; i++)
            {
                log.InfoFormat(
                    "Int {0}, Double {1:N4}, String {2}, Bool {3}, Decimal {4:N4}, Guid {5}, Timestamp {6}, DateTime {7}",
                    123243,
                    32423432.4398438,
                    "Some random string",
                    true,
                    4234324324.23423423,
                    Guid.NewGuid(),
                    DateTime.UtcNow.TimeOfDay,
                    DateTime.UtcNow
                );

                log.InfoFormat(
                    "Enum {0}, UnknownEnum {1}",
                    DayOfWeek.Friday,
                    ConsoleColor.Blue
                );
            }

            // Give the appender some time to finish writing to file
            while (_waitableAppender.WrittenEventCount < numberOfEvents)
                Thread.Sleep(1);

            var gcCountAfter = GC.CollectionCount(0);

            Check.That(gcCountBefore).IsEqualTo(gcCountAfter);
        }
    }
}
