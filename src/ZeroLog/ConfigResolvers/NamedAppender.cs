using System;
using ZeroLog.Appenders;

namespace ZeroLog.ConfigResolvers
{
    public class NamedAppender : IEquatable<NamedAppender>
    {
        public IAppender Appender { get; }
        public string Name { get; }

        public NamedAppender(IAppender appender, string name)
        {
            Appender = appender;
            Name = name;
        }

        public bool Equals(NamedAppender other) => string.Equals(Name, other?.Name);
    }
}