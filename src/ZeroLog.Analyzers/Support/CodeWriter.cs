using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ZeroLog.Analyzers.Support;

internal sealed class CodeWriter
{
    private const int _indentWidth = 4;

    private readonly StringBuilder _sb = new();
    private bool _isAtStartOfLine = true;

    public int Indent { get; set; }

    public CodeWriter Append<T>(T? value)
        => Append(value?.ToString());

    public CodeWriter AppendLine<T>(T? value)
        => AppendLine(value?.ToString());

    public CodeWriter Append([StringSyntax("csharp")] string? value)
    {
        if (value is null or "")
            return this;

        var position = 0;

        while (value.IndexOf('\n', position) is var nextLineFeed and >= 0)
        {
            if (nextLineFeed == position || nextLineFeed == position + 1 && value[position] == '\r')
            {
                _sb.AppendLine();
            }
            else
            {
                AppendPendingIndent();
                _sb.Append(value, position, nextLineFeed + 1 - position);
            }

            _isAtStartOfLine = true;
            position = nextLineFeed + 1;
        }

        if (position < value.Length)
        {
            AppendPendingIndent();
            _sb.Append(value, position, value.Length - position);
        }

        return this;
    }

    public CodeWriter AppendRaw([StringSyntax("csharp")] string? value)
    {
        _sb.Append(value);
        return this;
    }

    public CodeWriter AppendLine([StringSyntax("csharp")] string? value = null)
    {
        Append(value);

        _sb.AppendLine();
        _isAtStartOfLine = true;

        return this;
    }

    public CodeWriter AppendUnindentedLine([StringSyntax("csharp")] string? value)
    {
        if (!_isAtStartOfLine)
            _sb.AppendLine();

        _sb.AppendLine(value);
        _isAtStartOfLine = true;

        return this;
    }

    public CodeWriter Clear()
    {
        _sb.Clear();
        _isAtStartOfLine = true;
        Indent = 0;
        return this;
    }

    private CodeWriter TrimEnd()
    {
        while (_sb.Length > 0 && char.IsWhiteSpace(_sb[_sb.Length - 1]))
            _sb.Remove(_sb.Length - 1, 1);

        return this;
    }

    public CodeWriter TrimComma()
    {
        TrimEnd();

        if (_sb.Length > 0 && _sb[_sb.Length - 1] == ',')
            _sb.Remove(_sb.Length - 1, 1);

        return this;
    }

    public CodeWriter AppendPendingIndent()
    {
        if (_isAtStartOfLine)
            _sb.Append(' ', Indent * _indentWidth);

        _isAtStartOfLine = false;
        return this;
    }

    public override string ToString()
        => _sb.ToString();

    public IndentScope Indented()
    {
        if (!_isAtStartOfLine)
            AppendLine();

        Indent++;
        return new IndentScope(this);
    }

    public BlockScope WriteBlock([StringSyntax("csharp")] string? header = null)
    {
        Append(header);

        if (!_isAtStartOfLine)
            AppendLine();

        AppendLine("{");
        Indent++;
        return new BlockScope(this);
    }

    public readonly struct IndentScope(CodeWriter writer) : IDisposable
    {
        public void Dispose()
        {
            writer.Indent--;
            writer.TrimEnd().AppendLine();
        }
    }

    public readonly struct BlockScope(CodeWriter writer) : IDisposable
    {
        public void Dispose()
        {
            writer.Indent--;
            writer.TrimEnd()
                  .AppendLine()
                  .AppendLine("}");
        }
    }
}
