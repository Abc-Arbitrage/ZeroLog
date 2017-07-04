namespace ZeroLog.ConfigResolvers
{
    public interface IConfigurationResolver : ILevelResolver, IAppenderResolver, IExhaustionStrategyResolver
    {
    }
}