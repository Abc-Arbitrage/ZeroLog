using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroLog.Appenders;

namespace ZeroLog.ConfigResolvers
{
    public class DummyResolver : IConfigurationResolver
    {
        private IList<IAppender> _appenders;
        private readonly Level _level;
        private readonly LogEventPoolExhaustionStrategy _strategy;

        public DummyResolver(IEnumerable<IAppender> appenders, Level level, LogEventPoolExhaustionStrategy strategy)
        {
            _level = level;
            _strategy = strategy;
            _appenders = new List<IAppender>(appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));
        }

        public IList<IAppender> ResolveAppenders(string name) => _appenders;
        public Level ResolveLevel(string name) => _level;
        public LogEventPoolExhaustionStrategy ResolveExhaustionStrategy(string name) => _strategy;

        public void Initialize(Encoding encoding)
        {
            _appenders = new List<IAppender>(_appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));

            foreach (var appender in _appenders)
            {
                appender.SetEncoding(encoding);
            }
        }

        public event Action Updated = delegate {};

        public void Dispose()
        {
            foreach (var appender in _appenders)
            {
                appender.Close();
            }
        }
    }
}