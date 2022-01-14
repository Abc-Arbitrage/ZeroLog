namespace ZeroLog.Appenders;

public class NoopAppender : IAppender
{
    public void Dispose()
    {
    }

    public void WriteMessage(FormattedLogMessage message)
    {
    }

    public void Flush()
    {
    }
}
