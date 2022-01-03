using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ZeroLog.Tests;

[TestFixture]
public unsafe class LogMessageTests
{
    private const int _bufferLength = 1024;
    private const int _stringCapacity = 16;

    private LogMessage _logMessage;
    private GCHandle _bufferHandler;

    [SetUp]
    public void SetUp()
    {
        var buffer = new byte[_bufferLength];
        _bufferHandler = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        var bufferSegment = new BufferSegment((byte*)_bufferHandler.AddrOfPinnedObject().ToPointer(), buffer.Length);
        _logMessage = new LogMessage(bufferSegment, _stringCapacity);
        _logMessage.Initialize(null, Level.Info);
    }

    [TearDown]
    public void Teardown()
    {
        _bufferHandler.Free();
    }

    [Test]
    public void should_append_string()
    {
        _logMessage.InternalAppend("foo");
    }
}
