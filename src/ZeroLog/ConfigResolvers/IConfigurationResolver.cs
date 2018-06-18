using System;
using System.Collections.Generic;
using System.Text;
using ZeroLog.Appenders;

namespace ZeroLog.ConfigResolvers
{
    public interface IConfigurationResolver : IDisposable
    {
        void Initialize(Encoding encoding);

        Level ResolveLevel(string name);
        IEnumerable<IAppender> GetAllAppenders();
        IAppender[] ResolveAppenders(string name);
        LogEventPoolExhaustionStrategy ResolveExhaustionStrategy(string name);

        event Action Updated;
    }
}
