using System;

namespace ZeroLog.ConfigResolvers
{
    public interface IConfigurationResolver : ILevelResolver, IAppenderResolver, IExhaustionStrategyResolver, IDisposable
    {
        event Action Updated;
    }
}