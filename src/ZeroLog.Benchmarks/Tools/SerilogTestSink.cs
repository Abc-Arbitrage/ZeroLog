using System;
using System.Collections.Generic;
using System.Threading;
using Serilog.Core;
using Serilog.Events;

namespace ZeroLog.Benchmarks.Tools;

public class SerilogTestSink : ILogEventSink
{
    private readonly bool _captureLoggedMessages;
    private readonly object _lock = new();
    private int _messageCount;
    private ManualResetEventSlim _signal;
    private int _messageCountTarget;

    public List<string> LoggedMessages { get; } = new();

    public SerilogTestSink(bool captureLoggedMessages)
    {
        _captureLoggedMessages = captureLoggedMessages;
    }

    public ManualResetEventSlim SetMessageCountTarget(int expectedMessageCount)
    {
        _signal = new ManualResetEventSlim(false);
        _messageCount = 0;
        _messageCountTarget = expectedMessageCount;
        return _signal;
    }

    public void Emit(LogEvent logEvent)
    {
        var formatted = logEvent.RenderMessage();

        lock (_lock)
        {
            if (_captureLoggedMessages)
                LoggedMessages.Add(formatted);

            if (++_messageCount == _messageCountTarget)
                _signal.Set();
        }
    }
}
