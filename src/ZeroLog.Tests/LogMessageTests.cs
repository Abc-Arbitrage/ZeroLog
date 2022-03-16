using System;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

public abstract partial class LogMessageTests
{
    private const int _bufferLength = 1024;
    private const int _stringCapacity = 4;

    private LogMessage _logMessage;

    [SetUp]
    public void SetUp()
    {
        _logMessage = LogMessage.CreateTestMessage(LogLevel.Info, _bufferLength, _stringCapacity);
    }

    private void ShouldNotAllocate(Action action)
    {
        var output = new char[1024];
        var keyValues = new KeyValueList(1024);

        GcTester.ShouldNotAllocate(
            () =>
            {
                action.Invoke();
                _logMessage.WriteTo(output, ZeroLogConfiguration.Default, LogMessage.FormatType.Formatted, keyValues);
            },
            () => _logMessage.Initialize(null, LogLevel.Info)
        );
    }
}
