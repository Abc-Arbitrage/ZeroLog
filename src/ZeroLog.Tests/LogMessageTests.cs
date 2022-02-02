using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

public abstract unsafe partial class LogMessageTests
{
    private const int _bufferLength = 1024;
    private const int _stringCapacity = 4;

    private LogMessage _logMessage;
    private byte* _buffer;

    [SetUp]
    public void SetUp()
    {
        _buffer = (byte*)NativeMemory.Alloc(_bufferLength);

        _logMessage = new LogMessage(new BufferSegment(_buffer, _bufferLength), _stringCapacity);
        _logMessage.Initialize(null, LogLevel.Info);
    }

    [TearDown]
    public void Teardown()
    {
        NativeMemory.Free(_buffer);
    }

    private void ShouldNotAllocate(Action action)
    {
        var output = new char[1024];

        GcTester.ShouldNotAllocate(
            () =>
            {
                action.Invoke();
                _logMessage.WriteTo(output, ZeroLogConfiguration.Default);
            },
            () => _logMessage.Initialize(null, LogLevel.Info)
        );
    }
}
