using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// An appender which does nothing.
/// </summary>
public sealed class NoopAppender : Appender
{
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="message">The message to ignore.</param>
    public override void WriteMessage(LoggedMessage message)
    {
    }
}
