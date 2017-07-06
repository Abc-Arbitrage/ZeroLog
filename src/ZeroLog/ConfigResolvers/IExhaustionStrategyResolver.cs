namespace ZeroLog.ConfigResolvers
{
    public interface IExhaustionStrategyResolver
    {
        LogEventPoolExhaustionStrategy ResolveExhaustionStrategy(string name);
    }
}