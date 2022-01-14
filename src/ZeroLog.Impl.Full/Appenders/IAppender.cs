using System;

namespace ZeroLog.Appenders;

public interface IAppender : IDisposable
{
    void WriteMessage(FormattedLogMessage message);
    void Flush();
}
