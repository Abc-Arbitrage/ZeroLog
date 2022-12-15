using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog;

partial class LogManager
{
    private class AppenderThread
    {
        private readonly LogManager _logManager;
        private readonly ConcurrentQueue<LogMessage> _queue;
        private readonly Thread _thread;
        private readonly LoggedMessage _loggedMessage;

        private ZeroLogConfiguration _config;
        private ZeroLogConfiguration? _nextConfig;
        private Appender[] _appenders;

        public AppenderThread(LogManager logManager)
        {
            _logManager = logManager;
            _config = logManager._config;
            _queue = logManager._queue;

            _appenders = _config.GetAllAppenders().ToArray();
            _loggedMessage = new LoggedMessage(OutputBufferSize, _config);

            _thread = new Thread(WriteThread)
            {
                Name = $"{nameof(ZeroLog)}.{nameof(AppenderThread)}",
                IsBackground = _config.UseBackgroundThread
            };

            _thread.Start();
        }

        public void Join()
            => _thread.Join();

        public void DisposeAppenders()
        {
            foreach (var appender in _appenders)
                appender.Dispose();

            _appenders = Array.Empty<Appender>();
        }

        public void UpdateConfiguration(ZeroLogConfiguration config)
        {
            Volatile.Write(ref _nextConfig, config);
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

                    _logManager.Dispose();
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

            while (_logManager._isRunning || !_queue.IsEmpty)
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
                _logManager.ReleaseAfterProcessing(logMessage);
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

        public void WaitUntilNewConfigurationIsApplied()
        {
            while (Volatile.Read(ref _nextConfig) != null)
                Thread.Yield();
        }
    }
}
