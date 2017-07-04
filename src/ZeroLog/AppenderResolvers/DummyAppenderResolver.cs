using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroLog.Appenders;

namespace ZeroLog.AppenderResolvers
{
    public class DummyAppenderResolver : IAppenderResolver
    {
        private IList<IAppender> _appenders;

        public DummyAppenderResolver(IEnumerable<IAppender> appenders, Encoding encoding)
        {
            _appenders = new List<IAppender>(appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));

            foreach (var appender in _appenders)
            {
                appender.SetEncoding(encoding);
            }
        }

        public IList<IAppender> Resolve(string name) => _appenders;

        public void Initialize(Encoding encoding)
        {
            _appenders = new List<IAppender>(_appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));

            foreach (var appender in _appenders)
                appender.SetEncoding(encoding);
        }
    }
}