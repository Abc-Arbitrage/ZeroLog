using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ZeroLog;

/// <summary>
/// Represents a log message being built.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
public sealed partial class LogMessage
{
    /// <summary>
    /// The message log level (severity).
    /// </summary>
    public LogLevel Level { get; private set; }

    /// <summary>
    /// The message timestamp in UTC.
    /// </summary>
    public DateTime Timestamp { get; internal set; }

    /// <summary>
    /// The thread which logged this message.
    /// </summary>
    public Thread? Thread { get; private set; }

    /// <summary>
    /// The exception associated with this message.
    /// </summary>
    public Exception? Exception { get; internal set; }

    /// <summary>
    /// Associates an exception with this message.
    /// </summary>
    /// <remarks>
    /// Only a single exception can be associated with a message.
    /// </remarks>
    /// <param name="exception">The exception to associate with the message.</param>
    public LogMessage WithException(Exception? exception)
    {
        Exception = exception;
        return this;
    }

    /// <summary>
    /// Logs this message.
    /// </summary>
    /// <remarks>
    /// The message instance should no longer be used after being logged.
    /// </remarks>
    public partial void Log();

    /// <summary>
    /// Returns the logged message as a string.
    /// </summary>
    public override partial string ToString();

#if NETSTANDARD

    internal static readonly LogMessage Empty = new();

    private LogMessage()
    {
    }

    public partial void Log()
    {
    }

    public override partial string ToString()
        => string.Empty;

#endif

    /// <summary>
    /// An interpolated string handler used to append values to log messages.
    /// </summary>
    [InterpolatedStringHandler]
    public readonly ref partial struct AppendInterpolatedStringHandler
    {
        private readonly LogMessage _message;

        /// <summary>
        /// Creates the interpolated string handler.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        public AppendInterpolatedStringHandler(int literalLength, int formattedCount, LogMessage message)
            => _message = message;

        /// <summary>
        /// Appends a literal string to the handler.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void AppendLiteral(string value)
            => _message.InternalAppendString(value);

        /// <summary>
        /// Appends a string to the handler.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void AppendFormatted(string? value)
            => _message.InternalAppendString(value);

        /// <summary>
        /// Appends a string represented by a <c>ReadOnlySpan&lt;char&gt;</c> to the handler.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void AppendFormatted(ReadOnlySpan<char> value)
            => _message.InternalAppendStringSpan(value);

        /// <summary>
        /// Appends an UTF-8 string represented by a <c>ReadOnlySpanReadOnlySpan&lt;byte&gt;</c> to the handler.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void AppendFormatted(ReadOnlySpan<byte> value)
            => _message.InternalAppendUtf8StringSpan(value);

        /// <summary>
        /// Appends an enum to the handler.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void AppendFormatted<T>(T value)
            where T : struct, Enum
            => _message.InternalAppendEnum(value);

        /// <summary>
        /// Appends a nullable enum to the handler.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void AppendFormatted<T>(T? value)
            where T : struct, Enum
            => _message.InternalAppendEnum(value);

        /// <summary>
        /// Executes an append operation on the underlying log message.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        public void AppendFormatted<T>(AppendOperation<T> operation)
            => operation.AppendAction?.Invoke(_message, operation.Value);
    }

    /// <summary>
    /// An append operation used for indirections.
    /// </summary>
    /// <typeparam name="T">The value type to append.</typeparam>
    public readonly struct AppendOperation<T>
    {
        /// <summary>
        /// The value to append.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// The action called to append the value.
        /// </summary>
        public Action<LogMessage, T?>? AppendAction { get; }

        /// <summary>
        /// Creates an indirect append operation.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <param name="appendAction">The action called to append the value. This should be a static delegate instance.</param>
        public AppendOperation(T? value, Action<LogMessage, T?> appendAction)
        {
            Value = value;
            AppendAction = appendAction;
        }

        /// <summary>
        /// Returns a string that represents the value to append.
        /// </summary>
        public override string ToString()
            => Value?.ToString() ?? string.Empty;
    }
}
