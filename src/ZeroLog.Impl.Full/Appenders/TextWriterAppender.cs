using System;
using System.IO;
using System.Reflection;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// Appenders which writes to a <see cref="System.IO.TextWriter"/>.
/// </summary>
public class TextWriterAppender : Appender
{
    private TextWriter? _textWriter;
    private Formatter? _formatter;
    private bool _useSpanWrite;

    /// <summary>
    /// The target <see cref="System.IO.TextWriter"/>.
    /// </summary>
    public TextWriter? TextWriter
    {
        get => _textWriter;
        set => SetTextWriter(value);
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
    /// Initializes a new instance of the appender.
    /// </summary>
    public TextWriterAppender()
    {
    }

    /// <summary>
    /// Initializes a new instance of the appender.
    /// </summary>
    /// <param name="textWriter">The target <see cref="System.IO.TextWriter"/>.</param>
    public TextWriterAppender(TextWriter? textWriter)
    {
        TextWriter = textWriter;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        base.Dispose();
        TextWriter?.Dispose();
    }

    /// <inheritdoc/>
    public override void WriteMessage(LoggedMessage message)
    {
        if (TextWriter is not { } writer)
            return;

        // Same logic as in StreamAppender: use the ROS<char> overload if it's overridden, otherwise use the older overload.
        if (_useSpanWrite)
        {
            var chars = Formatter.FormatMessage(message);
            writer.Write(chars);
        }
        else
        {
            Formatter.FormatMessage(message);
            var buffer = Formatter.GetBuffer(out var length);
            writer.Write(buffer, 0, length);
        }
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        TextWriter?.Flush();
        base.Flush();
    }

    private void SetTextWriter(TextWriter? newTextWriter)
    {
        Flush();

        var isSameType = _textWriter?.GetType() == newTextWriter?.GetType();
        _textWriter = newTextWriter;

        if (!isSameType)
            _useSpanWrite = _textWriter is { } textWriter && OverridesSpanWrite(textWriter.GetType());
    }

    internal static bool OverridesSpanWrite(Type textWriterType)
        => textWriterType.GetMethod(nameof(System.IO.TextWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(ReadOnlySpan<char>)])?.DeclaringType == textWriterType;
}
