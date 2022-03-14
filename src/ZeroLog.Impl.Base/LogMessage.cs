using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ZeroLog;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public sealed partial class LogMessage
{
    public LogLevel Level { get; private set; }
    public DateTime Timestamp { get; internal set; }
    public Thread? Thread { get; private set; }
    public Exception? Exception { get; internal set; }

    public LogMessage WithException(Exception? exception)
    {
        Exception = exception;
        return this;
    }

    public partial void Log();

#if NETSTANDARD

    internal static readonly LogMessage Empty = new();

    private LogMessage()
    {
    }

    public partial void Log()
    {
    }

    public override string ToString()
        => string.Empty;

#endif

    [InterpolatedStringHandler]
    public readonly ref partial struct AppendInterpolatedStringHandler
    {
        private readonly LogMessage _message;

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        public AppendInterpolatedStringHandler(int literalLength, int formattedCount, LogMessage message)
            => _message = message;

        public void AppendLiteral(string value)
            => _message.InternalAppendString(value);

        public void AppendFormatted(string? value)
            => _message.InternalAppendString(value);

        public void AppendFormatted<T>(T value)
            where T : struct, Enum
            => _message.InternalAppendEnum(value);

        public void AppendFormatted<T>(T? value)
            where T : struct, Enum
            => _message.InternalAppendEnum(value);

        public void AppendFormatted<T>(AppendOperation<T> operation)
            => operation.AppendAction?.Invoke(_message, operation.Value);
    }

    public readonly struct AppendOperation<T>
    {
        public T? Value { get; }
        public Action<LogMessage, T?>? AppendAction { get; }

        public AppendOperation(T? value, Action<LogMessage, T?> appendAction)
        {
            Value = value;
            AppendAction = appendAction;
        }

        public override string ToString()
            => Value?.ToString() ?? string.Empty;
    }
}
