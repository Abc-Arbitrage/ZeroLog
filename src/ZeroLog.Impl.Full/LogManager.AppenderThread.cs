using System;
using System.Collections.Concurrent;
using System.Linq;
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
        private readonly ZeroLogConfiguration _config;
        private readonly ConcurrentQueue<LogMessage> _queue;
        private readonly ObjectPool<LogMessage> _pool;
        private readonly Appender[] _appenders;
        private readonly Thread _thread;

        public AppenderThread(LogManager logManager)
        {
            _logManager = logManager;
            _config = logManager._config;
            _queue = logManager._queue;
            _pool = logManager._pool;

            _appenders = _config.GetAllAppenders().ToArray();

            _thread = new Thread(WriteThread)
            {
                Name = $"{nameof(ZeroLog)}.{nameof(AppenderThread)}"
            };

            _thread.Start();
        }

        public void Join()
            => _thread.Join();

        public void DisposeAppenders()
        {
            foreach (var appender in _appenders)
                appender.Dispose();
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
            var loggedMessage = new LoggedMessage(OutputBufferSize, _config);
            var flush = false;

            while (_logManager._isRunning || !_queue.IsEmpty)
            {
                if (TryToProcessQueue(loggedMessage))
                {
                    spinWait.Reset();
                    flush = true;
                    continue;
                }

                if (flush && spinWait.NextSpinWillYield)
                {
                    FlushAppenders();
                    flush = false;
                }
                else
                {
                    spinWait.SpinOnce();
                }
            }

            FlushAppenders();
        }

        private bool TryToProcessQueue(LoggedMessage loggedMessage)
        {
            if (!_queue.TryDequeue(out var logMessage))
                return false;

            try
            {
                var appenders = logMessage.Logger?.GetAppenders(logMessage.Level) ?? Array.Empty<Appender>();
                if (appenders.Length == 0)
                    return true;

                loggedMessage.SetMessage(logMessage);

                foreach (var appender in appenders)
                    appender.InternalWriteMessage(loggedMessage, _config);
            }
            finally
            {
                if (logMessage.IsPooled && _logManager._isRunning)
                    _pool.Release(logMessage);
            }

            return true;
        }

        private void FlushAppenders()
        {
            foreach (var appender in _appenders)
                appender.InternalFlush();
        }
    }
}
