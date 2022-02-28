using System;
using System.Runtime.InteropServices;

namespace ZeroLog.Benchmarks.Logging;

internal unsafe class BenchmarkLogMessageProvider : ILogMessageProvider, IDisposable
{
    private readonly LogMessage _logMessage;
    private readonly byte* _buffer;

    public BenchmarkLogMessageProvider(uint logMessageBufferSize = 128, uint logMessageStringCapacity = 32)
    {
        _buffer = (byte*)NativeMemory.Alloc(logMessageBufferSize);
        var bufferSegment = new BufferSegment(_buffer, (int)logMessageBufferSize);
        _logMessage = new LogMessage(bufferSegment, (int)logMessageStringCapacity);
    }

    public LogMessage TryAcquireLogMessage() => _logMessage;

    public void Submit(LogMessage message)
    {
    }

    ~BenchmarkLogMessageProvider()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        NativeMemory.Free(_buffer);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}
