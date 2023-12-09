using System;
using System.IO;
using System.Reflection;
using System.Text;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// Base class for appenders which write to a <see cref="System.IO.Stream"/>.
/// </summary>
public abstract class StreamAppender : Appender
{
    private byte[] _byteBuffer = [];

    private readonly Formatter _formatter = new DefaultFormatter();
    private Encoding _encoding = Encoding.UTF8;
    private Utf8Formatter? _utf8Formatter;
    private bool _useSpanGetBytes;
    private bool _allowUtf8Formatter = true;

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
            if (ReferenceEquals(value, _encoding))
                return;

            _encoding = value;
            UpdateEncodingSpecificData();
        }
    }

    /// <summary>
    /// The formatter to use to convert log messages to text.
    /// </summary>
    public Formatter Formatter
    {
        get => _formatter;
        init
        {
            if (ReferenceEquals(value, _formatter))
                return;

            _formatter = value;
            UpdateEncodingSpecificData();
        }
    }

    /// <summary>
    /// For benchmarks.
    /// </summary>
    internal bool AllowUtf8Formatter
    {
        get => _allowUtf8Formatter;
        set
        {
            _allowUtf8Formatter = value;
            UpdateEncodingSpecificData();
        }
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
        if (Stream is not { } stream)
            return;

        if (_utf8Formatter is { } utf8Formatter)
        {
            stream.Write(utf8Formatter.FormatMessage(message));
        }
        else if (_useSpanGetBytes)
        {
            var chars = _formatter.FormatMessage(message);
            var byteCount = _encoding.GetBytes(chars, _byteBuffer);
            stream.Write(_byteBuffer, 0, byteCount);
        }
        else
        {
            _formatter.FormatMessage(message);
            var charBuffer = _formatter.GetBuffer(out var charCount);
            var byteCount = _encoding.GetBytes(charBuffer, 0, charCount, _byteBuffer, 0);
            stream.Write(_byteBuffer, 0, byteCount);
        }
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        Stream?.Flush();
        base.Flush();
    }

    private void UpdateEncodingSpecificData()
    {
        if (_encoding is UTF8Encoding && _formatter.AsUtf8Formatter() is { } utf8Formatter && _allowUtf8Formatter)
        {
            // Fast path
            _utf8Formatter = utf8Formatter;
            return;
        }

        _utf8Formatter = null;

        // The base Encoding class allocates buffers in all non-abstract GetBytes overloads in order to call the abstract
        // GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) in the end.
        // If an encoding overrides the Span version of GetBytes, we assume it avoids this allocation
        // and it skips safety checks as those are guaranteed by the Span struct. In that case, we can call this overload directly.
        _useSpanGetBytes = OverridesSpanGetBytes(_encoding.GetType());

        var maxBytes = _encoding.GetMaxByteCount(LogManager.OutputBufferSize);

        if (_byteBuffer.Length < maxBytes)
            _byteBuffer = GC.AllocateUninitializedArray<byte>(maxBytes);
    }

    internal static bool OverridesSpanGetBytes(Type encodingType)
        => encodingType.GetMethod(nameof(System.Text.Encoding.GetBytes), BindingFlags.Public | BindingFlags.Instance, [typeof(ReadOnlySpan<char>), typeof(Span<byte>)])?.DeclaringType == encodingType;
}
