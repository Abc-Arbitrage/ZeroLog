using System;
using System.IO;
using System.Text;

namespace ZeroLog.Appenders;

public abstract class StreamAppender : Appender
{
    private readonly char[] _charBuffer = GC.AllocateUninitializedArray<char>(LogManager.OutputBufferSize);
    private readonly byte[] _byteBuffer = GC.AllocateUninitializedArray<byte>(4 * LogManager.OutputBufferSize);

    private PrefixWriter? _prefixWriter;

    protected Stream? Stream { get; set; }
    protected Encoding Encoding { get; set; } = Encoding.UTF8;

    public string PrefixPattern
    {
        get => _prefixWriter?.Pattern ?? string.Empty;
        set => _prefixWriter = !string.IsNullOrEmpty(value) ? new PrefixWriter(value) : null;
    }

    public override void Dispose()
    {
        Stream?.Dispose();
        Stream = null;

        base.Dispose();
    }

    public override void WriteMessage(FormattedLogMessage message)
    {
        // TODO try to do a single Stream.Write call

        if (Stream is null)
            return;

        if (_prefixWriter != null)
        {
            var prefixLength = _prefixWriter.WritePrefix(message, _charBuffer);
            Write(_charBuffer.AsSpan(0, prefixLength));
        }

        Write(message.GetMessage());
        Write(Environment.NewLine);

        if (message.Exception != null)
        {
            // This allocates, but there's no better way to get the details.
            Write(message.Exception.ToString());
            Write(Environment.NewLine);
        }
    }

    private void Write(ReadOnlySpan<char> value)
    {
        if (Stream is null)
            return;

        var byteCount = Encoding.GetBytes(value, _byteBuffer);
        Stream.Write(_byteBuffer, 0, byteCount);
    }

    public override void Flush()
    {
        Stream?.Flush();
        base.Flush();
    }
}
