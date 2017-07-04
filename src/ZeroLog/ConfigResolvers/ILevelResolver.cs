namespace ZeroLog.ConfigResolvers
{
    public interface ILevelResolver
    {
        Level ResolveLevel(string name);
    }
}
