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
    private readonly LoggedMessage _loggedMessage;

    private ZeroLogConfiguration _config;
    private Appender[] _appenders;

    protected bool IsRunning { get; private set; }

    protected Runner(ZeroLogConfiguration config)
    {
        _config = config;

        var bufferSegmentProvider = new BufferSegmentProvider(config.LogMessagePoolSize, config.LogMessageBufferSize);
        _pool = new ObjectPool<LogMessage>(config.LogMessagePoolSize, () => new LogMessage(bufferSegmentProvider.GetSegment(), config.LogMessageStringCapacity));

        _appenders = config.GetAllAppenders().ToArray();

        _loggedMessage = new LoggedMessage(LogManager.OutputBufferSize, _config);

        IsRunning = true;
    }

    public void Dispose()
    {
        if (!IsRunning)
            return;

        IsRunning = false;

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

        if (!IsRunning)
            return LogMessage.Empty;

        return null;
    }

    public abstract void Submit(LogMessage message);

    public abstract void UpdateConfiguration(ZeroLogConfiguration newConfig);
    protected abstract void Stop();

    internal abstract void WaitUntilNewConfigurationIsApplied(); // For unit tests

    protected void ProcessMessage(LogMessage message)
    {
        try
        {
            var appenders = message.Logger?.GetAppenders(message.Level) ?? Array.Empty<Appender>();
            if (appenders.Length == 0)
                return;

            _loggedMessage.SetMessage(message);

            foreach (var appender in appenders)
                appender.InternalWriteMessage(_loggedMessage, _config);
        }
        finally
        {
            if (message.ReturnToPool && IsRunning)
            {
                message.ReturnToPool = false;
                _pool.Release(message);
            }
        }
    }

    protected void FlushAppenders()
    {
        foreach (var appender in _appenders)
            appender.InternalFlush();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected void ApplyConfigurationUpdate(ZeroLogConfiguration newConfig)
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

internal sealed class AsyncRunner : Runner
{
    private readonly ConcurrentQueue<LogMessage> _queue;
    private readonly Thread _thread;

    private ZeroLogConfiguration? _nextConfig;

    public AsyncRunner(ZeroLogConfiguration config)
        : base(config)
    {
        _queue = new ConcurrentQueue<LogMessage>(new ConcurrentQueueCapacityInitializer(config.LogMessagePoolSize));

        _thread = new Thread(WriteThread)
        {
            Name = $"{nameof(ZeroLog)}.{nameof(AsyncRunner)}",
            IsBackground = config.UseBackgroundThread
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

        while (IsRunning || !_queue.IsEmpty)
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

        ProcessMessage(logMessage);
        return true;
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
}

internal sealed class SyncRunner : Runner
{
    private readonly object _lock = new();

    public SyncRunner(ZeroLogConfiguration config)
        : base(config)
    {
    }

    public override void Submit(LogMessage message)
    {
        lock (_lock)
        {
            ProcessMessage(message);
            FlushAppenders();
        }
    }

    public override void UpdateConfiguration(ZeroLogConfiguration newConfig)
    {
        lock (_lock)
        {
            ApplyConfigurationUpdate(newConfig);
        }
    }

    protected override void Stop()
    {
    }

    internal override void WaitUntilNewConfigurationIsApplied()
    {
    }
}
