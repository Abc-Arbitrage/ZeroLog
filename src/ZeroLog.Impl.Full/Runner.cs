using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Support;

namespace ZeroLog;

internal abstract class Runner : ILogMessageProvider, IDisposable
{
    private readonly ObjectPool<LogMessage> _pool;

    protected ZeroLogConfiguration _config;
    protected Appender[] _appenders;

    protected bool _isRunning;

    protected Runner(ZeroLogConfiguration config)
    {
        _config = config;

        var bufferSegmentProvider = new BufferSegmentProvider(config.LogMessagePoolSize, config.LogMessageBufferSize);
        _pool = new ObjectPool<LogMessage>(config.LogMessagePoolSize, () => new LogMessage(bufferSegmentProvider.GetSegment(), config.LogMessageStringCapacity));

        _appenders = config.GetAllAppenders().ToArray();

        _isRunning = true;
    }

    public void Dispose()
    {
        if (!_isRunning)
            return;

        _isRunning = false;

        _pool.Clear();
        Stop();
        _pool.Clear();

        if (_pool.IsAnyItemAcquired())
            Thread.Sleep(100); // Can't really do much better here

        foreach (var appender in _appenders)
            appender.Dispose();

        _appenders = Array.Empty<Appender>();
    }

    public LogMessage? TryAcquireLogMessage()
    {
        if (_pool.TryAcquire(out var message))
        {
            message.ReturnToPool = true;
            return message;
        }

        if (!_isRunning)
            return LogMessage.Empty;

        return null;
    }

    public abstract void Submit(LogMessage message);

    public abstract void UpdateConfiguration(ZeroLogConfiguration config);
    protected abstract void Stop();

    internal abstract void WaitUntilNewConfigurationIsApplied(); // For unit tests

    protected void ReleaseAfterProcessing(LogMessage message)
    {
        if (message.ReturnToPool && _isRunning)
        {
            message.ReturnToPool = false;
            _pool.Release(message);
        }
    }
}

internal sealed class AsyncRunner : Runner
{
    private readonly ConcurrentQueue<LogMessage> _queue;
    private readonly Thread _thread;
    private readonly LoggedMessage _loggedMessage;

    private ZeroLogConfiguration? _nextConfig;

    public AsyncRunner(ZeroLogConfiguration config)
        : base(config)
    {
        _queue = new ConcurrentQueue<LogMessage>(new ConcurrentQueueCapacityInitializer(config.LogMessagePoolSize));

        _loggedMessage = new LoggedMessage(LogManager.OutputBufferSize, _config);

        _thread = new Thread(WriteThread)
        {
            Name = $"{nameof(ZeroLog)}.{nameof(AsyncRunner)}",
            IsBackground = _config.UseBackgroundThread
        };

        _thread.Start();
    }

    protected override void Stop()
        => _thread.Join();

    public override void Submit(LogMessage message)
        => _queue.Enqueue(message);

    public override void UpdateConfiguration(ZeroLogConfiguration config)
        => Volatile.Write(ref _nextConfig, config);

    internal override void WaitUntilNewConfigurationIsApplied()
    {
        while (Volatile.Read(ref _nextConfig) != null)
            Thread.Yield();
    }

    private void WriteThread()
    {
        try
        {
            WriteToAppenders();
        }
        catch (Exception ex)
        {
            try
            {
                Console.Error.WriteLine("Fatal error in ZeroLog." + nameof(WriteThread) + ":");
                Console.Error.WriteLine(ex);

                Dispose();
            }
            catch
            {
                // Don't kill the process
            }
        }
    }

    private void WriteToAppenders()
    {
        var spinWait = new SpinWait();
        var flush = false;

        while (_isRunning || !_queue.IsEmpty)
        {
            if (TryToProcessQueue())
            {
                spinWait.Reset();
                flush = true;
                continue;
            }

            if (spinWait.NextSpinWillYield)
            {
                if (flush)
                {
                    FlushAppenders();
                    flush = false;
                    continue;
                }

                if (TryApplyConfigurationUpdate())
                    continue;
            }

            spinWait.SpinOnce();
        }

        TryApplyConfigurationUpdate(); // Make sure any new appenders are taken into account before disposal
        FlushAppenders();
    }

    private bool TryToProcessQueue()
    {
        if (!_queue.TryDequeue(out var logMessage))
            return false;

        try
        {
            var appenders = logMessage.Logger?.GetAppenders(logMessage.Level) ?? Array.Empty<Appender>();
            if (appenders.Length == 0)
                return true;

            _loggedMessage.SetMessage(logMessage);

            foreach (var appender in appenders)
                appender.InternalWriteMessage(_loggedMessage, _config);
        }
        finally
        {
            ReleaseAfterProcessing(logMessage);
        }

        return true;
    }

    private void FlushAppenders()
    {
        foreach (var appender in _appenders)
            appender.InternalFlush();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryApplyConfigurationUpdate()
    {
        var newConfig = Interlocked.Exchange(ref _nextConfig, null);
        if (newConfig is null)
            return false;

        ApplyConfigurationUpdate(newConfig);
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ApplyConfigurationUpdate(ZeroLogConfiguration newConfig)
    {
        _config = newConfig;

        // We could dispose the appenders that are no longer present in the new configuration right now,
        // but the user may want to add them back later, so their state needs to remain valid.

        var appenders = newConfig.GetAllAppenders();

        if (!appenders.SetEquals(_appenders))
        {
            appenders.UnionWith(_appenders);
            _appenders = appenders.ToArray();
        }

        _loggedMessage.UpdateConfiguration(newConfig);
    }
}
