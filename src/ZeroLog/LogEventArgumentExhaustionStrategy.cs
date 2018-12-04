namespace ZeroLog
{
    public enum LogEventArgumentExhaustionStrategy
    {
        TruncateMessage = 0,
        Allocate = 1,
    
        Default = TruncateMessage
    }
}
