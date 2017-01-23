using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;
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

        public LogEvent AllocateLogEvent()
        {
            throw new System.NotImplementedException();
        }

        public void Enqueue(LogEvent logEvent)
        {
            throw new System.NotImplementedException();
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

            private class NoopLogEvent : ILogEvent
            {
                public Level Level { get; }
                public DateTime Timestamp { get; }
                public int ThreadId { get; }
                public string Name { get; }
                public ILogEvent Append(string s)
                {
                    return this;
                }

                public unsafe ILogEvent Append(byte[] bytes, int length, Encoding encoding)
                {
                    return this;
                }

                public ILogEvent AppendAsciiString(byte[] bytes, int length)
                {
                    return this;
                }

                public unsafe ILogEvent AppendAsciiString(byte* bytes, int length)
                {
                    return this;
                }

                public ILogEvent Append(bool b)
                {
                    return this;
                }

                public ILogEvent Append(byte b)
                {
                    return this;
                }

                public ILogEvent Append(byte b, string format)
                {
                    return this;
                }

                public ILogEvent Append(char c)
                {
                    return this;
                }

                public ILogEvent Append(short s)
                {
                    return this;
                }

                public ILogEvent Append(short s, string format)
                {
                    return this;
                }

                public ILogEvent Append(int i)
                {
                    return this;
                }

                public ILogEvent Append(int i, string format)
                {
                    return this;
                }

                public ILogEvent Append(long l)
                {
                    return this;
                }

                public ILogEvent Append(long l, string format)
                {
                    return this;
                }

                public ILogEvent Append(float f)
                {
                    return this;
                }

                public ILogEvent Append(float f, string format)
                {
                    return this;
                }

                public ILogEvent Append(double d)
                {
                    return this;
                }

                public ILogEvent Append(double d, string format)
                {
                    return this;
                }

                public ILogEvent Append(decimal d)
                {
                    return this;
                }

                public ILogEvent Append(decimal d, string format)
                {
                    return this;
                }

                public ILogEvent Append(Guid g)
                {
                    return this;
                }

                public ILogEvent Append(Guid g, string format)
                {
                    return this;
                }

                public ILogEvent Append(DateTime dt)
                {
                    return this;
                }

                public ILogEvent Append(DateTime dt, string format)
                {
                    return this;
                }

                public ILogEvent Append(TimeSpan ts)
                {
                    return this;
                }

                public ILogEvent Append(TimeSpan ts, string format)
                {
                    return this;
                }

                public void Log()
                {
                }

                public unsafe void WriteToStringBuffer(StringBuffer stringBuffer)
                {
                }
            }
        }
    }
}