using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;
using System.Threading;
using System.Threading.Tasks;
using ZeroLog.AppenderResolvers;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public interface IAppenderResolver
    {
        IList<IAppender> Resolve(string name);
        void Initialize(Encoding encoding);
    }

    public class LogManager : IInternalLogManager
    {
        private static readonly IInternalLogManager _defaultLogManager = new NoopLogManager();
        private static IInternalLogManager _logManager = _defaultLogManager;

        private readonly ConcurrentQueue<IInternalLogEvent> _queue;
        private readonly ObjectPool<IInternalLogEvent> _pool;
        private readonly Encoding _encoding;

        private readonly LogEventPoolExhaustionStrategy _logEventPoolExhaustionStrategy;

        private readonly BufferSegmentProvider _bufferSegmentProvider;

        private readonly IAppenderResolver _appenderResolver;

        public bool IsRunning { get; set; }
        public Task WriteTask { get; }
        //public List<IAppender> Appenders { get; }

        internal LogManager(IAppenderResolver appenderResolver, LogManagerConfiguration configuration)
        {
            _appenderResolver = appenderResolver;
            Level = configuration.Level;

            _encoding = Encoding.Default;
            _logEventPoolExhaustionStrategy = configuration.LogEventPoolExhaustionStrategy;

            _queue = new ConcurrentQueue<IInternalLogEvent>(new ConcurrentQueueCapacityInitializer(configuration.LogEventQueueSize));

            _bufferSegmentProvider = new BufferSegmentProvider(configuration.LogEventQueueSize * configuration.LogEventBufferSize, configuration.LogEventBufferSize);
            _pool = new ObjectPool<IInternalLogEvent>(configuration.LogEventQueueSize, () => new LogEvent(_bufferSegmentProvider.GetSegment()));

            _appenderResolver.Initialize(_encoding);

            IsRunning = true;
            WriteTask = Task.Factory.StartNew(WriteToAppenders, TaskCreationOptions.LongRunning);
        }

        public Level Level { get; }

        public static ILogManager Initialize(IAppenderResolver appenderResolver, LogManagerConfiguration configuration)
        {
            if (_logManager != _defaultLogManager)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(appenderResolver, configuration);
            return _logManager;
        }

        public static ILogManager Initialize(IEnumerable<IAppender> appenders, LogManagerConfiguration configuration)
        {
            if (_logManager != _defaultLogManager)
                throw new ApplicationException("LogManager is already initialized");

            _logManager = new LogManager(new DummyAppenderResolver(appenders, Encoding.Default), configuration);
            return _logManager;
        }

        public static ILogManager Initialize(IEnumerable<IAppender> appenders, int logEventQueueSize = 1024, int logEventBufferSize = 128, Level level = Level.Finest)
        {
            return Initialize(appenders, new LogManagerConfiguration
            {
                LogEventQueueSize = logEventQueueSize,
                LogEventBufferSize = logEventBufferSize,
                Level = level,
            });
        }

        public static void Shutdown()
        {
            var logManager = _logManager;
            _logManager = _defaultLogManager;

            logManager?.Dispose();
        }

        public void Dispose()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            WriteTask.Wait(15000);

            // TODO
            //foreach (var appender in Appenders)
            //    appender.Close();

            _bufferSegmentProvider.Dispose();
        }

        public static ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public static ILog GetLogger(string name)
        {
            if (_logManager == null)
                throw new ApplicationException("LogManager is not yet initialized, please call LogManager.Initialize()");

            var log = _logManager.GetNewLog(_logManager, name);
            return log;
        }

        void IInternalLogManager.Enqueue(IInternalLogEvent logEvent)
        {
            _queue.Enqueue(logEvent);
        }

        ILog IInternalLogManager.GetNewLog(IInternalLogManager logManager, string name)
        {
            return new Log(logManager, name);
        }

        public IList<IAppender> ResolveAppenders(string name)
        {
            return _appenderResolver.Resolve(name);
        }

        public LogEventPoolExhaustionStrategy ResolveLogEventPoolExhaustionStrategy(string name)
        {
            return _logEventPoolExhaustionStrategy;
        }

        public Level ResolveLevel(string name)
        {
            return Level;
        }

        IInternalLogEvent IInternalLogManager.AllocateLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, IInternalLogEvent notifyPoolExhaustionLogEvent)
        {
            if (_pool.TryAcquire(out var logEvent))
                return logEvent;

            switch (logEventPoolExhaustionStrategy)
            {
                case LogEventPoolExhaustionStrategy.WaitForLogEvent:
                    return AcquireLogEvent();

                case LogEventPoolExhaustionStrategy.DropLogMessage:
                    return NoopLogEvent.Instance;

                default:
                    return notifyPoolExhaustionLogEvent;
            }
        }

        private IInternalLogEvent AcquireLogEvent()
        {
            var spinwait = new SpinWait();

            IInternalLogEvent logEvent;
            while (!_pool.TryAcquire(out logEvent))
            {
                spinwait.SpinOnce();
            }

            return logEvent;
        }

        private void WriteToAppenders()
        {
            var spinWait = new SpinWait();
            var stringBuffer = new StringBuffer(16 * 1024);
            var destination = new byte[16 * 1024];

            while (IsRunning || !_queue.IsEmpty)
            {
                if (TryToProcessQueue(stringBuffer, destination))
                    spinWait.Reset();
                else
                    spinWait.SpinOnce();
            }
        }

        private bool TryToProcessQueue(StringBuffer stringBuffer, byte[] destination)
        {
            if (!_queue.TryDequeue(out var logEvent))
                return false;
            if ((logEvent.Appenders?.Count ?? 0) <= 0)
                return true;

            try
            {
                FormatLogMessage(stringBuffer, logEvent);
            }
            catch (Exception)
            {
                FormatErrorMessage(stringBuffer, logEvent);
            }

            var bytesWritten = CopyStringBufferToByteArray(stringBuffer, destination);

            WriteMessageLogToAppenders(destination, logEvent, bytesWritten);

            if (logEvent.IsPooled)
                _pool.Release(logEvent);

            return true;
        }

        private static void FormatErrorMessage(StringBuffer stringBuffer, IInternalLogEvent logEvent)
        {
            stringBuffer.Clear();
            stringBuffer.Append("An error occured during formatting: ");

            logEvent.WriteToStringBufferUnformatted(stringBuffer);
        }

        private void WriteMessageLogToAppenders(byte[] destination, IInternalLogEvent logEvent, int bytesWritten)
        {
            var appenders = logEvent.Appenders;
            for (var i = 0; i < appenders.Count; i++)
            {
                var appender = appenders[i];
                // if (logEvent.Level >= Level) // TODO Check this ? log event should not be in queue if not > Level
                appender.WriteEvent(logEvent, destination, bytesWritten);
            }
        }

        private static void FormatLogMessage(StringBuffer stringBuffer, IInternalLogEvent logEvent)
        {
            stringBuffer.Clear();
            logEvent.WriteToStringBuffer(stringBuffer);
        }

        private unsafe int CopyStringBufferToByteArray(StringBuffer stringBuffer, byte[] destination)
        {
            int bytesWritten;
            fixed (byte* dest = destination)
            {
                bytesWritten = stringBuffer.CopyTo(dest, destination.Length, 0, stringBuffer.Count, _encoding);
            }
            return bytesWritten;
        }
    }
}
