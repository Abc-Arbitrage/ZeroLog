using System;
using System.IO;
using System.Text;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// Base class for appenders which write to a <see cref="System.IO.Stream"/>.
/// </summary>
public abstract class StreamAppender : Appender
{
    private byte[] _byteBuffer = Array.Empty<byte>();

    private Encoding _encoding = new UTF8Encoding(false, false);
    private Formatter? _formatter;

    /// <summary>
    /// The stream to write to.
    /// </summary>
    protected internal Stream? Stream { get; set; }

    /// <summary>
    /// The encoding to use when writing to the stream.
    /// </summary>
    protected internal Encoding Encoding
    {
        get => _encoding;
        set
        {
            _encoding = value;
            UpdateEncodingSpecificData();
        }
    }

    /// <summary>
    /// The formatter to use to convert log messages to text.
    /// </summary>
    public Formatter Formatter
    {
        get => _formatter ??= new DefaultFormatter();
        init => _formatter = value;
    }

    /// <summary>
    /// Initializes a new instance of the stream appender.
    /// </summary>
    protected StreamAppender()
    {
        UpdateEncodingSpecificData();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Stream?.Dispose();
        Stream = null;

        base.Dispose();
    }

    /// <inheritdoc/>
    public override void WriteMessage(LoggedMessage message)
    {
        if (Stream is null)
            return;

        var chars = Formatter.FormatMessage(message);
        var byteCount = _encoding.GetBytes(chars, _byteBuffer);
        Stream.Write(_byteBuffer, 0, byteCount);
    }

    /// <inheritdoc/>
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
