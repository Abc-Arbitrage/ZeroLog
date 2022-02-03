namespace ZeroLog.Configuration;

public enum LogMessagePoolExhaustionStrategy
{
    DropLogMessageAndNotifyAppenders = 0,
    DropLogMessage = 1,
    WaitUntilAvailable = 2,

    Default = DropLogMessageAndNotifyAppenders,
}
