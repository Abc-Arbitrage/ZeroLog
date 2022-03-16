using System;
using System.Diagnostics;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// An appender which handles logged messages.
/// </summary>
/// <remarks>
/// Messages are handled by appenders on a dedicated thread.
/// </remarks>
public abstract class Appender : IDisposable
{
    private readonly Stopwatch _quarantineStopwatch = new();
    private bool _needsFlush;

    /// <summary>
    /// The minimum log level of messages this appender should handle.
    /// </summary>
    public LogLevel Level { get; init; }

    /// <summary>
    /// Handles a logged message.
    /// </summary>
    /// <param name="message">The logged message.</param>
    public abstract void WriteMessage(LoggedMessage message);

    /// <summary>
    /// Flushes the appender.
    /// </summary>
    /// <remarks>
    /// This is called each time the message queue is empty after a message has been handled.
    /// </remarks>
    public virtual void Flush()
    {
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public virtual void Dispose()
    {
    }

    internal void InternalWriteMessage(LoggedMessage message, ZeroLogConfiguration config)
    {
        if (_quarantineStopwatch.IsRunning && _quarantineStopwatch.Elapsed < config.AppenderQuarantineDelay)
            return;

        try
        {
            _needsFlush = true;
            WriteMessage(message);
            _quarantineStopwatch.Stop();
        }
        catch
        {
            _quarantineStopwatch.Restart();
        }
    }

    internal void InternalFlush()
    {
        if (!_needsFlush)
            return;

        try
        {
            _needsFlush = false;
            Flush();
        }
        catch
        {
            _quarantineStopwatch.Restart();
        }
    }
}
