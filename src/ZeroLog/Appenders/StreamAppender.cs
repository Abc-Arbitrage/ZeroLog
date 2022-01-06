using System;
using System.IO;
using System.Text;

namespace ZeroLog.Appenders;

public abstract class StreamAppender : IAppender
{
    private readonly char[] _charBuffer = GC.AllocateUninitializedArray<char>(16 * 1024);
    private readonly byte[] _byteBuffer = GC.AllocateUninitializedArray<byte>(16 * 1024 * 4);

    private readonly PrefixWriter? _prefixWriter;
    protected Stream? _stream;
    protected Encoding _encoding = Encoding.UTF8;

    public StreamAppender(string? prefixPattern)
    {
        if (!string.IsNullOrEmpty(prefixPattern))
            _prefixWriter = new PrefixWriter(prefixPattern);
    }

    public virtual void Dispose()
    {
        _stream?.Dispose();
        _stream = null;
    }

    public virtual void WriteMessage(FormattedLogMessage message)
    {
        // TODO try to do a single Stream.Write call

        if (_stream is null)
            return;

        if (_prefixWriter != null)
        {
            var prefixLength = _prefixWriter.WritePrefix(message.Message, _charBuffer);
            Write(_charBuffer.AsSpan(0, prefixLength));
        }

        Write(message.GetMessage());
        Write(Environment.NewLine);
    }

    private void Write(ReadOnlySpan<char> value)
    {
        if (_stream is null)
            return;

        var byteCount = _encoding.GetBytes(value, _byteBuffer);
        _stream.Write(_byteBuffer, 0, byteCount);
    }

    public virtual void Flush()
    {
        _stream?.Flush();
    }
}
