using System.IO;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// Appenders which writes to a <see cref="System.IO.TextWriter"/>.
/// </summary>
public class TextWriterAppender : Appender
{
    private TextWriter? _textWriter;
    private Formatter? _formatter;

    /// <summary>
    /// The target <see cref="System.IO.TextWriter"/>.
    /// </summary>
    public TextWriter? TextWriter
    {
        get => _textWriter;
        set
        {
            Flush();
            _textWriter = value;
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

        var chars = Formatter.FormatMessage(message);
        writer.Write(chars);
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        TextWriter?.Flush();
        base.Flush();
    }
}
