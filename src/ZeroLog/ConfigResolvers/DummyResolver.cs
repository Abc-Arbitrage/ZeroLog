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
        private readonly int _logEventQueueSize;
        private readonly int _logEventBufferSize;

        public DummyResolver(IEnumerable<IAppender> appenders, Level level, LogEventPoolExhaustionStrategy strategy, int logEventQueueSize = 1024, int logEventBufferSize = 128)
        {
            _level = level;
            _strategy = strategy;
            _logEventQueueSize = logEventQueueSize;
            _logEventBufferSize = logEventBufferSize;
            _appenders = new List<IAppender>(appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));
        }
        
        public IList<IAppender> ResolveAppenders(string name) => _appenders;

        public Level ResolveLevel(string name) => _level;

        public LogEventPoolExhaustionStrategy ResolveExhaustionStrategy(string name) => _strategy;

        public int LogEventBufferSize => _logEventBufferSize;

        public int LogEventQueueSize => _logEventQueueSize;


        public void Initialize(Encoding encoding)
        {
            _appenders = new List<IAppender>(_appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));

            foreach (var appender in _appenders)
                appender.SetEncoding(encoding);
        }

        public event Action Updated = delegate {};

        public void Dispose()
        {
            foreach (var appender in _appenders)
                appender.Close();
        }
    }
}