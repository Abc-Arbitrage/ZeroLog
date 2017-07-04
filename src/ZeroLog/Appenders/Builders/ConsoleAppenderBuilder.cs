namespace ZeroLog.Appenders.Builders
{
    public class ConsoleAppenderBuilder : IAppenderBuilder
    {
        public string TypeName 
            => nameof(ConsoleAppender);

        public IAppender BuildAppender(string name, string config)
            => new ConsoleAppender(JSONExtensions.DeserializeOrDefault(config, new Config()).PrefixPattern);

        public class Config
        {
            public string PrefixPattern { get; set; } = ConsoleAppender.DefaultPrefixPattern;
        }

    }
}