namespace ZeroLog
{
    public enum LogEventPoolExhaustionStrategy
    {
        DropLogMessageAndNotifyAppenders = 0,
        DropLogMessage = 1,
        WaitForLogEvent = 2,
        
        Default = DropLogMessageAndNotifyAppenders,
    }
}
