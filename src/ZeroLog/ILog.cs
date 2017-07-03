using System;
using System.Collections.Generic;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public interface ILog
    {
        IList<IAppender> Appenders { get; }
        LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; }

        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }

        bool IsLevelEnabled(Level level);
        ILogEvent ForLevel(Level level);

        ILogEvent Debug();
        void Debug(string message);
        void Debug(string message, Exception ex);
        void DebugFormat<T0>(string format, T0 arg0);
        void DebugFormat<T0, T1>(string format, T0 arg0, T1 arg1);
        void DebugFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2);
        void DebugFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3);
        void DebugFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        void DebugFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        void DebugFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        void DebugFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

        ILogEvent Info();
        void Info(string message);
        void Info(string message, Exception ex);
        void InfoFormat<T0>(string format, T0 arg0);
        void InfoFormat<T0, T1>(string format, T0 arg0, T1 arg1);
        void InfoFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2);
        void InfoFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3);
        void InfoFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        void InfoFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        void InfoFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        void InfoFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

        ILogEvent Warn();
        void Warn(string message);
        void Warn(string message, Exception ex);
        void WarnFormat<T0>(string format, T0 arg0);
        void WarnFormat<T0, T1>(string format, T0 arg0, T1 arg1);
        void WarnFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2);
        void WarnFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3);
        void WarnFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        void WarnFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        void WarnFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        void WarnFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

        ILogEvent Error();
        void Error(string message);
        void Error(string message, Exception ex);
        void ErrorFormat<T0>(string format, T0 arg0);
        void ErrorFormat<T0, T1>(string format, T0 arg0, T1 arg1);
        void ErrorFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2);
        void ErrorFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3);
        void ErrorFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        void ErrorFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        void ErrorFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        void ErrorFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

        ILogEvent Fatal();
        void Fatal(string message);
        void Fatal(string message, Exception ex);
        void FatalFormat<T0>(string format, T0 arg0);
        void FatalFormat<T0, T1>(string format, T0 arg0, T1 arg1);
        void FatalFormat<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2);
        void FatalFormat<T0, T1, T2, T3>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3);
        void FatalFormat<T0, T1, T2, T3, T4>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        void FatalFormat<T0, T1, T2, T3, T4, T5>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        void FatalFormat<T0, T1, T2, T3, T4, T5, T6>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        void FatalFormat<T0, T1, T2, T3, T4, T5, T6, T7>(string format, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    }
}
