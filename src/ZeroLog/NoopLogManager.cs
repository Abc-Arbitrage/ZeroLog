using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZeroLog
{
    internal class NoopLogManager : IInternalLogManager
    {
        public Level Level { get; } = Level.Finest;
        public bool IsRunning { get; set; } = true;
        public Task WriteTask { get; }
        public List<IAppender> Appenders { get; }

        public NoopLogManager()
        {
            WriteTask = Task.FromResult(true);
            Appenders = new List<IAppender>(0);
        }

        public IInternalLogEvent AllocateLogEvent()
        {
            throw new NotImplementedException();
        }

        public void Enqueue(IInternalLogEvent logEvent)
        {
            throw new NotImplementedException();
        }

        public ILog GetNewLog(IInternalLogManager logManager, string name)
        {
            return new NoopLog();
        }

        internal class NoopLog : ILog
        {
            private static readonly ILogEvent _event = new NoopLogEvent();

            public bool IsDebugEnabled { get; } = false;
            public bool IsInfoEnabled { get; } = false;
            public bool IsWarnEnabled { get; } = false;
            public bool IsErrorEnabled { get; } = false;
            public bool IsFatalEnabled { get; } = false;

            public ILogEvent Debug()
            {
                return _event;
            }

            public void Debug(string message)
            {
            }

            public void Debug(string message, Exception ex)
            {
            }

            public void DebugFormat<T0>(string format, T0 arg0)
            {
            }

            public void DebugFormat<T0, T1>(string format, T0 arg0, T1 arg1)
            {
            }

            public void DebugFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
            {
            }

            public void DebugFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
            {
            }

            public void DebugFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
            }

            public void DebugFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
            }

            public void DebugFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
            }

            public void DebugFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
            }

            public ILogEvent Info()
            {
                return _event;
            }

            public void Info(string message)
            {
            }

            public void Info(string message, Exception ex)
            {
            }

            public void InfoFormat<T0>(string format, T0 arg0)
            {
            }

            public void InfoFormat<T0, T1>(string format, T0 arg0, T1 arg1)
            {
            }

            public void InfoFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
            {
            }

            public void InfoFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
            {
            }

            public void InfoFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
            }

            public void InfoFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
            }

            public void InfoFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
            }

            public void InfoFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
            }

            public ILogEvent Warn()
            {
                return _event;
            }

            public void Warn(string message)
            {
            }

            public void Warn(string message, Exception ex)
            {
            }

            public void WarnFormat<T0>(string format, T0 arg0)
            {
            }

            public void WarnFormat<T0, T1>(string format, T0 arg0, T1 arg1)
            {
            }

            public void WarnFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
            {
            }

            public void WarnFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
            {
            }

            public void WarnFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
            }

            public void WarnFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
            }

            public void WarnFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
            }

            public void WarnFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
            }

            public ILogEvent Error()
            {
                return _event;
            }

            public void Error(string message)
            {
            }

            public void Error(string message, Exception ex)
            {
            }

            public void ErrorFormat<T0>(string format, T0 arg0)
            {
            }

            public void ErrorFormat<T0, T1>(string format, T0 arg0, T1 arg1)
            {
            }

            public void ErrorFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
            {
            }

            public void ErrorFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
            {
            }

            public void ErrorFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
            }

            public void ErrorFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
            }

            public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
            }

            public void ErrorFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
            }

            public ILogEvent Fatal()
            {
                return _event;
            }

            public void Fatal(string message)
            {
            }

            public void Fatal(string message, Exception ex)
            {
            }

            public void FatalFormat<T0>(string format, T0 arg0)
            {
            }

            public void FatalFormat<T0, T1>(string format, T0 arg0, T1 arg1)
            {
            }

            public void FatalFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
            {
            }

            public void FatalFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
            {
            }

            public void FatalFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
            }

            public void FatalFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
            {
            }

            public void FatalFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
            {
            }

            public void FatalFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
            {
            }

            public ILogEvent ForLevel(Level level)
            {
                return _event;
            }
        }
    }
}
