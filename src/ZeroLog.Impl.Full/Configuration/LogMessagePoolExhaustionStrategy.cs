namespace ZeroLog.Configuration;

/// <summary>
/// The strategy to apply upon log message pool exhaustion.
/// </summary>
public enum LogMessagePoolExhaustionStrategy
{
    /// <summary>
    /// Drop the log message and log an error instead.
    /// </summary>
    DropLogMessageAndNotifyAppenders = 0,

    /// <summary>
    /// Forget about the message.
    /// </summary>
    DropLogMessage = 1,

    /// <summary>
    /// Block until it's possible to log. This can potentially lock the application if log messages are never released back to the pool.
    /// </summary>
    WaitUntilAvailable = 2,

    /// <summary>
    /// The default value is <see cref="DropLogMessageAndNotifyAppenders"/>.
    /// </summary>
    Default = DropLogMessageAndNotifyAppenders
}
