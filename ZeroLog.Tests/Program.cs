namespace ZeroLog.Tests
{
    public class Program
    {
        public static void Main(string [] args)
        {
            IntegrationTests test = new IntegrationTests();
            test.SetUp();
            try
            {
                test.should_not_allocate();
            }
            finally
            {
                test.Teardown();
            }
        }
    }
}