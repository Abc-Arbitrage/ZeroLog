namespace ZeroLog;

/// <summary>
/// Represents a log severity level.
/// </summary>
public enum LogLevel
{
    // Same level values as in Microsoft.Extensions.Logging.LogLevel, but with different names

    /// <summary>
    /// The most detailed log level.
    /// </summary>
    Trace,

    /// <summary>
    /// Debugging information.
    /// </summary>
    Debug,

    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warn,

    /// <summary>
    /// Error message.
    /// </summary>
    Error,

    /// <summary>
    /// Critical error message.
    /// </summary>
    Fatal,

    /// <summary>
    /// Represents a disabled log level.
    /// </summary>
    None
}
