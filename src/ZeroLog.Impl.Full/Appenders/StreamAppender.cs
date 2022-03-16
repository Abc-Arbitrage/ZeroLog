using System;
using System.IO;
using System.Text;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public abstract class StreamAppender : Appender
{
    private byte[] _byteBuffer = Array.Empty<byte>();

    private Encoding _encoding = Encoding.UTF8;

    protected internal Stream? Stream { get; set; }

    protected internal Encoding Encoding
    {
        get => _encoding;
        set
        {
            _encoding = value;
            UpdateEncodingSpecificData();
        }
    }

    public Formatter Formatter { get; init; } = new DefaultFormatter();

    protected StreamAppender()
    {
        UpdateEncodingSpecificData();
    }

    public override void Dispose()
    {
        Stream?.Dispose();
        Stream = null;

        base.Dispose();
    }

    public override void WriteMessage(FormattedLogMessage message)
    {
        if (Stream is null)
            return;

        var chars = Formatter.FormatMessage(message);
        var byteCount = _encoding.GetBytes(chars, _byteBuffer);
        Stream.Write(_byteBuffer, 0, byteCount);
    }

    public override void Flush()
    {
        Stream?.Flush();
        base.Flush();
    }

    private void UpdateEncodingSpecificData()
    {
        var maxBytes = _encoding.GetMaxByteCount(LogManager.OutputBufferSize);

        if (_byteBuffer.Length < maxBytes)
            _byteBuffer = GC.AllocateUninitializedArray<byte>(maxBytes);
    }
}
