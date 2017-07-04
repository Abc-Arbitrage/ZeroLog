using System.Linq;
using ZeroLog.Config;

namespace ZeroLog.Appenders.Builders
{
    public class AppenderFactory : IAppenderFactory
    {
        private readonly IAppenderBuilder[] _builders;

        public AppenderFactory(params IAppenderBuilder[] builders)
        {
            _builders = builders;
        }

        public IAppender BuildAppender(AppenderDefinition definition)
        {
            return _builders.Single(x => x.TypeName == definition.AppenderTypeName)
                            .BuildAppender(definition.Name, definition.AppenderJsonConfig);
        }
    }
}
