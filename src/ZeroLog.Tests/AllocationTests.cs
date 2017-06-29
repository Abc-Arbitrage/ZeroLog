using System;
using System.IO;
using System.Threading;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class AllocationTests
    {
        [SetUp]
        public void Setup()
        {
            foreach (var file in Directory.EnumerateFiles(@".\", $"allocation-test.*"))
            {
                File.Delete(file);
            }

            LogManager.Initialize(new[] { new DateAndSizeRollingFileAppender("allocation-test") }, 2048 * 10, 512);
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

            var log = LogManager.GetLogger("AllocationTest");

            GC.Collect(2, GCCollectionMode.Forced, true);
            var gcCountBefore = GC.CollectionCount(0);

            for (var i = 0; i < 2048 * 10; i++)
            {
                log.InfoFormat("Int {0}, Double {1:N4}, String {2}, Bool {3}, Decimal {4:N4}, Guid {5}, Timestamp {6}, DateTime {7}",
                               123243,
                               32423432.4398438,
                               "Some random string",
                               true,
                               4234324324.23423423,
                               Guid.NewGuid(),
                               DateTime.UtcNow.TimeOfDay,
                               DateTime.UtcNow);
            }

            // Give the appender some time to finish writing to file
            Thread.Sleep(1000);

            var gcCountAfter = GC.CollectionCount(0);

            Check.That(gcCountBefore).IsEqualTo(gcCountAfter);
        }
    }
}
