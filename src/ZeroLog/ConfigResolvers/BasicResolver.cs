using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroLog.Appenders;

namespace ZeroLog.ConfigResolvers
{
    public class BasicResolver : IConfigurationResolver
    {
        private readonly IAppender[] _appenders;
        private readonly Level _level;
        private readonly LogEventPoolExhaustionStrategy _logEventPoolExhaustionStrategy;

        public BasicResolver(IEnumerable<IAppender> appenders, Level level, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy)
        {
            _level = level;
            _logEventPoolExhaustionStrategy = logEventPoolExhaustionStrategy;
            _appenders = appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))).ToArray<IAppender>();
        }

        public IEnumerable<IAppender> GetAllAppenders() => _appenders;

        public LogConfig ResolveLogConfig(string name)
        {
            return new LogConfig
            {
                Appenders = _appenders,
                Level = _level,
                LogEventPoolExhaustionStrategy = _logEventPoolExhaustionStrategy,
            };
        }

        public void Initialize(Encoding encoding)
        {
            foreach (var appender in _appenders)
            {
                appender.SetEncoding(encoding);
            }
        }

        public event Action Updated = delegate { };

        public void Dispose()
        {
            foreach (var appender in _appenders)
                appender.Dispose();
        }
    }
}
