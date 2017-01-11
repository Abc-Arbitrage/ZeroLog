using System;

namespace ZeroLog.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var test = new IntegrationTests();

            test.SetUp();
            try
            {
                test.should_test_encoding_and_decoding();
            }
            finally
            {
                test.Teardown();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}