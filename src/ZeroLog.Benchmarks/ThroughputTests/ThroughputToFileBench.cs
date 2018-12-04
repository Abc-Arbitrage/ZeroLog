using System;
using System.Diagnostics;
using System.IO;
using ZeroLog.Appenders;
using ZeroLog.Config;

namespace ZeroLog.Benchmarks.ThroughputTests
{
    public class ThroughputToFileBench
    {
        public static void Run()
        {
            var dir = Path.GetFullPath(Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);

            try
            {
                Console.WriteLine("Initializing...");

                BasicConfigurator.Configure(
                    new[] { new DateAndSizeRollingFileAppender(Path.Combine(dir, "Output")), },
                    new ZeroLogInitializationConfig
                    {
                        LogEventQueueSize = 1000 * 4096 * 4,
                    },
                    logEventPoolExhaustionStrategy: LogEventPoolExhaustionStrategy.WaitForLogEvent
                );

                var log = LogManager.GetLogger(typeof(ThroughputToFileBench));
                var duration = TimeSpan.FromSeconds(10);

                Console.WriteLine("Starting...");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var sw = Stopwatch.StartNew();
                long counter = 0;
                while (sw.Elapsed < duration)
                {
                    counter++;
                    log.Debug().Append("Counter is: ").Append(counter).Log();
                }

                Console.WriteLine($"Log events: {counter:N0}, Time to append: {sw.Elapsed}");
                Console.WriteLine("Flushing...");
                LogManager.Shutdown();
                Console.WriteLine($"Time to flush: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Directory.Delete(dir, true);
            }

            Console.WriteLine("Done");
        }
    }
}
