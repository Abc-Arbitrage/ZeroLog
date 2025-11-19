using System;
using System.Diagnostics.CodeAnalysis;
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

    private Encoding _encoding = Encoding.UTF8;
    private bool _useSpanGetBytes;
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

        if (_useSpanGetBytes)
        {
            var chars = Formatter.FormatMessage(message);
            var byteCount = _encoding.GetBytes(chars, _byteBuffer);
            Stream.Write(_byteBuffer, 0, byteCount);
        }
        else
        {
            Formatter.FormatMessage(message);
            var charBuffer = Formatter.GetBuffer(out var charCount);
            var byteCount = _encoding.GetBytes(charBuffer, 0, charCount, _byteBuffer, 0);
            Stream.Write(_byteBuffer, 0, byteCount);
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
        var maxBytes = _encoding.GetMaxByteCount(LogManager.OutputBufferSize);

        // The base Encoding class allocates buffers in all non-abstract GetBytes overloads in order to call the abstract
        // GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) in the end.
        // If an encoding overrides the Span version of GetBytes, we assume it avoids this allocation
        // and it skips safety checks as those are guaranteed by the Span struct. In that case, we can call this overload directly.
        _useSpanGetBytes = UseSpanGetBytesOverload(_encoding);

        if (_byteBuffer.Length < maxBytes)
            _byteBuffer = GC.AllocateUninitializedArray<byte>(maxBytes);
    }

    internal static bool UseSpanGetBytesOverload(Encoding encoding)
    {
        // These are known to be good.
        if (encoding is UTF8Encoding or ASCIIEncoding || ReferenceEquals(encoding, Encoding.Latin1))
            return true;

        return UseSpanGetBytesOverload(encoding.GetType());
    }

    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2070", Justification = "Returning false is OK, it just skips an optimization.")]
    internal static bool UseSpanGetBytesOverload(Type encodingType)
    {
        var declaringType = encodingType.GetMethod(nameof(System.Text.Encoding.GetBytes), BindingFlags.Public | BindingFlags.Instance, [typeof(ReadOnlySpan<char>), typeof(Span<byte>)])?.DeclaringType;
        return declaringType is not null && declaringType != typeof(Encoding);
    }
}
