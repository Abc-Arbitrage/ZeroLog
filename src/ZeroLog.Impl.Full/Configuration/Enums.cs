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
    /// Allocates a new log message.
    /// </summary>
    Allocate = 3,

    /// <summary>
    /// The default value is <see cref="DropLogMessageAndNotifyAppenders"/>.
    /// </summary>
    Default = DropLogMessageAndNotifyAppenders
}

/// <summary>
/// Specifies the way log messages are formatted and passed to appenders.
/// </summary>
public enum AppendingStrategy
{
    /// <summary>
    /// Use a dedicated thread to format log messages and write them to appenders.
    /// </summary>
    /// <remarks>
    /// Intended for production. Ensures minimal overhead on the thread logging a message.
    /// </remarks>
    Asynchronous,

    /// <summary>
    /// Use the current thread to format log messages and write them to appenders.
    /// </summary>
    /// <remarks>
    /// Intended for unit testing. Can cause contention if several threads try to log messages simultaneously.
    /// </remarks>
    Synchronous
}
