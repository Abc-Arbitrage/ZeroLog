using System;
using System.Linq;
using System.Runtime;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    public static class Program
    {
        public static void Main()
        {
            /*
            var test = new PerformanceTests();
            
            test.SetUp();

            try
            {
                test.should_run_test();
            }
            finally
            {
                test.Teardown();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            */

            var testAppender = new TestAppender(false);

            for (int i = 0; i < 1_000; i++)
            {
                LogManager.Initialize(new []{ testAppender }, 1024*100, 1024);
                var logger = LogManager.GetLogger("toto");         
                LogManager.Shutdown();
            }
        }
    }
}