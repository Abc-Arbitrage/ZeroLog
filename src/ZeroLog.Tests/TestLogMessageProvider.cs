using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ZeroLog.Tests;

#nullable enable

internal unsafe class TestLogMessageProvider : ILogMessageProvider, IDisposable
{
    private byte* _buffer;
    private bool _isAcquired;
    private bool _isSubmitted;
    private LogMessage? _message;

    public TestLogMessageProvider()
    {
        const int bufferSize = 128;
        _buffer = (byte*)NativeMemory.Alloc(bufferSize);
        _message = new LogMessage(new BufferSegment(_buffer, bufferSize), 16);
    }

    public void Dispose()
    {
        _message = null;
        NativeMemory.Free(_buffer);
        _buffer = null;
    }

    public LogMessage TryAcquireLogMessage()
    {
        if (_message is null)
            throw new ObjectDisposedException("The provider is disposed");

        if (_isAcquired)
            throw new InvalidOperationException("The message is already acquired");

        _isAcquired = true;
        return _message;
    }

    public void Submit(LogMessage message)
    {
        if (_message is null)
            throw new ObjectDisposedException("The provider is disposed");

        if (!ReferenceEquals(message, _message))
            throw new InvalidOperationException("Unexpected message submitted");

        if (!_isAcquired)
            throw new InvalidOperationException("Message submitted multiple times");

        _isAcquired = false;
        _isSubmitted = true;
    }

    public LogMessage GetSubmittedMessage()
    {
        if (_message is null)
            throw new ObjectDisposedException("The provider is disposed");

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
