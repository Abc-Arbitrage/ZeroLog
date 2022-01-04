namespace ZeroLog
{
    public partial interface ILog
    {
        bool IsEnabled(Level level);
        LogMessage ForLevel(Level level);
    }
}
