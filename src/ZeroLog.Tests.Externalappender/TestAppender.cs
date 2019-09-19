using System;
using ZeroLog.Appenders;

namespace ZeroLog.Tests.ExternalAppender
{
    public class TestAppender : ConsoleAppender
    {
        public TestAppender() {}

        public TestAppender(string prefixPattern) : base(prefixPattern)
        {
        }

        public override void Configure(DefaultAppenderConfig parameters)
        {
            parameters.PrefixPattern = String.Concat("(", Name, "): ", parameters.PrefixPattern);
            base.Configure(parameters);
        }
    }
}
