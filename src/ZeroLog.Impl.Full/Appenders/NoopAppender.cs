using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public sealed class NoopAppender : Appender
{
    public override void WriteMessage(LoggedMessage message)
    {
    }
}
