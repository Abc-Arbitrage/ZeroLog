using JetBrains.Annotations;
using ZeroLog.Appenders;

namespace ZeroLog.Tests.ExternalAppender
{
    [UsedImplicitly]
    public class TestAppender : ConsoleAppender
    {
        public TestAppender()
        {
        }

        public TestAppender(string prefixPattern)
        {
            PrefixPattern = prefixPattern;
        }
    }
}
