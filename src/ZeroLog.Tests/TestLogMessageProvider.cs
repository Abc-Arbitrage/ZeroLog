using System;
using NUnit.Framework;
using ZeroLog.Configuration;

namespace ZeroLog.Tests;

#nullable enable

internal class TestLogMessageProvider : ILogMessageProvider
{
    private bool _isAcquired;
    private bool _isSubmitted;
    private readonly LogMessage _message = LogMessage.CreateTestMessage(LogLevel.Info, 128, 16);

    public LogMessage AcquireLogMessage(LogMessagePoolExhaustionStrategy poolExhaustionStrategy)
    {
        if (_isAcquired)
            throw new InvalidOperationException("The message is already acquired");

        _isAcquired = true;
        return _message;
    }

    public void Submit(LogMessage message)
    {
        if (!ReferenceEquals(message, _message))
            throw new InvalidOperationException("Unexpected message submitted");

        if (!_isAcquired)
            throw new InvalidOperationException("Message submitted multiple times");

        _isAcquired = false;
        _isSubmitted = true;
    }

    public LogMessage GetSubmittedMessage()
    {
        if (!_isSubmitted)
            Assert.Fail("No message was submitted");

        return _message;
    }

    public void ShouldNotBeLogged()
    {
        if (_isAcquired || _isSubmitted)
            Assert.Fail("A message has been logged");
    }
}
