using System.Collections.Generic;
using System.Threading;
using NLog;
using NLog.Common;
using NLog.Targets;

namespace ZeroLog.Benchmarks;

[Target("NLogTestTarget")]
internal class NLogTestTarget : NLog.Targets.TargetWithLayout
{
    private readonly bool _captureLoggedMessages;
    private int _messageCount;
    private ManualResetEventSlim _signal;
    private int _messageCountTarget;

    public NLogTestTarget(bool captureLoggedMessages)
    {
        _captureLoggedMessages = captureLoggedMessages;
    }

    public List<string> LoggedMessages { get; } = new List<string>();

    public ManualResetEventSlim SetMessageCountTarget(int expectedMessageCount)
    {
        _signal = new ManualResetEventSlim(false);
        _messageCount = 0;
        _messageCountTarget = expectedMessageCount;
        return _signal;
    }

    protected override void Write(LogEventInfo logEvent)
    {
        string logMessage = this.Layout.Render(logEvent);

        if (_captureLoggedMessages)
            LoggedMessages.Add(logMessage);

        if (++_messageCount == _messageCountTarget)
            _signal.Set();
    }
}
