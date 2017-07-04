namespace ZeroLog.Appenders.Builders
{
    public interface IAppenderBuilder
    {
        string TypeName { get; }
        IAppender BuildAppender(string name, string config);
    }
}