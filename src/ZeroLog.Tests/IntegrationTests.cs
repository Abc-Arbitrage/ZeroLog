using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    [TestFixture]
    [Ignore("Manual")]
    public class IntegrationTests
    {
        private PerformanceAppender _performanceAppender;
        private const int _nbThreads = 4;
        private const int _queueSize = 1 << 16;
        private const int _count = _queueSize / _nbThreads;
        private readonly List<double[]> _enqueueMicros = new List<double[]>();

        [SetUp]
        public void SetUp()
        {
            _performanceAppender = new PerformanceAppender(_count * _nbThreads);
//            LogManager.Initialize(new[] { _performanceAppender, }, _queueSize);
            LogManager.Initialize(new[] { new ConsoleAppender(), }, _queueSize);
            for (int i = 0; i < _nbThreads; i++)
            {
                _enqueueMicros.Add(new double[_count]);
            }
        }

        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void should_test_console()
        {
            LogManager.GetLogger(typeof(IntegrationTests)).Info().Append("Hello").Log();
            LogManager.GetLogger(typeof(IntegrationTests)).Warn().Append("Hello").Log();
            LogManager.GetLogger(typeof(IntegrationTests)).Fatal().Append("Hello").Log();
            LogManager.GetLogger(typeof(IntegrationTests)).Error().Append("Hello").Log();
            LogManager.GetLogger(typeof(IntegrationTests)).Debug().Append("Hello").Log();
        }

        [Test]
        public unsafe void should_test_encoding_and_decoding()
        {
            var allEncodings = new[] { Encoding.ASCII, Encoding.Unicode, Encoding.UTF8, /*Encoding.UTF7, Encoding.UTF32*/ };

            var inputBytesByEncoding = allEncodings.ToDictionary(x => x, x => x.GetBytes("abc"));

            var encodingPairs = (from inputEncoding in allEncodings
                                 from outputEncoding in allEncodings
                                 select new { inputEncoding, outputEncoding }).ToList();

            var bufferSegmentProvider = new BufferSegmentProvider(1024, 1024);
            var logEvent = new LogEvent(bufferSegmentProvider.GetSegment());
            var output = new StringBuffer(128) { Culture = CultureInfo.InvariantCulture };
            var outputBytes = new byte[12];

            var writtenBytes = 0;

            for (var iter = 0; iter < 5000000; iter++)
            {
                for (var i = 0; i < encodingPairs.Count; i++)
                {
                    logEvent.Initialize(Level.Info, null);
                    output.Clear();

                    var encodingPair = encodingPairs[i];
                    var inputBytes = inputBytesByEncoding[encodingPair.inputEncoding];
                    logEvent.Append(inputBytes, inputBytes.Length, encodingPair.inputEncoding);
                    logEvent.WriteToStringBuffer(output);
                    fixed (byte* o = outputBytes)
                        writtenBytes = output.CopyTo(o, outputBytes.Length, 0, output.Count, encodingPair.outputEncoding);
                }
            }

            Console.OpenStandardOutput().Write(outputBytes, 0, writtenBytes);
        }

        [Test]
        public void should_test_append()
        {

            Console.WriteLine("Starting test");
            var sw = Stopwatch.StartNew();

            Parallel.For(0, _nbThreads, threadId =>
            {
                var logger = LogManager.GetLogger(typeof(IntegrationTests).Name + threadId.ToString());
                for (var i = 0; i < _count; i++)
                {
                    var timestamp = Stopwatch.GetTimestamp();
                    logger.InfoFormat("{0}", timestamp);
                    _enqueueMicros[threadId][i] = ToMicroseconds(Stopwatch.GetTimestamp() - timestamp);
                }
                //                logger.InfoFormat("{0}", (byte)1, (char)1, (short)2, (float)3, 2.0, "", true, TimeSpan.Zero);
            });

            LogManager.Shutdown();
            var throughput = _count / sw.Elapsed.TotalSeconds;

            Console.WriteLine($"Finished test, throughput is: {throughput:N0} msgs/second");

            _performanceAppender.PrintTimeTaken();

            var streamWriter = new StreamWriter(new FileStream("write-times.csv", FileMode.Create));
            foreach (var thread in _enqueueMicros)
            {
                foreach (var timeTaken in thread)
                {
                    streamWriter.WriteLine(timeTaken);
                }
            }
            Console.WriteLine("Printed total time taken csv");
        }

        private static double ToMicroseconds(long ticks)
        {
            return unchecked(ticks * 1000000 / (double)(Stopwatch.Frequency));
        }

        [Test]
        public void should_not_allocate()
        {
            const int count = 1000000;

            GC.Collect(2, GCCollectionMode.Forced, true);
            var gcCount = GC.CollectionCount(0);

            var timer = Stopwatch.StartNew();

            var logger = LogManager.GetLogger(typeof(IntegrationTests));
            for (int k = 0; k < count; k++)
            {
                Thread.Sleep(1);
                logger.Info().Append("Hello").Log();
            }

            LogManager.Shutdown();
            timer.Stop();
            Console.WriteLine("BCL  : {0} us/log", timer.ElapsedMilliseconds * 1000.0 / count);
            Console.WriteLine("GCs  : {0}", GC.CollectionCount(0) - gcCount);
        }
    }
}
