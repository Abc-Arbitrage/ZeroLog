using System;
using System.Collections.Generic;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog.ConfigResolvers
{
    public class BasicResolver : IConfigurationResolver
    {
        private readonly IAppender[] _appenders;
        private readonly Level _level;
        private readonly LogMessagePoolExhaustionStrategy _logMessagePoolExhaustionStrategy;

        public BasicResolver(IEnumerable<IAppender> appenders, Level level, LogMessagePoolExhaustionStrategy logMessagePoolExhaustionStrategy)
        {
            _level = level;
            _logMessagePoolExhaustionStrategy = logMessagePoolExhaustionStrategy;
            _appenders = appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))).ToArray<IAppender>();
        }

        public IEnumerable<IAppender> GetAllAppenders() => _appenders;

        public LogConfig ResolveLogConfig(string name)
        {
            return new LogConfig
            {
                Appenders = _appenders,
                Level = _level,
                LogMessagePoolExhaustionStrategy = _logMessagePoolExhaustionStrategy
            };
        }

        public event Action? Updated
        {
            add { }
            remove { }
        }

        public void Dispose()
        {
            foreach (var appender in _appenders)
                appender.Dispose();
        }
    }
}
