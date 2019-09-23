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
            : base(prefixPattern)
        {
        }

        public override void Configure(DefaultAppenderConfig parameters)
        {
            parameters.PrefixPattern = $"({Name}): {parameters.PrefixPattern}";
            base.Configure(parameters);
        }
    }
}
