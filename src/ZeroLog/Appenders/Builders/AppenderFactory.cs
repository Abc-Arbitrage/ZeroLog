using System.Collections.Generic;
using System.Linq;
using ZeroLog.Config;

namespace ZeroLog.Appenders.Builders
{
    public class AppenderFactory : IAppenderFactory
    {
        private readonly IDictionary<string, IAppenderBuilder> _builders;

        public AppenderFactory(params IAppenderBuilder[] builders)
        {
            _builders = builders.ToDictionary(x => x.TypeName);
        }

        public IAppender BuildAppender(AppenderDefinition definition)
        {
            return _builders[definition.AppenderTypeName].BuildAppender(definition.Name, definition.AppenderJsonConfig);
        }
    }
}
