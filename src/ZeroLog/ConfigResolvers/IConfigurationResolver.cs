using System;

namespace ZeroLog.ConfigResolvers
{
    public interface IConfigurationResolver : ILevelResolver, IAppenderResolver, IExhaustionStrategyResolver
    {
        event Action Updated;
    }
}