using System;
using System.IO;
using System.Threading;
using NCrunch.Framework;
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
        private string _tempDirectory;

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
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);

            _waitableAppender = new WaitableAppender(Path.Combine(_tempDirectory, "allocation-test"));

            BasicConfigurator.Configure(new ZeroLogBasicConfiguration
            {
                Appenders = { _waitableAppender },
                LogEventQueueSize = 2048 * 10,
                LogEventBufferSize = 512
            });

            LogManager.RegisterEnum<DayOfWeek>();
        }

        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
            Directory.Delete(_tempDirectory, true);
        }

        [Test]
        public void should_not_allocate_using_all_formats_and_file_appender()
        {
            // Allocation tests are unreliable when run from NCrunch
            if (NCrunchEnvironment.NCrunchIsResident())
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
                    "Enum {0}, UnknownEnum {1}, NullableEnum {2}, NullableNullEnum {3}, NullableInt {4}, NullableNullInt {5}",
                    DayOfWeek.Friday,
                    UnregisteredEnum.Bar,
                    (DayOfWeek?)DayOfWeek.Monday,
                    (DayOfWeek?)null,
                    (int?)42,
                    (int?)null
                );
            }

            // Give the appender some time to finish writing to file
            while (_waitableAppender.WrittenEventCount < numberOfEvents)
                Thread.Sleep(1);

            var gcCountAfter = GC.CollectionCount(0);

            Check.That(gcCountBefore).IsEqualTo(gcCountAfter);
        }

        private enum UnregisteredEnum
        {
            Foo,
            Bar,
            Baz
        }
    }
}
