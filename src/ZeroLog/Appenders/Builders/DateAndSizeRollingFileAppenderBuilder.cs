using System;
using Jil;

namespace ZeroLog.Appenders.Builders
{
    public class DateAndSizeRollingFileAppenderBuilder : IAppenderBuilder
    {
        public string TypeName 
            => nameof(DateAndSizeRollingFileAppender);

        public IAppender BuildAppender(string name, string configJson)
        {
            var config = JSONExtensions.DeserializeOrDefault(configJson, new Config());
            return new DateAndSizeRollingFileAppender(config.FilepathRoot, config.MaxFileSizeInBytes, config.Extension, config.PrefixPattern);
        }

        public class Config
        {
            public string FilepathRoot { get; set; }
            public string Extension { get; set; } = DateAndSizeRollingFileAppender.DefaultExtension;
            public int MaxFileSizeInBytes { get; set; } = DateAndSizeRollingFileAppender.DefaultMaxSize;
            public string PrefixPattern { get; set; } = DateAndSizeRollingFileAppender.DefaultPrefixPattern;
        }
    }
}