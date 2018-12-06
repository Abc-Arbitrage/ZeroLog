using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private readonly LogEventArgumentExhaustionStrategy _logEventArgumentExhaustionStrategy;

        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "For compatibility")]
        public BasicResolver(IEnumerable<IAppender> appenders, Level level, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy)
            : this(appenders, level, logEventPoolExhaustionStrategy, LogEventArgumentExhaustionStrategy.Default)
        {
        }

        public BasicResolver(IEnumerable<IAppender> appenders, Level level, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, LogEventArgumentExhaustionStrategy logEventArgumentExhaustionStrategy)
        {
            _level = level;
            _logEventPoolExhaustionStrategy = logEventPoolExhaustionStrategy;
            _logEventArgumentExhaustionStrategy = logEventArgumentExhaustionStrategy;
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
                LogEventArgumentExhaustionStrategy = _logEventArgumentExhaustionStrategy
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
