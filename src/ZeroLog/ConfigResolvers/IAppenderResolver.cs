using System.Collections.Generic;
using System.Text;
using ZeroLog.Appenders;

namespace ZeroLog.ConfigResolvers
{
    public interface IAppenderResolver
    {
        IList<IAppender> ResolveAppenders(string name);
        void Initialize(Encoding encoding);
    }
}