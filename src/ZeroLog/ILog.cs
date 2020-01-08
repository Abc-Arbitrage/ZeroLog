namespace ZeroLog
{
    public partial interface ILog
    {
        bool IsLevelEnabled(Level level);
        ILogEvent ForLevel(Level level);
    }
}
